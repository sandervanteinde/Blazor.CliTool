using Blazor.CliTool.Services;
using Cocona;

namespace Blazor.CliTool.Commands;

internal class ComponentCommand
{
    private readonly IBlazorContext _blazorContext;
    private readonly ITemplateWriter _templateWriter;

    public ComponentCommand(IBlazorContext blazorContext, ITemplateWriter templateWriter)
    {
        _blazorContext = blazorContext;
        _templateWriter = templateWriter;
    }

    [Command("component", Aliases = new[] { "c" }, Description = "Generates a component")]
    public async Task RunCommand([Argument(Order = 1)] string name)
    {
        await _templateWriter.WriteTemplateAsync($"{name}.razor", @$"<h3>{name}</h3>");
        await _templateWriter.WriteTemplateAsync($"{name}.razor.cs", GetTemplateForCsFile(name));
        await _templateWriter.WriteTemplateAsync($"{name}.razor.css", string.Empty);

        _templateWriter.PrintGeneratedOutput();
    }

    private string GetTemplateForCsFile(string name)
    {
        var @namespace = _blazorContext.GetNamespace();
        if (_blazorContext.IsFileScopedNamespacesRequested())
        {
            return $@"namespace {@namespace};

public partial class {name}
{{

}}";
        }

        return $@"namespace {@namespace}
{{
    public partial class {name}
    {{
    
    }}
}}";
    }
}
