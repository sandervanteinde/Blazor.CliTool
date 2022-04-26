namespace Blazor.CliTool.Services;

internal interface ITemplateWriter
{
    Task WriteTemplateAsync(string templateName, string templateContent);

    void PrintGeneratedOutput();
}