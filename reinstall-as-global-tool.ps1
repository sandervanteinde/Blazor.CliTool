dotnet tool uninstall Blazor.CliTool --global
dotnet pack ./Blazor.CliTool --output C:/Nuget
dotnet tool install --global --add-source C:/Nuget Blazor.CliTool --ignore-failed-sources
