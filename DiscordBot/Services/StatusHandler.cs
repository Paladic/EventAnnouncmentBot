using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using IResult = Discord.Interactions.IResult;

namespace Discordbot.Services
{
    public class StatusHandler
    {
        private readonly DiscordSocketClient _client;

        public StatusHandler(DiscordSocketClient client)
        {
            _client = client;
        }

#pragma warning disable 1998
        public async Task InitializeAsync ( )
#pragma warning restore 1998
        {
            _client.Ready += OnReady;
        }
        private async Task OnReady()
        {
            await _client.SetGameAsync("тебя", null, ActivityType.Listening);
            await _client.SetStatusAsync(UserStatus.Idle);
            Console.WriteLine($"[{DateTime.Now.ToString("hh:mm:ss dd.MM.yyyy")}] {_client.CurrentUser.Username}: Статус установлен!");
        }
    }
}