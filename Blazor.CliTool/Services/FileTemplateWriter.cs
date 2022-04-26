namespace Blazor.CliTool.Services;

internal class FileTemplateWriter : ITemplateWriter
{
    private readonly IBlazorContext _context;
    private readonly List<string> _generatedFiles = new();

    public FileTemplateWriter(IBlazorContext context)
    {
        _context = context;
    }

    public async Task WriteTemplateAsync(string templateName, string templateContent)
    {
        var filePath = Path.Combine(_context.GetOutputDirectory(), templateName);
        using var fileStream = File.Create(filePath);
        using var textWriter = new StreamWriter(fileStream);

        await textWriter.WriteLineAsync(templateContent);
        _generatedFiles.Add(filePath);
    }

    public void PrintGeneratedOutput()
    {
        foreach (var file in _generatedFiles)
        {
            Console.WriteLine($"Created [{file}]");
        }
    }
}
