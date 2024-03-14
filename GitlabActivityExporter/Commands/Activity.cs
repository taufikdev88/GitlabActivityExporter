using Cocona;
using NGitLab;
using NGitLab.Models;

namespace GitlabActivityExporter.Commands;
public class Activity(GitLabClient client)
{
    [Command(Description = "Get Last Activities")]
    public void All()
    {
        var events = client.GetUserEvents(client.Users.Current.Id);

        if (events is null)
        {
            Console.WriteLine("Empty events");
            return;
        }

        var activities = events.Get(new EventQuery
        {
        });

        foreach (var activity in activities)
        {
            Console.WriteLine($"{activity.Action}\t{activity.Title}\t{activity.CreatedAt}");
        }
    }
}
