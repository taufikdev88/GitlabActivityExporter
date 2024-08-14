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

        // Get user events
        var iuserEvents = client.GetUserEvents(client.Users.Current.Id);
        if (iuserEvents is null)
        {
            Console.WriteLine("Empty events");
            return;
        }

        after ??= DateTime.Now.AddDays(-1);
        var eventQuery = new EventQuery
        {
            After = after.Value.ToUniversalTime(),
        };

        var userEvents = iuserEvents.Get(eventQuery);

        // grouping by project id and ref, get the minimum creation date
        var userEventSummaries = new List<EventSummary>();
        foreach (var userEvent in userEvents)
        {
            if (userEvent.PushData == null)
                continue;

            var existingEventSummary = userEventSummaries.FirstOrDefault(e => e.ProjectId.Equals(userEvent.ProjectId) && e.Ref.Equals(userEvent.PushData.Ref));
            if (existingEventSummary != null && existingEventSummary.CreatedAt < userEvent.CreatedAt)
                continue;

            if (existingEventSummary != null)
                userEventSummaries.Remove(existingEventSummary);

            userEventSummaries.Add(new EventSummary
            {
                ProjectId = userEvent.ProjectId,
                Ref = userEvent.PushData.Ref,
                CreatedAt = userEvent.CreatedAt
            });
        }

        // get all commits by project id, reff, since creation date

        Dictionary<int, string> projectNames = [];
        Dictionary<int, IRepositoryClient> projectRepos = [];
        Dictionary<string, CommitSummary> commitSummaries = [];

        foreach (var eventSummary in userEventSummaries)
        {
            if (!projectNames.TryGetValue(eventSummary.ProjectId, out var projectName))
            {
                var project = client.Projects.GetById(eventSummary.ProjectId, new SingleProjectQuery
                {
                    Statistics = false
                });
                projectName = project.NameWithNamespace;
                projectNames.Add(eventSummary.ProjectId, projectName);
            }
            if (!projectRepos.TryGetValue(eventSummary.ProjectId, out var projectRepo))
            {
                projectRepo = client.GetRepository(eventSummary.ProjectId);
                projectRepos.Add(eventSummary.ProjectId, projectRepo);
            }

            if (projectRepo is null)
                continue;

            var commitRequest = new GetCommitsRequest
            {
                RefName = eventSummary.Ref,
                Since = eventSummary.CreatedAt.AddMinutes(-1)
            };
            var commits = projectRepo.GetCommits(commitRequest);

            foreach (var commit in commits)
            {
                if (commit.AuthorEmail != client.Users.Current.CommitEmail ||
                    commitSummaries.ContainsKey(commit.ShortId) ||
                    commit.CreatedAt < after)
                {
                    continue;
                }

                commitSummaries.Add(commit.ShortId, new CommitSummary
                {
                    ProjectName = projectName,
                    Ref = eventSummary.Ref,
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
                Console.WriteLine($"{commitSummary.CreatedAt:yyyy/MM/ddTHH:mm:sszzz}\t{commitSummary.ProjectName,-64} {commitSummary.Ref,-20} ({commitSummary.ShortId}) {commitSummary.Title,-72}");
        }
    }

    private class EventSummary
    {
        public required int ProjectId { get; set; }
        public required string Ref { get; set; }
        public required DateTime CreatedAt { get; set; }
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
