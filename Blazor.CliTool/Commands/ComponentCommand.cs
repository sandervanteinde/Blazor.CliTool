using Blazor.CliTool.Services;
using Cocona;

namespace Blazor.CliTool.Commands;

internal static class ComponentCommand
{
    public static async Task RunCommand([Argument(Order = 1)] string name, IBlazorContext blazorContext, ITemplateWriter templateWriter)
    {
        var @namespace = blazorContext.GetNamespace();

        await templateWriter.WriteTemplateAsync($"{name}.razor", @$"<h3>{name}</h3>");

        await templateWriter.WriteTemplateAsync($"{name}.razor.cs", $@"namespace {@namespace};

public partial class {name}
{{

}}");

        await templateWriter.WriteTemplateAsync($"{name}.razor.css", string.Empty);

        templateWriter.PrintGeneratedOutput();
    }
}
