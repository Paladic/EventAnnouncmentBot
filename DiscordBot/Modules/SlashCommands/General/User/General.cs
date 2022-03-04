using System.Diagnostics;
using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace DiscordBot.Modules.SlashCommands.General.User
{
    public class General : InteractionModuleBase 
    { 
        
        [SlashCommand("ивент", "создать оповещение об ивенте.")]
        public async Task EventAnnounce(
            //[Summary("пользователь","выбери того, кто будет проводить ивент")]SocketGuildUser user,
            
            [Summary("ивент", "название ивента"),
            Choice("бункер","бункер"),
            Choice("among us", "among-us"),
            Choice("своя игра", "своя-игра"),
            Choice("gartic phone", "gartic-phone"),
            Choice("jackbox", "jackbox"),
            //Choice("unfortunate spacemen", "unfortunate-spacemen"),
            //Choice("project winter", "project-winter"),
            Choice("codenames", "codenames"),
            Choice("бункер (сайт)", "бункер-сайт"),
            Choice("шляпа alias", "шляпа-alias"),
            Choice("secret hitler", "secret-hitler"),
            //Choice("карты против всех", "карты-против-всех"),
            Choice("crab game", "crabe-game"),
            //Choice("мафия", "мафия")
            ]string eventName,
            
            [Summary("когда","выбери, когда ивент. Если выбрать сегодня / завтра - укажи после еще и время"),
            Choice("сейчас", "сейчас"),
            Choice("сегодня", "сегодня"),
            Choice("завтра", "завтра")]string eventDay, 
            [Summary("время","напиши время, в которое будет ивентдопустим: 19:00")]string? eventTime = null)
        {
            await DeferAsync();
            
            var authorName = $"{Context.User.Username}#{Context.User.Discriminator}";
            var authorUrl = Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl();
            
            IConfiguration curEvent = new ConfigurationBuilder()
                .AddJsonFile($"events/{eventName}.json", optional: true)
                .Build();

            var mentionRoles = "<@&846120721570070528> " + curEvent["mentionRole"];

            var title = curEvent["title"];
            
            var gameDesc = curEvent["gameDesc"] + "\n\n";
            var playerDesc = curEvent["playerDesc"] + "\n\n";
            
            var ruleDesc = "__Правила:__\n" +
                $"[Нажми на меня]({curEvent["ruleDesc"]})\n\n";
            
            var enjoyToGame = "";
            
            var playerCount = $"▻ **Игроков:** {curEvent["playerCount"]} человек\n";
            var eventMaster = $"▻ **Ведущий:** {Context.User.Mention}";
           
            var imageUrl = curEvent["imageUrl"];
            
            var r = int.Parse(curEvent["colorR"]);
            var g = int.Parse(curEvent["colorG"]);
            var b = int.Parse(curEvent["colorB"]);
            var color = new Color(r,g,b);
            
            switch (eventDay)
            {
                case "сейчас":
                    title += " | ПРЯМО СЕЙЧАС";
                    gameDesc = $"Всем привет! Прямо сейчас мы начинаем **{curEvent["title"]}**!\n\n";
                    playerDesc = "";
                    enjoyToGame = $"[**Присоединиться к игре!**]({curEvent["enjoyToGame"]})\n\n";
                    break;
                case "сегодня" or "завтра":
                    if (eventTime == null)
                    {
                        await FollowupAsync("Вы не ввели время ивента");
                        return;
                    }
                    title += $" | {eventDay.ToUpper()} | {eventTime}";
                    if (playerDesc == "\n\n") playerDesc = null;
                    break;
            }
            
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x.WithName(authorName);
                    x.WithIconUrl(authorUrl);
                    x.Build();
                })
                .WithTitle(title)
                .WithDescription(gameDesc + playerDesc + ruleDesc + enjoyToGame + playerCount + eventMaster)
                .WithImageUrl(imageUrl)
                .WithColor(color)
                .Build();

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
            
            if (!ulong.TryParse(config["eventChannelId"], out var channelId))
            {
                await FollowupAsync("appsettings.json => eventChannelId - отсутствует Id");
                return;
            }
            
            var channel = await Context.Guild.GetChannelAsync(channelId);

            await ((SocketTextChannel) channel).SendMessageAsync(mentionRoles, embed: embed);
            await Context.Interaction.ModifyOriginalResponseAsync(x => x.Content = "сообщение отправлено");
        }
}
}