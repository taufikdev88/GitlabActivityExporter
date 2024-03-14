using Cocona;
using Microsoft.Extensions.Options;
using NGitLab;
using NGitLab.Models;
using System.ComponentModel;
using System.Text.Json;

namespace GitlabActivityExporter.Commands;

[Description("Projects on gitlab server")]
public class Project(
    GitLabClient client,
    IOptions<JsonSerializerOptions> options)
{
    [Command(Description = "List All Projects")]
    public void List(
        [Option(Description = "Filter by project name or path")] string? filter,
        [Option(Description = "Default 1 day ago")] DateTimeOffset? after,
        [Option(Description = "Default last_activity_at")] string? orderBy = "last_activity_at")
    {
        var projects = client.Projects.GetAsync(new ProjectQuery
        {
            Simple = true,
            Statistics = false,
            OrderBy = orderBy,
            Search = filter,
            LastActivityAfter = after ?? DateTimeOffset.Now.AddDays(-1)
        });

        Console.WriteLine("Projects:");
        Console.WriteLine("Id\tPath");
        foreach (var project in projects)
        {
            Console.WriteLine($"{project.Id}\t{project.WebUrl}");
        }
    }

    [Command(Description = "Get Project Detail")]
    public void Detail(int id)
    {
        var project = client.Projects.GetById(id, new SingleProjectQuery
        {
            Statistics = false
        });

        if (project is null)
        {
            Console.WriteLine("Not Found");
            return;
        }

        Console.WriteLine(project.NameWithNamespace);
        Console.WriteLine(JsonSerializer.Serialize(project, options.Value));
    }
}
