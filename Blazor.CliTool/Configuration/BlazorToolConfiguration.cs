namespace Blazor.CliTool.Configuration;

public record BlazorToolConfiguration(NamespaceConfigurationOption? Namespace, bool GenerateInFolder, string StylesheetExtension)
{
    public static BlazorToolConfiguration Default => new(null, false, "css");
}