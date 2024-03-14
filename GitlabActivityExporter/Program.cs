using Cocona;
using GitlabActivityExporter.Commands;
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

builder.Services.AddSingleton(new GitLabClient("https://lab.tog.co.id", "glpat-cyKHXyN2n2G4Etz2zXqz"));
//builder.Services.AddSingleton(new GitLabClient(builder.Configuration["Server:Host"], builder.Configuration["Server:PrivateToken"]));

var app = builder.Build();

app.AddSubCommand("project", x =>
{
    x.AddCommands<Project>();
}).WithDescription("Projects in gitlab");

app.AddSubCommand("activity", x =>
{
    x.AddCommands<Activity>();
}).WithDescription("Activities in gitlab");

app.Run();