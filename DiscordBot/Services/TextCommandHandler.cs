using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Newtonsoft.Json.Linq;
using IResult = Discord.Commands.IResult;

#pragma warning disable 1998

namespace Discordbot.Services
{
    public class TextCommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _provider;
        private readonly CommandService _service;

        public TextCommandHandler(IServiceProvider provider, DiscordSocketClient client, 
            CommandService service)
        {
            _client = client;
            _provider = provider;
            _service = service;
        }

        public async Task InitializeAsync ( )
        {
            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);

            // Process the InteractionCreated payloads to execute Interactions commands
            _client.MessageReceived += OnMessageReceived;
            _service.CommandExecuted += OnCommandExecuted;
        }
        
        private async Task OnCommandExecuted(Optional<CommandInfo> optional, ICommandContext commandContext, IResult result)
        {
            if (!result.IsSuccess)
            {
                Console.WriteLine($"[{DateTime.Now:hh:mm:ss dd.MM.yyyy}] {commandContext.User.Username}#{commandContext.User.Discriminator}({commandContext.User.Id})" +
                                  $" использовал \"{commandContext.Message}\" в {commandContext.Guild.Name}({commandContext.Guild.Id}) " +
                                  $"> #{commandContext.Channel.Name}({commandContext.Channel.Id}). Команда выдала ошибку: {result.ErrorReason}");
                var error = $"```\n{result.ErrorReason}```● Необработанная ошибка.";
                if (result.ErrorReason != null)
                {
                    if(result.ErrorReason == "Unknown command." && commandContext.Message.Content.Length < 3) return;
                    switch (result.ErrorReason)
                    {
                        case "The input text has too few parameters.":
                            error = "Вами не были указаны все параметры, проверьте и попробуйте еще раз.";
                            break;
                        case "The input text has too many parameters.":
                            error = "Вы указали слишком много параметров, проверьте и попробуйте еще раз.";
                            break;
                        case "Unknown command.":
                            error = $"Неизвестная команда.";
                            break;
                        case "User not found.":
                            error = "Пользователь не найден.";
                            break;
                        case "User requires guild permission Administrator.":
                            error = "Для использования команды тебе необходимы права администратора";
                            break;
                        case "Bot requires guild permission ManageRoles.":
                            error = "Для использования этой команды мне необходимы права на работу с ролями.";
                            break;
                        case "Command precondition group Администратор failed.":
                            error = "Для использования команды тебе необходимы права администратора";
                            break;
                    }
                }
                
            }
            else if (result.IsSuccess)
            {
                Console.WriteLine($"[{DateTime.Now:hh:mm:ss dd.MM.yyyy}] {commandContext.User.Username}#{commandContext.User.Discriminator}({commandContext.User.Id})" +
                                  $" использовал \"{commandContext.Message}\" в {commandContext.Guild.Name}({commandContext.Guild.Id}) " +
                                  $"> #{commandContext.Channel.Name}({commandContext.Channel.Id})");
            }
        }

        private async Task OnMessageReceived(SocketMessage socketMessage)
        {
            if (socketMessage is not SocketUserMessage {Source: MessageSource.User} message) return;

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
            
            var argPos = 0;
            var prefix = config["Prefix"];
            if (!message.HasStringPrefix(prefix, ref argPos) &&
                !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return; 
            
            var context = new SocketCommandContext(_client, message);
            await _service.ExecuteAsync(context,argPos, _provider);

        }
    }
}