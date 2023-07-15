using Discord;
using MelonLoader;
using System;
using System.Threading;

[assembly: MelonInfo(typeof(DiscordStatus.DiscordStatusPlugin), "Discord Status", "2.0.0", "slxdy")]
[assembly: MelonColor(255, 100, 0, 255)]

namespace DiscordStatus;

public class DiscordStatusPlugin : MelonPlugin
{
    public const long AppId = 977473789854089226;
    private Discord.Discord discordClient;
    private ActivityManager activityManager;

    private bool gameClosing;
    private bool gameStarted;
    public long gameStartedTime;

    public override void OnPreInitialization()
    {
        DiscordLibraryLoader.LoadLibrary();
        InitializeDiscord();
        UpdateActivity();
        new Thread(DiscordLoopThread).Start();
    }

    public override void OnLateInitializeMelon()
    {
        gameStarted = true;
        gameStartedTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

        UpdateActivity();
    }

    public override void OnDeinitializeMelon()
    {
        gameClosing = true;
    }

    public void DiscordLoopThread()
    {
        for (; ; )
        {
            if (gameClosing)
                break;

            discordClient?.RunCallbacks();
            Thread.Sleep(200);
        }
    }

    public void InitializeDiscord()
    {
        discordClient = new Discord.Discord(AppId, (ulong)CreateFlags.NoRequireDiscord);
        discordClient.SetLogHook(LogLevel.Debug, DiscordLogHandler);

        activityManager = discordClient.GetActivityManager();
    }

    private void DiscordLogHandler(LogLevel level, string message)
    {
        switch (level)
        {
            case LogLevel.Info:
            case LogLevel.Debug:
                LoggerInstance.Msg(message);
                break;

            case LogLevel.Warn:
                LoggerInstance.Warning(message);
                break;

            case LogLevel.Error:
                LoggerInstance.Error(message);
                break;
        }
    }

    public void UpdateActivity()
    {
        var activity = new Activity
        {
            Details = $"Playing {MelonUtils.CurrentGameAttribute.Name}"
        };

        activity.Assets.LargeImage = "ml_icon";
        activity.Name = $"MelonLoader {BuildInfo.Version}";
        activity.Instance = true;
        activity.Assets.LargeText = activity.Name;

        var modsCount = MelonMod.RegisteredMelons.Count;
        activity.State = gameStarted ? $"{modsCount} {(modsCount == 1 ? "Mod" : "Mods")} Loaded" : "Loading MelonLoader";

        if (gameStarted)
            activity.Timestamps.Start = gameStartedTime;

        activityManager?.UpdateActivity(activity, ResultHandler);
    }

    public void ResultHandler(Result result)
    {

    }
}
