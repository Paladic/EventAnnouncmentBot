using System.Diagnostics;
using Discord.Interactions;

namespace Discordbot.Modules.TextCommands.General.User;

public class General : InteractionModuleBase
{
    [SlashCommand("пинг", "Проверка задержки бота.")]
    public async Task Ping()
    {
        var sw = new Stopwatch();
        sw.Start();
        await RespondAsync($"pong ");
        sw.Stop();
        await Context.Interaction.ModifyOriginalResponseAsync(properties => properties.Content = $"pong `{sw.ElapsedMilliseconds}` ms.");
    }
}