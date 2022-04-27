using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Blazor.CliTool.Services;

public class BlazorContext : IBlazorContext
{
    private readonly string _runningDirectory;
    private readonly Lazy<string> _namespace;
    private readonly Lazy<(XElement projectFile, string location)?> _projectFile;
    private readonly INamespaceSanitizer _namespaceSanitizer;
    private readonly ILogger<BlazorContext> _logger;


    public BlazorContext(INamespaceSanitizer namespaceFixer, ILogger<BlazorContext> logger)
    {
        _runningDirectory = Environment.CurrentDirectory;
        _namespace = new(DetermineNamespaceByCurrentDirectory);
        _namespaceSanitizer = namespaceFixer;
        _logger = logger;
        _projectFile = new Lazy<(XElement projectFile, string location)?>(GetProjectFile);
    }

    public bool IsFileScopedNamespacesRequested()
    {
        _logger.LogDebug("Checking if file scoped namespaces were requested");

        var projectFile = _projectFile.Value;
        if (!projectFile.HasValue)
        {
            _logger.LogDebug("No csproj found, for safety. Not using file-scoped namespaces.");
            return false;
        }

        var csproj = projectFile.Value.projectFile;
        var targetFrameworkNodes = csproj.Descendants("TargetFramework")
            .Select(node => node.Value)
            .ToList();

        _logger.LogDebug("Found {FrameworkNodes} framework nodes in csproj file.", targetFrameworkNodes.Count);

        if (!targetFrameworkNodes.Any())
        {
            _logger.LogDebug("csproj did not contain any <TargetFramework> xml nodes. Not using file-scoped namespaces.");
            return false;
        }

        var targetFramework = targetFrameworkNodes.FirstOrDefault();
        var targetFrameworkSupportsFileScoped = IsSupportedFrameworkForFileScopedNamespace(targetFramework);
        _logger.LogDebug($@"Framework {{TargetFramework}} {(targetFrameworkSupportsFileScoped ? "does" : "does not")} support file scoped namespaces.", targetFramework);
        return targetFrameworkSupportsFileScoped;
    }

    public string GetNamespace()
    {
        return _namespace.Value;
    }

    public string GetOutputDirectory()
    {
        return _runningDirectory;
    }

    private string DetermineNamespaceByCurrentDirectory()
    {
        _logger.LogDebug("Determining namespace");
        var projectFileResult = _projectFile.Value;
        if (projectFileResult is null)
        {
            _logger.LogDebug("No project file found, returning default namespace 'App'.");
            return "App";
        }

        var (csproj, location) = projectFileResult.Value;

        var directory = Path.GetDirectoryName(location)!;
        var rootNamespace = csproj
            .Descendants("RootNamespace")
            .Select(e => e.Value)
            .FirstOrDefault()
            ?? Path.GetFileNameWithoutExtension(location);

        var nestedDirectoryPath = _runningDirectory.Replace(directory, string.Empty)
            .TrimStart(Path.DirectorySeparatorChar)
            .Split(Path.DirectorySeparatorChar);

        var fullNamespace = nestedDirectoryPath.Length == 0
            ? rootNamespace
            : $"{rootNamespace}.{string.Join('.', nestedDirectoryPath)}";

        return _namespaceSanitizer.SanitizeNamespace(fullNamespace);
    }

    private (XElement projectFile, string location)? GetProjectFile()
    {
        _logger.LogDebug("Determining project file location");
        var foldersProcessed = new LinkedList<string>();

        var directory = new DirectoryInfo(_runningDirectory);

        while (directory is not null)
        {
            _logger.LogDebug("Checking if directory '{Directory}' contains csproj file", directory.FullName);
            var projectFileLocation = directory.EnumerateFiles("*.csproj")
                .FirstOrDefault();
            if (projectFileLocation is not null)
            {
                var xmlReader = XElement.Load(projectFileLocation.FullName);
                return (xmlReader, projectFileLocation.FullName);
            }

            foldersProcessed.AddFirst(directory.Name);
            directory = directory.Parent;
        }

        _logger.LogDebug("No csproj found in parent directory tree.");
        return null;
    }

    private bool IsSupportedFrameworkForFileScopedNamespace(string framework)
    {
        return Regex.IsMatch(framework, "net[67]\\.0.*");
    }
}
