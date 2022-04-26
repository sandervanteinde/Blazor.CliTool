# Blazor.CliTool

A simple Cli tool that can be installed to enable easily generating Blazor components in an Angular CLI fashion:

## Installation
Run the following command to install globally on your machine:
```
dotnet tool install Blazor.CliTool --global
```

## Usage

1. Open the directory in which you want to generate the component
   - For Visual Studio 2022 you can right-click the directory and click 'Open in Terminal'.
   - For Visual Studio Code you can right-click the directory and click 'Open in integrated terminal'
2. Run one of the commands:
   - `blazor component ComponentName` - Generates a component with the name `ComponentName` in the current directory.
