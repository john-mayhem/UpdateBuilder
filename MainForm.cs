using System;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;
using Microsoft.Extensions.Configuration;
using UpdateBuilder.Models;
using UpdateBuilder.Services;
using Serilog;

namespace UpdateBuilder
{
    public partial class MainForm : Form
    {
        private readonly UpdatePackageBuilder _packageBuilder;
        private readonly BuildConfiguration _config;
        private CancellationTokenSource? _cancellationTokenSource;

        public MainForm()
        {
            InitializeComponent();
            _packageBuilder = new UpdatePackageBuilder();
            _config = LoadConfiguration();
        }

        private BuildConfiguration LoadConfiguration()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

                if (!File.Exists(configPath))
                {
                    Log.Warning("Configuration file not found, using defaults");
                    return new BuildConfiguration();
                }

                var configuration = new ConfigurationBuilder()
                    .AddJsonFile(configPath, optional: true)
                    .Build();

                var config = new BuildConfiguration
                {
                    ExcludedExtensions = configuration.GetSection("ExcludedExtensions").Get<List<string>>() ?? [],
                    ExcludedFolders = configuration.GetSection("ExcludedFolders").Get<List<string>>() ?? [],
                    MaxProductNameLength = int.TryParse(configuration["MaxProductNameLength"], out int maxLen) ? maxLen : 32
                };

                // Set default compression level from config
                if (Enum.TryParse<CompressionLevel>(configuration["DefaultCompressionLevel"], out var compressionLevel))
                {
                    cmbCompressionLevel.SelectedItem = compressionLevel.ToString();
                }

                Log.Information("Configuration loaded successfully");
                return config;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load configuration, using defaults");
                return new BuildConfiguration();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Initialize compression level dropdown
            cmbCompressionLevel.Items.Clear();
            cmbCompressionLevel.Items.Add(CompressionLevel.Optimal.ToString());
            cmbCompressionLevel.Items.Add(CompressionLevel.Fastest.ToString());
            cmbCompressionLevel.Items.Add(CompressionLevel.NoCompression.ToString());
            cmbCompressionLevel.SelectedIndex = 0;

            UpdateStartButtonState();
        }

        private void BtnBrowseInput_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtInputDirectory.Text = dialog.SelectedPath;
                UpdateStartButtonState();
            }
        }

        private void BtnBrowseDestination_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtDestinationFolder.Text = dialog.SelectedPath;
                UpdateVersionFromExistingFilelist(dialog.SelectedPath);
                UpdateStartButtonState();
            }
        }

        private void UpdateStartButtonState()
        {
            btnProcess.Enabled = !string.IsNullOrWhiteSpace(txtInputDirectory.Text) &&
                                 !string.IsNullOrWhiteSpace(txtDestinationFolder.Text) &&
                                 _cancellationTokenSource == null;
        }

        private async void BtnProcess_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                btnProcess.Enabled = false;
                btnCancel.Enabled = true;

                Log.Information("Starting build process");

                var compressionLevel = Enum.Parse<CompressionLevel>(cmbCompressionLevel.SelectedItem?.ToString() ?? "Optimal");
                var progress = new Progress<BuildProgress>(UpdateProgress);

                string version = chkAutoIncrement.Checked
                    ? await UpdatePackageBuilder.GetNextVersionAsync(txtDestinationFolder.Text)
                    : txtNewVersion.Text;

                var package = await UpdatePackageBuilder.BuildPackageAsync(
                    txtInputDirectory.Text,
                    txtDestinationFolder.Text,
                    txtProductName.Text,
                    version,
                    compressionLevel,
                    _config,
                    progress,
                    _cancellationTokenSource.Token);

                txtNewVersion.Text = package.Version;
                MessageBox.Show(
                    $"Package built successfully!\n\n" +
                    $"Files: {package.Files.Count}\n" +
                    $"Packed Size: {FormatBytes(package.TotalPackedSize)}\n" +
                    $"Unpacked Size: {FormatBytes(package.TotalUnpackedSize)}\n" +
                    $"Compression Ratio: {(package.TotalUnpackedSize > 0 ? (100.0 - (package.TotalPackedSize * 100.0 / package.TotalUnpackedSize)) : 0):F1}%",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                Log.Information("Build process completed successfully");
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Operation cancelled by user.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Log.Information("Build process cancelled by user");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log.Error(ex, "Build process failed");
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                btnProcess.Enabled = true;
                btnCancel.Enabled = false;
                overallProgressBar.Value = 0;
                lblCurrentFile.Text = string.Empty;
                UpdateStartButtonState();
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (_cancellationTokenSource != null)
            {
                Log.Information("Cancellation requested");
                _cancellationTokenSource.Cancel();
                btnCancel.Enabled = false;
            }
        }

        private bool ValidateInputs()
        {
            if (!Directory.Exists(txtInputDirectory.Text))
            {
                MessageBox.Show("Input directory does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!Directory.Exists(txtDestinationFolder.Text))
            {
                MessageBox.Show("Destination folder does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Product name is required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (txtProductName.Text.Length > _config.MaxProductNameLength)
            {
                MessageBox.Show(
                    $"Product name exceeds maximum length of {_config.MaxProductNameLength} characters.\n" +
                    $"Current length: {txtProductName.Text.Length}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            if (!chkAutoIncrement.Checked && !Version.TryParse(txtNewVersion.Text, out _))
            {
                MessageBox.Show("Invalid version format. Please use format like: 1.0.0", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                string testFile = Path.Combine(txtDestinationFolder.Text, "test.txt");
                File.Create(testFile).Close();
                File.Delete(testFile);
            }
            catch
            {
                MessageBox.Show("No write permission in the destination folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void UpdateProgress(BuildProgress progress)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateProgress(progress)));
                return;
            }

            overallProgressBar.Value = Math.Min(progress.PercentComplete, overallProgressBar.Maximum);
            lblCurrentFile.Text = $"Processing: {progress.CurrentFile}";
        }

        private async void UpdateVersionFromExistingFilelist(string destinationFolder)
        {
            string fileListPath = Path.Combine(destinationFolder, "filelist.bin");
            if (File.Exists(fileListPath))
            {
                _ = new FileListManager();
                var package = await FileListManager.ReadFileListAsync(fileListPath);

                if (package != null)
                {
                    txtProductName.Text = package.ProductName;
                    txtCurrentVersion.Text = package.Version;
                    txtNewVersion.Text = package.Version;
                    chkAutoIncrement.Enabled = true;
                }
            }
            else
            {
                txtCurrentVersion.Text = string.Empty;
                txtNewVersion.Text = "0.0.1";
                chkAutoIncrement.Checked = false;
                chkAutoIncrement.Enabled = false;
            }
            UpdateNewVersionState();
        }

        private void ChkAutoIncrement_CheckedChanged(object sender, EventArgs e)
        {
            UpdateNewVersionState();
        }

        private void UpdateNewVersionState()
        {
            if (chkAutoIncrement.Checked)
            {
                txtNewVersion.ReadOnly = true;
                if (Version.TryParse(txtCurrentVersion.Text, out Version? currentVersion))
                {
                    if (currentVersion.Build == -1)
                    {
                        txtNewVersion.Text = new Version(currentVersion.Major, currentVersion.Minor + 1).ToString();
                    }
                    else
                    {
                        txtNewVersion.Text = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build + 1).ToString();
                    }
                }
            }
            else
            {
                txtNewVersion.ReadOnly = false;
                if (string.IsNullOrEmpty(txtCurrentVersion.Text))
                {
                    txtNewVersion.Text = "0.0.1";
                }
                else
                {
                    txtNewVersion.Text = txtCurrentVersion.Text;
                }
            }
        }

        private static string FormatBytes(long bytes)
        {
            string[] sizes = ["B", "KB", "MB", "GB"];
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
