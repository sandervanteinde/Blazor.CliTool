using Blazor.CliTool.Commands;
using Blazor.CliTool.Services;
using Cocona;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = CoconaApp.CreateBuilder();
builder.Services
    .AddLogging(opts =>
    {


        if (builder.Environment.IsDevelopment())
        {
            opts.SetMinimumLevel(LogLevel.Trace);
        }
    })
    .AddSingleton<IBlazorContext, BlazorContext>()
    .AddSingleton<ITemplateWriter, FileTemplateWriter>()
    .AddTransient<INamespaceFixer, NamespaceFixer>();

var app = builder.Build();

app.AddCommand("component", ComponentCommand.RunCommand)
    .WithAliases("c");

await app.RunAsync();