using SlackAPI;

namespace DependencyInjectionWorkshop.Adapter
{
    public class SlackAdapter : INotification
    {
        public void PushMessage(string message)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(repo =>
            {
            }, "my channel", message, "my bot name");
        }
    }
}