using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Blazor.CliTool.Services;

public class NamespaceFixer : INamespaceFixer
{
    private static readonly Regex dotNumberRegex = new Regex(@"\.(\d)", RegexOptions.Compiled);
    private readonly ILogger _logger;

    public NamespaceFixer(ILogger<NamespaceFixer> logger)
    {
        _logger = logger;
    }

    public string FixNamespace(string original)
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