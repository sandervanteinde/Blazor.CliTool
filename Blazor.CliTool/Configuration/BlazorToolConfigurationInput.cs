namespace Blazor.CliTool.Configuration;

public record BlazorToolConfigurationInput(NamespaceConfigurationOption? Namespace, bool? GenerateInFolder, string? StyleSheetExtension)
{
    public BlazorToolConfiguration ApplyDefaults()
    {
        var config = BlazorToolConfiguration.Default;
        return config with
        {
            Namespace = Namespace ?? config.Namespace,
            GenerateInFolder = GenerateInFolder ?? config.GenerateInFolder,
            StylesheetExtension = StyleSheetExtension ?? config.StylesheetExtension
        };
    }
}
