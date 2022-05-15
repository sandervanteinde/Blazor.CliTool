namespace Blazor.CliTool.Services;

public interface IBlazorContext
{
    void Initialize(string runningDirectory, string componentName);
    string GetNamespace();
    string GetOutputDirectory();
    bool IsFileScopedNamespacesRequested();
    string GetRequestedStylesheetExtension();
}
