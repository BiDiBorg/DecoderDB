using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using org.bidib.Net.Core.Services.Interfaces;
using org.bidib.Net.DecoderDB.Models.Firmware;

namespace org.bidib.DecocderDB.RepoGenerator.Data;

public class FirmwareRepository(
    IIoService ioService,
    IJsonService jsonService,
    ILogger<FirmwareRepository> logger) : IFirmwareRepository
{
    private readonly Dictionary<FirmwareDefinition, FileInfo> firmwares = [];

    public ICollection<FirmwareDefinition> Firmwares => firmwares.Keys;

    public void Reload(string path)
    {
        firmwares.Clear();
        LoadFirmwares(path);
    }

    public (string sha1, long size) GetFileInfo(FirmwareDefinition firmware)
    {
        FileInfo fileInfo = null;

        if (firmwares.TryGetValue(firmware, out var decFileInfo))
        {
            fileInfo = decFileInfo;
        }

        if (fileInfo == null)
        {
            return (string.Empty, 0);
        }

        var sha1 = ioService.GetSha1(fileInfo.FullName);
        return (sha1, fileInfo.Length);
    }

    private void LoadFirmwares(string repoPath)
    {
        var firmwarePath = ioService.GetPath(repoPath, "firmware");

        if (!ioService.DirectoryExists(firmwarePath))
        {
            logger.LogWarning("Directory {Path} does not exist", firmwarePath);
            return;
        }

        var files = new List<string>();

        var directories = ioService.GetDirectories(firmwarePath);
        logger.LogInformation("{Directories} directories found", directories.Length);

        foreach (var manufacturerDirectory in directories)
        {
            var jsonFiles = ioService.GetFiles(manufacturerDirectory, "*.json");
            files.AddRange(jsonFiles);
            logger.LogInformation("{Files} files found in {Directory}", jsonFiles.Length, manufacturerDirectory);
        }

        logger.LogInformation("{Files} total files", files.Count);
        foreach (var file in files)
        {
            LoadFromFile(file);
        }

        logger.LogInformation("{Files} firmwares loaded", Firmwares.Count);
    }

    private void LoadFromFile(string file)
    {
        logger.LogInformation("Processing {File}", file);
        var firmware = jsonService.LoadFromFile<FirmwareDefinition>(file);

        if (firmware == null)
        {
            logger.LogWarning("Firmware could not be parsed! Skipping ...");
            return;
        }
        else
        {
            firmware.SourceFile = ioService.GetFileName(file);
            firmwares.Add(firmware, new FileInfo(file));
        }
    }
}