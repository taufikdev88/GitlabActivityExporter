using Cocona;
using GitlabActivityExporter.Commands;
using GitlabActivityExporter.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using NGitLab;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = CoconaApp.CreateBuilder(args, options =>
{
    options.EnableConvertOptionNameToLowerCase = true;
});

// dependency injection
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.WriteIndented = true;
    options.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddTransient<IDirectoryService, DirectoryService>();
builder.Services.AddTransient<IConfigurationService, ConfigurationService>();
builder.Services.AddTransient(sp =>
{
    var directoryService = sp.GetRequiredService<IDirectoryService>();
    return DataProtectionProvider.Create(new DirectoryInfo(directoryService.GetUserDirectoryPath()));
});
builder.Services.AddSingleton(sp =>
{
    var configProvider = sp.GetRequiredService<IConfigurationService>();
    return new GitLabClient(configProvider.Get("Host"), configProvider.Get("Token"));
});

var app = builder.Build();

app.AddSubCommand("project", x =>
{
    x.AddCommands<Project>();
}).WithDescription("Projects in gitlab");

app.AddSubCommand("activity", x =>
{
    x.AddCommands<Activity>();
}).WithDescription("Activities in gitlab");

app.AddSubCommand("config", x =>
{
    x.AddCommands<Config>();
}).WithDescription("App's configuration");

app.Run();