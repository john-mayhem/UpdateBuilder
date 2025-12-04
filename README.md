# UpdateBuilder

A Windows Forms application for creating structured update packages with individual file archives and manifests. Designed for software distribution systems that require per-file SHA256 verification and selective updates.

## Features

- **Per-File Archiving** - Creates individual `.zip` archives for each file, enabling selective updates
- **SHA256 Verification** - Calculates and stores hash values for integrity checking
- **Archive Validation** - Automatically validates each archive after creation
- **Compression Options** - Choose between Optimal, Fastest, or No Compression
- **Smart Versioning** - Auto-increment or manual version control
- **Exclusion Patterns** - Filter files by extension and folder names
- **Parallel Processing** - Multi-threaded for maximum performance
- **Comprehensive Logging** - Detailed Serilog file logging for troubleshooting
- **Cancellation Support** - Stop long-running operations at any time
- **High DPI Support** - Works perfectly with display scaling (150%, 200%, etc.)

## How It Works

UpdateBuilder takes a source directory and creates:

1. **Individual Archives** - Each file is compressed into its own `.zip` archive
2. **Manifest File** (`filelist.bin`) - XOR-encoded manifest containing:
   - Build timestamp
   - Product name and version
   - Size statistics (packed/unpacked)
   - File entries (SHA256 hash, size, relative path)

## Usage

### Basic Workflow

1. **Select Input Folder** - Choose the directory containing files to package
2. **Select Output Folder** - Choose where to create the update package
3. **Configure Settings**:
   - Product Name (max 32 characters)
   - Version (manual or auto-increment)
   - Compression Level
4. **Start** - Process files and create package
5. **Verify** - Check the log file in `logs/` if needed

### Configuration

Edit `appsettings.json` to customize:

```json
{
  "ExcludedExtensions": [".pdb", ".log", ".tmp"],
  "ExcludedFolders": [".git", ".vs", "bin", "obj", "node_modules"],
  "MaxProductNameLength": 32,
  "DefaultCompressionLevel": "Optimal"
}
```

**Exclusion Patterns:**
- `ExcludedExtensions` - Skip files with these extensions
- `ExcludedFolders` - Skip any folder with these names (recursive)

**Compression Levels:**
- `Optimal` - Best compression ratio (default)
- `Fastest` - Faster processing, larger files
- `NoCompression` - Store only, no compression

## Output Structure

```
OutputFolder/
├── filelist.bin                    # Manifest file
├── SubFolder/
│   ├── file1.txt.zip              # Individual archives
│   └── file2.dat.zip
└── file3.exe.zip
```

### Manifest Format (`filelist.bin`)

The manifest is XOR-encoded (key: 0xAA) with the following structure:

```
Line 1:  Timestamp (dd.MM.yyyy - HH:mm:ss)
Line 2:  Product name
Line 3:  Version
Line 4:  TotalPackedSize/TotalUnpackedSize
Lines 5-10: Reserved (empty)
Lines 11+: <SHA256_hash> <file_size> <relative_path>
```

## Architecture

```
UpdateBuilder/
├── Models/              # Data structures
│   ├── FileListEntry.cs
│   ├── UpdatePackage.cs
│   └── BuildConfiguration.cs
├── Services/            # Business logic
│   ├── FileHasher.cs
│   ├── FileArchiver.cs
│   ├── ValidationService.cs
│   ├── FileListManager.cs
│   └── UpdatePackageBuilder.cs
├── Utilities/           # Helper classes
│   └── ExclusionMatcher.cs
└── MainForm.cs          # UI layer
```

## Requirements

- .NET 8.0 Windows
- Windows Forms

## Dependencies

- **Serilog** - File logging
- **Serilog.Sinks.File** - Log file output
- **Microsoft.Extensions.Configuration** - JSON configuration

## Building

```bash
dotnet build
```

## Logging

Logs are written to `logs/updatebuilder-<date>.txt` in the application directory.

Log levels:
- **Debug** - Detailed operation info (hashing, archiving, validation)
- **Information** - Build process milestones
- **Warning** - Non-fatal issues (missing config, validation warnings)
- **Error** - Failures and exceptions
- **Fatal** - Application-level crashes

## Use Case

This tool is designed for update systems where:
- Client applications verify files using SHA256 hashes
- Updates are delivered as individual file archives
- Selective updates are required (only changed files)
- A manifest file coordinates the update process

## License

MIT License - See [LICENSE.txt](LICENSE.txt)

## Author

John Mayhem (c) 2023

## Repository

https://github.com/john-mayhem/UpdateBuilder
