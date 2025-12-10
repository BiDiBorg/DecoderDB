using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using org.bidib.DecocderDB.RepoGenerator.Data;
using org.bidib.Net.Core.Services;
using org.bidib.Net.Core.Services.Interfaces;
using Serilog;
using System;
using System.IO;

namespace org.bidib.DecocderDB.RepoGenerator;

internal static class Program
{
    private static readonly IIoService IoService = new IoService(NullLogger<IoService>.Instance);       

    private static void Main(string[] args)
    {
        var logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.File("logs/generator.txt", rollingInterval: RollingInterval.Day).CreateLogger();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog(logger);
        });

        logger.Information("Bidib Repo Generator started.");

        var jsonService = new JsonService(loggerFactory);

        var config = new ConfigurationLoader(IoService).Load(args);
        if (config == null)
        {
            Console.ReadLine();
            return;
        }

        try
        {
            var decoderRepository = new DecoderRepository(IoService, jsonService, loggerFactory.CreateLogger<DecoderRepository>());
            decoderRepository.Reload(config.RepoPath);

            var firmwareRepository = new FirmwareRepository(IoService, jsonService, loggerFactory.CreateLogger<FirmwareRepository>());
            firmwareRepository.Reload(config.RepoPath);

            var generator = new RepositoryGenerator(firmwareRepository, decoderRepository, IoService, jsonService, loggerFactory.CreateLogger<RepositoryGenerator>());
            generator.Generate(config.OutputPath, config.BaseUri, config.RepoPath);
        }
        catch (InvalidDataException e)
        {
            logger.Error("Repo generation failed!");
            logger.Error(e.Message);

            Environment.Exit(-1);
        }
    }
}
