using SlackAPI;

internal class SlackAdapter
{
    public void Notify(string message)
    {
        var slackClient = new SlackClient("my api token");
        slackClient.PostMessage(repo =>
        {
        }, "my channel", message, "my bot name");
    }
}