using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Blazor.CliTool.Services;

public class NamespaceSanitizer : INamespaceSanitizer
{
    private static readonly Regex dotNumberRegex = new(@"\.(\d)", RegexOptions.Compiled);
    private readonly ILogger _logger;

    public NamespaceSanitizer(ILogger<NamespaceSanitizer> logger)
    {
        _logger = logger;
    }

    public string SanitizeNamespace(string original)
    {
        var newNamespace = dotNumberRegex.Replace(original, "._$1")
            .Replace(' ', '_');

        if (original == newNamespace)
        {
            _logger.LogDebug("No invalid characters found in namespace");
        }
        else
        {
            _logger.LogDebug("Changed namespace from '{OriginalNamespace}' to {NewNamespace}.", original, newNamespace);
        }

        return newNamespace;
    }
}