namespace Blazor.CliTool.Services;

public interface IBlazorContext
{
    string GetNamespace();
    string GetOutputDirectory();
    bool IsFileScopedNamespacesRequested();
}
