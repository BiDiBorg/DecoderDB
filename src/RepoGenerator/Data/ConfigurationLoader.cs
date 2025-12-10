using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using org.bidib.DecocderDB.RepoGenerator.Models;
using org.bidib.Net.Core.Services.Interfaces;
using Serilog;
using System.IO;

namespace org.bidib.DecocderDB.RepoGenerator.Data;

public class ConfigurationLoader(IIoService ioService) : IConfigurationLoader
{
    public Config Load(string[] args)
    {
        var builder = new ConfigurationBuilder();
        builder.AddCommandLine(args);
        var commandConfig = builder.Build();

        Config config;

        if (ioService.FileExists("config.json"))
        {
            var configString = File.ReadAllText("config.json");
            config = JsonConvert.DeserializeObject<Config>(configString);
        }
        else
        {
            Log.Information("Local config.json not found. Using new instance.");
            config = new Config();
        }

        config.RepoPath = !string.IsNullOrEmpty(commandConfig["repoPath"])
            ? commandConfig["repoPath"]
            : config.RepoPath;

        if (string.IsNullOrEmpty(config.RepoPath))
        {
            Log.Error("Path to repository base directory is missing! Provide a path with '--repoPath [PathToBaseDirectory]'");
            return null;
        }

        config.OutputPath = !string.IsNullOrEmpty(commandConfig["outputPath"])
            ? commandConfig["outputPath"]
            : config.OutputPath;

        if (string.IsNullOrEmpty(config.OutputPath))
        {
            config.OutputPath = string.Empty;
            Log.Information("No output path defined! Using execution path");
        }

        config.BaseUri = !string.IsNullOrEmpty(commandConfig["baseUri"])
            ? commandConfig["baseUri"]
            : config.BaseUri;

        if (string.IsNullOrEmpty(config.BaseUri))
        {
            config.BaseUri = "http://localhost";
            Log.Information($"No base uri defined! Using '{config.BaseUri}' as default.");

        }
        
        config.FileName = !string.IsNullOrEmpty(commandConfig["file"])
            ? commandConfig["file"]
            : config.FileName;

        return config;
    }
}