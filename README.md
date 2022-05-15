# Blazor.CliTool

[![Build and Push to NuGet](https://github.com/sandervanteinde/Blazor.CliTool/actions/workflows/dotnet.yml/badge.svg)](https://github.com/sandervanteinde/Blazor.CliTool/actions/workflows/dotnet.yml)

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

## Configuration

You are able to add a `blazor-tool.json` file anywhere in your file system tree.
The first occurence in the folder where you execute this, or any of its parents, will be used as configuration for the tool.
This allows you to put this file in the root of projects, repositories or file systems to determine the scope of the configuration.

All configurations are optional, an example of the configuration looks as follows:

```JSON
{
  "$schema": "https://raw.githubusercontent.com/sandervanteinde/Blazor.CliTool/main/schema/v1/schema.json",
  "namespace": "file", // option between 'file' and 'block', being either file-scoped namespaces (.NET 6+) or block scoped. Defaults to whatever your projects supports, unless overwritten here.
  "generateInFolder": false, // option to generate files in an additional folder. E.g.: `blazor component Test` would create the component as follows: `/Test/Test.razor.cs`. Defaults to `false`.
  "styleSheetExtension": "css" // option to overwrite the stylesheet you want to generate. Defaults to `css`.
}

```
