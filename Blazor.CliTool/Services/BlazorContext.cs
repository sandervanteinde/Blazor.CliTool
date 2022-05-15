using Blazor.CliTool.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Blazor.CliTool.Services;

public class BlazorContext : IBlazorContext
{
    private string? _runningDirectory;
    private BlazorToolConfiguration? _configuration;
    private XElement? _projectFile;
    private string? _projectFileLocation;
    private string? _componentName;

    private readonly INamespaceSanitizer _namespaceSanitizer;
    private readonly ILogger<BlazorContext> _logger;


    public BlazorContext(INamespaceSanitizer namespaceFixer, ILogger<BlazorContext> logger)
    {
        _namespaceSanitizer = namespaceFixer;
        _logger = logger;
    }

    public void Initialize(string runningDirectory, string componentName)
    {
        _runningDirectory = runningDirectory;
        _componentName = componentName;
        _logger.LogDebug("Finding project info and configuration json.");
        var directory = new DirectoryInfo(runningDirectory);
        while (directory is not null)
        {
            var projectFileLocation = directory.EnumerateFiles("*.csproj")
                .FirstOrDefault();

            if (projectFileLocation is not null)
            {
                _projectFileLocation = projectFileLocation.FullName;
                _projectFile = XElement.Load(projectFileLocation.FullName);
            }

            var configFile = directory.EnumerateFiles("blazor-tool.json")
                .FirstOrDefault();

            if (configFile is not null)
            {
                _configuration = ReadConfigFile(configFile);
            }

            directory = directory.Parent;
        }

        if (_configuration is null)
        {
            _configuration = BlazorToolConfiguration.Default;
        }
    }


    public bool IsFileScopedNamespacesRequested()
    {
        _logger.LogDebug("Checking if file scoped namespaces were requested");
        var settingFromConfiguration = _configuration?.Namespace;

        if (settingFromConfiguration is not null)
        {
            return settingFromConfiguration == NamespaceConfigurationOption.File;
        }

        if (_projectFile is null)
        {
            _logger.LogDebug("No csproj found, for safety. Not using file-scoped namespaces.");
            return false;
        }

        var csproj = _projectFile;
        var targetFrameworkNodes = csproj.Descendants("TargetFramework")
            .Select(node => node.Value)
            .ToList();

        _logger.LogDebug("Found {FrameworkNodes} framework nodes in csproj file.", targetFrameworkNodes.Count);

        var targetFramework = targetFrameworkNodes.FirstOrDefault();
        if (targetFramework is null)
        {
            _logger.LogDebug("csproj did not contain any <TargetFramework> xml nodes. Not using file-scoped namespaces.");
            return false;
        }

        var targetFrameworkSupportsFileScoped = IsSupportedFrameworkForFileScopedNamespace(targetFramework);
        _logger.LogDebug($@"Framework {{TargetFramework}} {(targetFrameworkSupportsFileScoped ? "does" : "does not")} support file scoped namespaces.", targetFramework);
        return targetFrameworkSupportsFileScoped;
    }

    public string GetNamespace()
    {
        return DetermineNamespaceByCurrentDirectory();
    }

    public string GetOutputDirectory()
    {
        ValidateInitialized();
        if (_configuration.GenerateInFolder)
        {
            return Path.Combine(_runningDirectory, _componentName);
        }
        return _runningDirectory;
    }

    public string GetRequestedStylesheetExtension()
    {
        ValidateInitialized();
        return _configuration.StylesheetExtension;
    }

    [MemberNotNull(nameof(_runningDirectory), nameof(_componentName), nameof(_configuration))]
    private void ValidateInitialized()
    {
        ValidateValueNotNull(_runningDirectory);
        ValidateValueNotNull(_componentName);
        ValidateValueNotNull(_configuration);

        static void ValidateValueNotNull<T>([NotNull] T value)
        {
            if (value is null)
            {
                throw new InvalidOperationException("The BlazorContext was not initialized prior to invoking this method.");
            }
        }
    }

    private string DetermineNamespaceByCurrentDirectory()
    {
        ValidateInitialized();
        _logger.LogDebug("Determining namespace");
        if (_projectFile is null)
        {
            _logger.LogDebug("No project file found, returning default namespace 'App'.");
            return "App";
        }

        var directory = Path.GetDirectoryName(_projectFileLocation)!;
        var rootNamespace = _projectFile
            .Descendants("RootNamespace")
            .Select(e => e.Value)
            .FirstOrDefault()
            ?? Path.GetFileNameWithoutExtension(_projectFileLocation)
            ?? throw new InvalidOperationException("Unable to get a file name from the cs project file.");

        var nestedDirectoryPath = _runningDirectory.Replace(directory, string.Empty)
            .TrimStart(Path.DirectorySeparatorChar)
            .Split(Path.DirectorySeparatorChar);

        var fullNamespace = nestedDirectoryPath.Length == 0
            ? rootNamespace
            : $"{rootNamespace}.{string.Join('.', nestedDirectoryPath)}";

        return _namespaceSanitizer.SanitizeNamespace(fullNamespace);
    }

    private static bool IsSupportedFrameworkForFileScopedNamespace(string framework)
    {
        return Regex.IsMatch(framework, "net[6789]\\.0.*");
    }

    private BlazorToolConfiguration ReadConfigFile(FileInfo configFile)
    {
        using var fs = configFile.OpenRead();
        BlazorToolConfigurationInput? input = null;
        try
        {
            input = JsonSerializer.Deserialize<BlazorToolConfigurationInput>(fs, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) } });
        }
        catch (JsonException ex)
        {
            _logger.LogError("Failed to parse file {FileName}:{NewLine}\t{Reason}", configFile.FullName, Environment.NewLine, ex.Message);
            throw;
        }

        if (input is null)
        {
            _logger.LogError("Failed to parse file {FileName}: Parsing resulted in an empty object.", configFile.FullName);
            throw new InvalidOperationException("The filename was null.");
        }

        return input.ApplyDefaults();
    }
}
