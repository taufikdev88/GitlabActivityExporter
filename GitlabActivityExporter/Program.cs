using Cocona;
using GitlabActivityExporter.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NGitLab;

var builder = CoconaApp.CreateBuilder(args, options =>
{
    options.EnableConvertOptionNameToLowerCase = true;
});

// logging
builder.Logging.AddDebug();

// dependency injection
builder.Services.AddSingleton(new GitLabClient(builder.Configuration["Server:Host"], builder.Configuration["Server:PrivateToken"]));

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