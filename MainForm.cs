using System;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using System.Collections.Concurrent;

namespace UpdateBuilder
{
    public partial class MainForm : Form
    {
        private readonly BackgroundWorker _backgroundWorker;

        public MainForm()
        {
            InitializeComponent();
            _backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _backgroundWorker.DoWork += BackgroundWorker_DoWork;
            _backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            _backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
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
                                 !string.IsNullOrWhiteSpace(txtDestinationFolder.Text);
        }

        private async void BtnProcess_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            btnProcess.Enabled = false;
            try
            {
                await ProcessFilesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage($"Error: {ex}");
            }
            finally
            {
                btnProcess.Enabled = true;
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

            try
            {
                File.Create(Path.Combine(txtDestinationFolder.Text, "test.txt")).Close();
                File.Delete(Path.Combine(txtDestinationFolder.Text, "test.txt"));
            }
            catch
            {
                MessageBox.Show("No write permission in the destination folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private async Task ProcessFilesAsync()
        {
            string destinationFolder = txtDestinationFolder.Text;
            ClearOutputDirectory(destinationFolder);
            string inputDirectory = txtInputDirectory.Text;
            string fileListPath = Path.Combine(destinationFolder, "filelist.bin");

            string[] files = Directory.GetFiles(inputDirectory, "*", SearchOption.AllDirectories);

            var fileInfo = new ConcurrentDictionary<string, (string Hash, long Size)>();
            int totalFiles = files.Length;
            int filesProcessed = 0;
            long totalUnpackedSize = 0;
            long totalPackedSize = 0;

            await Task.Run(async () =>
            {
                await Parallel.ForEachAsync(files, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, async (file, token) =>
                {
                    string relativePath = file[(inputDirectory.Length + 1)..];
                    string hash = await FileUtils.CalculateFileHashAsync(file);
                    long fileSize = new FileInfo(file).Length;
                    fileInfo[relativePath.Replace("\\", "/")] = (hash, fileSize);

                    string archivePath = Path.Combine(destinationFolder, relativePath.Replace("\\", "/") + ".zip");
                    await FileUtils.ArchiveFileAsync(file, archivePath);

                    Interlocked.Add(ref totalUnpackedSize, fileSize);
                    Interlocked.Add(ref totalPackedSize, new FileInfo(archivePath).Length);

                    int processed = Interlocked.Increment(ref filesProcessed);
                    int progress = (int)((double)processed / totalFiles * 100);
                    ReportProgress(progress, progress);
                });
            });

            string updateVersion = GetUpdateVersion(destinationFolder);
            txtNewVersion.Text = updateVersion; // Update this line

            var fileListContent = new StringBuilder();

            // Add system information
            fileListContent.AppendLine(DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss"));
            fileListContent.AppendLine(txtProductName.Text[..Math.Min(txtProductName.Text.Length, 32)]);
            fileListContent.AppendLine(updateVersion);
            fileListContent.AppendLine($"{totalPackedSize}/{totalUnpackedSize}");
            for (int i = 4; i < 10; i++)
            {
                fileListContent.AppendLine("");
            }

            // Add file information
            foreach (var kvp in fileInfo.OrderBy(kvp => kvp.Key))
            {
                fileListContent.AppendLine($"{kvp.Value.Hash} {kvp.Value.Size,20} {kvp.Key}");
            }

            // Encode and write to file
            byte[] encodedContent = EncodeFileList(fileListContent.ToString());
            await File.WriteAllBytesAsync(fileListPath, encodedContent);

            LogMessage("Process completed successfully.");
        }

        private string GetUpdateVersion(string destinationFolder)
        {
            if (!string.IsNullOrWhiteSpace(txtNewVersion.Text))
            {
                return txtNewVersion.Text;
            }

            string existingFileList = Path.Combine(destinationFolder, "filelist.bin");
            if (File.Exists(existingFileList))
            {
                byte[] encodedContent = File.ReadAllBytes(existingFileList);
                string decodedContent = DecodeFileList(encodedContent);
                string[] lines = decodedContent.Split('\n');
                if (lines.Length > 2 && Version.TryParse(lines[2].Trim(), out Version? existingVersion) && existingVersion != null)
                {
                    return new Version(existingVersion!.Major, existingVersion.Minor, existingVersion.Build + 1).ToString();
                }
            }

            return "0.0.1";
        }

        private static byte[] EncodeFileList(string content)
        {
            // Simple XOR encoding for demonstration. Replace with a more secure method in production.
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ 0xAA);
            }
            return bytes;
        }

        private static string DecodeFileList(byte[] encodedContent)
        {
            // Corresponding decoding method
            for (int i = 0; i < encodedContent.Length; i++)
            {
                encodedContent[i] = (byte)(encodedContent[i] ^ 0xAA);
            }
            return Encoding.UTF8.GetString(encodedContent);
        }

        private void ReportProgress(int perFileProgress, int overallProgress)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ReportProgress(perFileProgress, overallProgress)));
                return;
            }

            perFileProgressBar.Value = Math.Min(perFileProgress, perFileProgressBar.Maximum);
            overallProgressBar.Value = Math.Min(overallProgress, overallProgressBar.Maximum);
        }

        private static void LogMessage(string message)
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
            string logMessage = $"{DateTime.Now}: {message}";
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }

        // BackgroundWorker event handlers (if needed for future use)
        private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e) { }
        private void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e) { }
        private void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e) { }


        private void UpdateVersionFromExistingFilelist(string destinationFolder)
        {
            string fileListPath = Path.Combine(destinationFolder, "filelist.bin");
            if (File.Exists(fileListPath))
            {
                byte[] encodedContent = File.ReadAllBytes(fileListPath);
                string decodedContent = DecodeFileList(encodedContent);
                string[] lines = decodedContent.Split('\n');
                if (lines.Length > 2)
                {
                    txtProductName.Text = lines[1].Trim();  // Set product name
                    string currentVersion = lines[2].Trim();
                    txtCurrentVersion.Text = currentVersion;
                    txtNewVersion.Text = currentVersion;
                    chkAutoIncrement.Enabled = true;
                }
            }
            else
            {
                txtCurrentVersion.Text = string.Empty;
                txtNewVersion.Text = "0.1";
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
                    if (currentVersion.Build == -1)  // This means it's a 2-part version (e.g., 0.2)
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
                    txtNewVersion.Text = "0.1";
                }
                else
                {
                    txtNewVersion.Text = txtCurrentVersion.Text;
                }
            }
        }


        private static void ClearOutputDirectory(string directory)
        {
            foreach (string file in Directory.GetFiles(directory))
            {
                if (!string.Equals(Path.GetFileName(file), "filelist.bin", StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(file);
                }
            }
            foreach (string dir in Directory.GetDirectories(directory))
            {
                Directory.Delete(dir, true);
            }
        }
    }

}