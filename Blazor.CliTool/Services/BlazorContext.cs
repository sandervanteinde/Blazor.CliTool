using Microsoft.Extensions.Logging;

namespace Blazor.CliTool.Services;

public class BlazorContext : IBlazorContext
{
    private readonly string _runningDirectory;
    private readonly Lazy<string> _namespace;
    private readonly INamespaceFixer _namespaceFixer;
    private readonly ILogger<BlazorContext> _logger;

    public BlazorContext(INamespaceFixer namespaceFixer, ILogger<BlazorContext> logger)
    {
        _runningDirectory = Environment.CurrentDirectory;
        _namespace = new(DetermineNamespaceByCurrentDirectory);
        _namespaceFixer = namespaceFixer;
        _logger = logger;
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
        var foldersProcessed = new LinkedList<string>();

        var directory = new DirectoryInfo(_runningDirectory);

        while (directory is not null)
        {
            _logger.LogDebug("Checking if directory '{Directory}' contains csproj file", directory.FullName);
            var projectFile = directory.EnumerateFiles("*.csproj")
                .FirstOrDefault();
            if (projectFile is not null)
            {
                _logger.LogDebug("Project file found at {ProjectFileLocation}", projectFile.FullName);

                var unprocessedNamespace = foldersProcessed.Count == 0
                    ? Path.GetFileNameWithoutExtension(projectFile.Name)
                    : $"{Path.GetFileNameWithoutExtension(projectFile.Name)}.{string.Join('.', foldersProcessed)}";

                return _namespaceFixer.FixNamespace(unprocessedNamespace);
            }

            foldersProcessed.AddFirst(directory.Name);
            directory = directory.Parent;
        }

        _logger.LogDebug("No csproj found in parent directory tree, return default namespace 'App'");
        return "App";
    }
}
