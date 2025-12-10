using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using org.bidib.Net.Core.Models.Common;
using org.bidib.Net.Core.Services.Interfaces;
using org.bidib.Net.DecoderDB.Models.Decoder;

namespace org.bidib.DecocderDB.RepoGenerator.Data;

public class DecoderRepository(
    IIoService ioService,
    IJsonService jsonService,
    ILogger<DecoderRepository> logger)
    : IDecoderRepository
{
    private readonly Dictionary<DecoderDefinition, FileInfo> decoders = [];
    private readonly Dictionary<Image, FileInfo> images = [];

    public ICollection<DecoderDefinition> Decoders => decoders.Keys;
    public ICollection<Image> Images => images.Keys;

    public void Reload(string path)
    {
        decoders.Clear();
        LoadDecoders(path);
    }

    public (string sha1, long size) GetFileInfo(DecoderDefinition decoder)
    {
        FileInfo fileInfo = null;
        if (decoders.TryGetValue(decoder, out var decFileInfo))
        {
            fileInfo = decFileInfo;
        }

        if (fileInfo == null)
        {
            return (string.Empty, 0);
        }

        return GetFileInfo(fileInfo);
    }

    public (string sha1, long size) GetFileInfo(Image image)
    {
        return !images.TryGetValue(image, out var fileInfo) ? (string.Empty, 0) : GetFileInfo(fileInfo);
    }

    private (string sha1, long size) GetFileInfo(FileInfo fileInfo)
    {
        var sha1 = ioService.GetSha1(fileInfo.FullName);
        return (sha1, fileInfo.Length);
    }

    private void LoadDecoders(string repoPath)
    {
        var decoderPath = ioService.GetPath(repoPath, "decoder");

        if (!ioService.DirectoryExists(decoderPath))
        {
            logger.LogWarning("directory {Path} does not exist", decoderPath);
            return;
        }

        var files = new List<string>();

        var directories = ioService.GetDirectories(decoderPath);
        logger.LogInformation("{Directories} directories found", directories.Length);

        foreach (var manufacturerDirectory in directories)
        {
            var subFiles = ioService.GetFiles(manufacturerDirectory, "*.json");
            files.AddRange(subFiles);
            logger.LogInformation("{SubFiles} files found in {ManufacturerDirectory}", subFiles.Length, manufacturerDirectory);
        }

        logger.LogInformation("{Files} total files", files.Count);
        foreach (var file in files)
        {
            LoadFromFile(file);
        }

        logger.LogInformation("{Decoders} decoders loaded", Decoders.Count);
    }

    private void LoadFromFile(string file)
    {
        logger.LogInformation("Processing {File}", file);
        var decoder = jsonService.LoadFromFile<DecoderDefinition>(file);

        if (decoder == null)
        {
            logger.LogWarning("Decoder definition could not be parsed! Skipping ...");
            return;
        }

        decoder.SourceFile = ioService.GetFileName(file);
        decoders.Add(decoder, new FileInfo(file));

        if (decoder.Decoder.Images == null) return;
        foreach (var image in decoder.Decoder.Images)
        {
            var directory = ioService.GetDirectory(file);
            image.Source = ioService.GetPath(directory, "images", image.Name);
            images.Add(image, new FileInfo(image.Source));
        }
    }
}