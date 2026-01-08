using Microsoft.Extensions.Logging;
using org.bidib.Net.Core.Services.Interfaces;
using org.bidib.Net.DecoderDB.Models.Decoder;
using org.bidib.Net.DecoderDB.Models.Detection;
using org.bidib.Net.DecoderDB.Models.Firmware;
using org.bidib.Net.DecoderDB.Models.Manufacturers;
using org.bidib.Net.DecoderDB.Models.Sync;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DecoderInfo = org.bidib.Net.DecoderDB.Models.Sync.DecoderInfo;

namespace org.bidib.DecocderDB.RepoGenerator.Data;

public class RepositoryGenerator(
    IFirmwareRepository firmwareRepository,
    IDecoderRepository decoderRepository,
    IIoService ioService,
    IJsonService jsonService,
    ILogger<RepositoryGenerator> logger)
    : IRepositoryGenerator
{

    private const string ShortDateFormat = "yyyy-MM-dd";

    public void Generate(string outputPath, string baseUri, string repositoryPath)
    {
        var response = new DecoderDbInfo { Version = 7 };

        if (string.IsNullOrEmpty(baseUri))
        {
            var di = new DirectoryInfo(outputPath);
            baseUri = di.FullName;
        }

        response.DecoderDetections = GetDecoderDetection(repositoryPath, baseUri);
        response.Manufacturers = GetManufacturers(repositoryPath, baseUri);

        var (decoderInfos, decoderDetails, imageInfos) = GetDecoderInfos(baseUri);
        var decFirmwareInfo = GetFirmwareInfo(baseUri);
        response.Decoders = decoderInfos;
        response.Images = imageInfos;
        response.Firmware = decFirmwareInfo;


        WriteData(outputPath, response, decoderDetails);
    }

    private ManufacturersInfo GetManufacturers(string repoPath, string baseUri)
    {
        var fileName = "Manufacturers.json";
        var info = new ManufacturersInfo
        {
            FileName = fileName,

        };

        var filePath = ioService.GetPath(repoPath, fileName);
        if (!ioService.FileExists(filePath))
        {
            return info;
        }

        var fileInfo = new FileInfo(filePath);
        info.FileSize = fileInfo.Length;
        info.Link = new Uri($"{baseUri}/{fileName}", UriKind.RelativeOrAbsolute);
        info.Sha1 = ioService.GetSha1(filePath);

        var detection = jsonService.LoadFromFile<ManufacturersList>(filePath);
        if (detection == null)
        {
            return info;
        }

        info.LastUpdate = detection.Version.LastUpdate;
        info.NmraListDate = detection.Version.ListDate.ToString(ShortDateFormat, CultureInfo.CurrentCulture);
        return info;
    }

    private BaseInfo GetDecoderDetection(string repoPath, string baseUri)
    {
        var fileName = "DecoderDetection.json";
        var info = new BaseInfo
        {
            FileName = fileName
        };

        var filePath = ioService.GetPath(repoPath, fileName);
        if (!ioService.FileExists(filePath))
        {
            return info;
        }

        var fileInfo = new FileInfo(filePath);
        info.FileSize = fileInfo.Length;
        info.Link = new Uri($"{baseUri}/{fileName}", UriKind.RelativeOrAbsolute);
        info.Sha1 = ioService.GetSha1(filePath);

        var detection = jsonService.LoadFromFile<DecoderDetection>(filePath);
        if (detection == null)
        {
            return info;
        }

        info.LastUpdate = detection.Version.LastUpdate;
        return info;
    }

    private (DecoderInfo[] decoderInfos, DecoderDetails[] decoderDetails, ImageInfo[] imageInfos) GetDecoderInfos(string baseUri)
    {
        var decoderInfos = new List<DecoderInfo>();
        var decoderDetails = new List<DecoderDetails>();
        var imageInfos = new List<ImageInfo>();

        foreach (var definition in decoderRepository.Decoders)
        {
            var manufacturerPath = $"{definition.Decoder.ManufacturerId}";
            if (definition.Decoder.ManufacturerExtendedId > 0)
            {
                manufacturerPath += $"_{definition.Decoder.ManufacturerExtendedId}";
            }

            DecoderInfo decoderInfo = MapDecoderInfo(baseUri, definition, manufacturerPath);

            decoderInfos.Add(decoderInfo);

            var fileName = definition.SourceFile;

            var details = new DecoderDetails
            {
                Name = definition.Decoder.Name,
                Type = definition.Decoder.Type,
                ManufacturerId = definition.Decoder.ManufacturerId,
                ManufacturerExtendedId = definition.Decoder.ManufacturerExtendedId,
                FileName = decoderInfo.FileName,
                Link = decoderInfo.Link,
                Sha1 = decoderInfo.Sha1,
                LastUpdate = definition.Version.LastUpdate,
                FileSize = decoderInfo.FileSize,
                Width = definition.Decoder.Specifications?.Dimensions?.Width,
                Height = definition.Decoder.Specifications?.Dimensions?.Height,
                Length = definition.Decoder.Specifications?.Dimensions?.Length,
                MaxTotalCurrent = definition.Decoder.Specifications?.Electrical?.MaxTotalCurrent,
                MaxVoltage = definition.Decoder.Specifications?.Electrical?.MaxVoltage,
                FunctionOutputs = definition.Decoder.Specifications?.Electrical?.FunctionOutputs
            };

            decoderDetails.Add(details);

            if (definition.Decoder.Images == null)
            {
                continue;
            }

            foreach (var image in definition.Decoder.Images)
            {
                var (imgSha1, imgSize) = decoderRepository.GetFileInfo(image);

                var imageInfo = new ImageInfo
                {
                    Name = image.Name,
                    ManufacturerId = definition.Decoder.ManufacturerId,
                    ManufacturerExtendedId = definition.Decoder.ManufacturerExtendedId,
                    FileName = image.Name,
                    Link = new Uri($"{baseUri}/decoder/{manufacturerPath}/images/{image.Name}",
                        UriKind.RelativeOrAbsolute),
                    Sha1 = imgSha1,
                    LastUpdate = image.LastModified,
                    FileSize = imgSize
                };

                imageInfos.Add(imageInfo);
            }
        }


        return (decoderInfos.OrderBy(x => x.FileName).ToArray(),
        decoderDetails.OrderBy(x => x.FileName).ToArray(),
        imageInfos.OrderBy(x => x.Name).ToArray()); 
    }

    private DecoderInfo MapDecoderInfo(string baseUri, DecoderDefinition definition, string manufacturerPath)
    {
        var (sha1, size) = decoderRepository.GetFileInfo(definition);

        return new DecoderInfo
        {
            Name = definition.Decoder.Name,
            ManufacturerId = definition.Decoder.ManufacturerId,
            ManufacturerExtendedId = definition.Decoder.ManufacturerExtendedId,
            FileName = definition.SourceFile,
            Link = new Uri($"{baseUri}/decoder/{manufacturerPath}/{definition.SourceFile}", UriKind.RelativeOrAbsolute),
            Sha1 = sha1,
            LastUpdate = definition.Version.LastUpdate,
            Created = definition.Version.Created.ToString(ShortDateFormat, CultureInfo.CurrentCulture),
            FileSize = size
        };
    }

    private FirmwareInfo[] GetFirmwareInfo(string baseUri)
    {
        var firmwareInfos = new List<FirmwareInfo>();

        foreach (var firmware in firmwareRepository.Firmwares)
        {
            FirmwareInfo firmwareInfo = MapFirwareInfo(baseUri, firmware);

            firmwareInfos.Add(firmwareInfo);
        }

        return [.. firmwareInfos.OrderBy(x => x.FileName)];
    }

    private FirmwareInfo MapFirwareInfo(string baseUri, FirmwareDefinition firmware)
    {
        var (sha1, size) = firmwareRepository.GetFileInfo(firmware);

        var manufacturerPath = $"{firmware.Firmware.ManufacturerId}";
        if (firmware.Firmware.ManufacturerExtendedId > 0)
        {
            manufacturerPath += $"_{firmware.Firmware.ManufacturerExtendedId}";
        }

        var firmwareInfo = new FirmwareInfo
        {
            ManufacturerId = firmware.Firmware.ManufacturerId,
            ManufacturerExtendedId = firmware.Firmware.ManufacturerExtendedId,
            FileName = firmware.SourceFile,
            Link = new Uri($"{baseUri}/firmware/{manufacturerPath}/{firmware.SourceFile}", UriKind.RelativeOrAbsolute),
            Sha1 = sha1,
            LastUpdate = firmware.Version.LastUpdate,
            Created = firmware.Version.Created.ToString(ShortDateFormat, CultureInfo.CurrentCulture),
            Version = firmware.Firmware.Version,
            VersionExtension = firmware.Firmware.VersionExtension,
            FileSize = size,
            Decoder = [.. firmware.Firmware.Decoders.Select(d => new DecoderReference
            {
                Name = d.Name,
                //Type = d.Type,
                //TypeIds = d.TypeIds
            })]
        };
        return firmwareInfo;
    }

    private void WriteData(string outputPath, DecoderDbInfo response, DecoderDetails[] decoderDetails)
    {
        outputPath = new DirectoryInfo(string.IsNullOrEmpty(outputPath) ? "." : outputPath).FullName;


        SaveJsonData(outputPath, response);

        SaveDecoderDetails(outputPath, decoderDetails);

        SaveFirmwareDetails(outputPath, response);
    }

    private void SaveFirmwareDetails(string outputPath, DecoderDbInfo response)
    {
        var firmwareDirectoryPath = ioService.GetPath(outputPath, "firmware");
        ioService.CreateDirectory(firmwareDirectoryPath);
        var firmwareDetailsOutputFilePath = ioService.GetPath(firmwareDirectoryPath, "firmwareDetails.json");
        if (jsonService.SaveToFile(response.Firmware, firmwareDetailsOutputFilePath))
        {
            var jsonInfo = new FileInfo(firmwareDetailsOutputFilePath);
            logger.LogInformation("firmwareDetails.json info generated at {Path} ({Length})", firmwareDetailsOutputFilePath, jsonInfo.Length);
        }
        else
        {
            logger.LogWarning("firmwareDetails.json was not generated!");
        }
    }

    private void SaveDecoderDetails(string outputPath, DecoderDetails[] decoderDetails)
    {
        var decoderDirectoryPath = ioService.GetPath(outputPath, "decoder");
        ioService.CreateDirectory(decoderDirectoryPath);
        var detailsOutputFilePath = ioService.GetPath(decoderDirectoryPath, "decoderDetails.json");
        if (jsonService.SaveToFile(decoderDetails, detailsOutputFilePath))
        {
            var jsonInfo = new FileInfo(detailsOutputFilePath);
            logger.LogInformation("decoderDetails.json info generated at {Path} ({Length})", detailsOutputFilePath, jsonInfo.Length);
        }
        else
        {
            logger.LogWarning("decoderDetails.json was not generated!");
        }
    }

    private void SaveJsonData(string outputPath, DecoderDbInfo response)
    {
        var json2OutputFilePath = ioService.GetPath(outputPath, "repository.json");

        if (jsonService.SaveToFile(response, json2OutputFilePath))
        {
            var jsonInfo = new FileInfo(json2OutputFilePath);
            logger.LogInformation("Repository (json) info generated at {Path} ({Length})", json2OutputFilePath, jsonInfo.Length);
        }
        else
        {
            logger.LogWarning("repository.json was not generated!");
        }
    }
}