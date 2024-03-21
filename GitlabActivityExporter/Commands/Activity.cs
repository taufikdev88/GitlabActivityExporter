using Cocona;
using NGitLab;
using NGitLab.Models;

namespace GitlabActivityExporter.Commands;
public class Activity(
    GitLabClient client)
{
    [Command(Description = "Get Last Activities")]
    public void All(
        [Option(Description = "Default after 1 day ago")] DateTime? after,
        [Option('q', Description = "Print quietly")] bool? quiet)
    {
        Console.WriteLine($"User Events ({client.Users.Current.Name}):");

        var iuserEvents = client.GetUserEvents(client.Users.Current.Id);
        if (iuserEvents is null)
        {
            Console.WriteLine("Empty events");
            return;
        }

        after ??= DateTime.Now.AddDays(-1);
        var userEvents = iuserEvents.Get(new EventQuery
        {
            After = after.Value.ToUniversalTime(),
        });

        Dictionary<int, string> projectNames = [];
        Dictionary<int, IRepositoryClient> projectRepos = [];
        Dictionary<string, CommitSummary> commitSummaries = [];

        foreach (var userEvent in userEvents)
        {
            if (!projectNames.TryGetValue(userEvent.ProjectId, out var projectName))
            {
                var project = client.Projects.GetById(userEvent.ProjectId, new SingleProjectQuery
                {
                    Statistics = false
                });
                projectName = project.NameWithNamespace;
                projectNames.Add(userEvent.ProjectId, projectName);
            }
            if (!projectRepos.TryGetValue(userEvent.ProjectId, out var projectRepo))
            {
                projectRepo = client.GetRepository(userEvent.ProjectId);
                projectRepos.Add(userEvent.ProjectId, projectRepo);
            }

            if (projectRepo is null || userEvent.PushData is null)
                continue;

            var commits = projectRepo.GetCommits(userEvent.PushData.Ref, userEvent.PushData.CommitCount);
            foreach (var commit in commits)
            {
                if (commitSummaries.ContainsKey(commit.ShortId) || commit.CreatedAt < after)
                    continue;

                commitSummaries.Add(commit.ShortId, new CommitSummary
                {
                    ProjectName = projectName,
                    Ref = userEvent.PushData.Ref,
                    ShortId = commit.ShortId,
                    Title = commit.Title,
                    CreatedAt = commit.CreatedAt
                });
            }
        }

        foreach (var commitSummary in commitSummaries.Values
            .OrderBy(e => e.ProjectName)
            .ThenByDescending(e => e.CreatedAt))
        {
            if (quiet == true)
                Console.WriteLine($"{commitSummary.CreatedAt:yyyy/MM/dd} - {commitSummary.ProjectName} - ({commitSummary.ShortId}) {commitSummary.Title,-72}");
            else
                Console.WriteLine($"{commitSummary.CreatedAt:yyyy/MM/ddTHH:mm:sszzz}\t{commitSummary.ProjectName,-30} {commitSummary.Ref,-20} ({commitSummary.ShortId}) {commitSummary.Title,-72}");
        }
    }

    private class CommitSummary
    {
        public required string ProjectName { get; set; }
        public required string Ref { get; set; }
        public required string ShortId { get; set; }
        public required string Title { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
