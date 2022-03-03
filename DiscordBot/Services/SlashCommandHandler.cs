using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using IResult = Discord.Interactions.IResult;

namespace Discordbot.Services
{
    public class SlashCommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;

        public SlashCommandHandler(DiscordSocketClient client, InteractionService commands, 
            IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
        }

        public async Task InitializeAsync ( )
        {
            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // Process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;

            // Process the command execution results 
            _commands.SlashCommandExecuted += SlashCommandExecuted;
            _commands.ContextCommandExecuted += ContextCommandExecuted;
            _commands.ComponentCommandExecuted += ComponentCommandExecuted;
        }

        private Task ComponentCommandExecuted (ComponentCommandInfo arg1, IInteractionContext arg2, IResult result)
        {
            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    case InteractionCommandError.UnknownCommand:
                        // implement
                        break;
                    case InteractionCommandError.BadArgs:
                        // implement
                        break;
                    case InteractionCommandError.Exception:
                        // implement
                        break;
                    case InteractionCommandError.Unsuccessful:
                        // implement
                        break;
                }
            }    

            Console.WriteLine($"[{DateTime.Now:hh:mm:ss dd.MM.yyyy}] {arg2.User.Username}#{arg2.User.Discriminator}({arg2.User.Id})" +
                              $" нажал на \"{arg1.Name}\" в {arg2.Guild.Name}({arg2.Guild.Id}) " +
                              $"> #{arg2.Channel.Name}({arg2.Channel.Id})");
            return Task.CompletedTask;
        }

        private Task ContextCommandExecuted (ContextCommandInfo arg1, IInteractionContext arg2, IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                switch (arg3.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    case InteractionCommandError.UnknownCommand:
                        // implement
                        break;
                    case InteractionCommandError.BadArgs:
                        // implement
                        break;
                    case InteractionCommandError.Exception:
                        // implement
                        break;
                    case InteractionCommandError.Unsuccessful:
                        // implement
                        break;
                }
            }

            Console.WriteLine("123");
            return Task.CompletedTask;
        }
        
        private async Task SlashCommandExecuted (SlashCommandInfo arg1, IInteractionContext arg2, IResult arg3)
        {
            // await arg2.Interaction.DeferAsync(false);
            string ermsg = "";
            if (!arg3.IsSuccess)
            {
                string error;
                switch (arg3.ErrorReason)
                {
                    case "The input text has too few parameters.":
                        error = "Вами не были указаны все параметры, проверьте и попробуйте еще раз.";
                        break;
                    case "The input text has too many parameters.":
                        error = "Вы указали слишком много параметров, проверьте и попробуйте еще раз.";
                        break;
                    case "Unknown command.":
                        error = "Неизвестная команда.";
                        break;
                    case "User not found.":
                        error = "Пользователь не найден.";
                        break;
                    case "User requires guild permission Administrator.":
                        error = "Для использования команды тебе необходимы права администратора.";
                        break;
                    case "Bot requires guild permission ManageRoles.":
                        error = "Для использования этой команды мне необходимы права на работу с ролями.";
                        break;
                    case "Command precondition group Администратор failed.":
                        error = "Для использования команды тебе необходимы права администратора.";
                        break;
                    case "Module precondition group Администратор failed.":
                        error = "Для использования команды тебе необходимы права администратора.";
                        break;
                    case "Command precondition group Модерация failed.":
                        error = "Для использования команды тебе необходимы права \"Отправить пользователя думать над своим поведением\"";
                        break;
                    default:
                        error = $"```\n{arg3.ErrorReason}```\n● Необработанная ошибка. Сообщите об этом через команду `/бот связь-с-разработчиком context:репорт опишите: `";
                        break;
                }

                string text = error;
                switch (arg3.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        text = error;
                        //await arg2.Interaction.RespondAsync(embed: embed);
                        break;
                    case InteractionCommandError.UnknownCommand:
                        text = error;
                        //await arg2.Interaction.RespondAsync(embed: embed);
                        break;
                    case InteractionCommandError.BadArgs:
                        text = error;
                        //await arg2.Interaction.RespondAsync(embed: embed);
                        break;
                    case InteractionCommandError.Exception:
                        text = error;
                        //await arg2.Interaction.RespondAsync(embed: embed);
                        break;
                    case InteractionCommandError.Unsuccessful:
                        text = error;
                        //await arg2.Interaction.RespondAsync(embed: embed);
                        break;
                }

                var embed = new EmbedBuilder()
                    .WithAuthor(arg2.Client.CurrentUser.Username)
                    .WithDescription(text);
                try
                {
                    await arg2.Interaction.RespondAsync(embed: embed.Build());
                }
                catch (Exception)
                {
                    await arg2.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
                }
                ermsg = $". Команда выдала ошибку: {arg3.ErrorReason}";
            }
            
            Console.WriteLine($"[{DateTime.Now:hh:mm:ss dd.MM.yyyy}] {arg2.User.Username}#{arg2.User.Discriminator}({arg2.User.Id})" +
                              $" использовал \"{arg1.Name}\" в {arg2.Guild.Name}({arg2.Guild.Id}) " +
                              $"> #{arg2.Channel.Name}({arg2.Channel.Id})" + ermsg);
            // return Task.CompletedTask;
        }

        private async Task HandleInteraction (SocketInteraction command)
        {
            if (command.Type != InteractionType.ApplicationCommand)
            {
                return;
            }
            try
            {
                // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
                var ctx = new SocketInteractionContext(_client, command);
                await _commands.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if(command.Type == InteractionType.ApplicationCommand)
                    await command.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}