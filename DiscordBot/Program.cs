using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Discordbot.Services;
using Fergun.Interactive;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Discordbot
{
    class Program
    {
        static void Main ( string[] args )
        {
            // One of the more flexable ways to access the configuration data is to use the Microsoft's Configuration model,
            // this way we can avoid hard coding the environment secrets. I opted to use the Json and environment variable providers here.
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            DiscordSocketConfig cf = new DiscordSocketConfig
            {
                 MessageCacheSize = 100, 
                 AlwaysDownloadUsers = true, 
                // LogLevel = LogSeverity.Debug,
                 GatewayIntents = GatewayIntents.All
            };

            RunAsync(cf, config).GetAwaiter().GetResult();
        }

        static async Task RunAsync (DiscordSocketConfig cf, IConfiguration configuration)
        {
            // Dependency injection is a key part of the Interactions framework but it needs to be disposed at the end of the app's lifetime.
            var name = IsDebug() ? "Debug" : "Default";
            using var services = ConfigureServices(cf);
            

            var client = services.GetRequiredService<DiscordSocketClient>();
            var commands = services.GetRequiredService<InteractionService>();
            client.Log += LogAsync;
            commands.Log += LogAsync;

            // Slash Commands and Context Commands are can be automatically registered, but this process needs to happen after the client enters the READY state.
            // Since Global Commands take around 1 hour to register, we should use a test guild to instantly update and test our commands. To determine the method we should
            // register the commands with, we can check whether we are in a DEBUG environment and if we are, we can register the commands to a predetermined test guild.
            client.Ready += async ( ) =>
            {
                if (IsDebug())
                    // Id of the test guild can be provided from the Configuration object
                    await commands.RegisterCommandsToGuildAsync(Convert.ToUInt64(configuration["DebugServerId"]), true);
                else
                    await commands.RegisterCommandsGloballyAsync(true);
            };

            // Here we can initialize the service that will register and execute our commands
            await services.GetRequiredService<StatusHandler>().InitializeAsync();
            await services.GetRequiredService<SlashCommandHandler>().InitializeAsync();
            // Bot token can be provided from the Configuration object we set up earlier
            if(IsDebug())
                await client.LoginAsync(TokenType.Bot, configuration["DebugToken"]);
            else
                await client.LoginAsync(TokenType.Bot, configuration["Token"]);
            await client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

        static Task LogAsync(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Info:
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Critical:
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Debug:
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogSeverity.Error:
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Verbose:
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case LogSeverity.Warning:
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                default:
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
            Console.WriteLine($"[{DateTime.Now:hh:mm:ss dd.MM.yyyy}] [{message.Severity}] {message.Source}: {message.Message}");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            if (message.Exception != null)
            {
                Console.WriteLine("========================\n" + message.Exception + "\n========================");
            }           
            return Task.CompletedTask;
        }
                                                    
        static ServiceProvider ConfigureServices (DiscordSocketConfig configuration)
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(configuration))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton(x => new CommandService())
                .AddSingleton<StatusHandler>()
                .AddSingleton<SlashCommandHandler>()
                .AddSingleton<InteractiveService>()
                
                .BuildServiceProvider();
        }

        static bool IsDebug ( )
        {
            #if DEBUG
                return true;
            #else
                return false;
            #endif
        }
    }
}