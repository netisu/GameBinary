using DiscordRPC;

namespace Netisu.Workshop
{
    public sealed class ActivityManager
    {
        public DiscordRpcClient discordRpcClient = null!;
        public static ActivityManager Instance { get; private set; } = null!;

        public ActivityManager(string Details = "My Game", string State = "Editing workspace")
        {
            discordRpcClient = new("1384763000753229844");
            discordRpcClient.Initialize();

            discordRpcClient.SetPresence(new RichPresence()
            {
                Details = Details,
                State = State,
                Assets = new Assets(),
            });

            Instance = this;
        }
    }
}