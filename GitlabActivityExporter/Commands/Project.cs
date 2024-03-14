using Cocona;
using NGitLab;
using NGitLab.Models;
using System.ComponentModel;

namespace GitlabActivityExporter.Commands;

[Description("Projects on gitlab server")]
public class Project(GitLabClient client)
{
    [Command(Description = "List All Projects")]
    public void List(string? filter, string? orderBy = "last_activity_at")
    {
        var projects = client.Projects.GetAsync(new ProjectQuery
        {
            Simple = true,
            Statistics = false,
            OrderBy = orderBy,
            Search = filter
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
            Statistics = true
        });

        if (project is null)
        {
            Console.WriteLine("Not Found");
            return;
        }

        Console.WriteLine(project);
    }
}
