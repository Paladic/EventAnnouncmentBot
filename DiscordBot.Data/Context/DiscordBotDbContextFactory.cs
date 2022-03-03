using System;
using System.Diagnostics;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DiscordBot.Data.Context
{
    public class DiscordBotDbContextFactory : IDesignTimeDbContextFactory<DiscordBotDbContext>
    {
        public DiscordBotDbContext CreateDbContext(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
            
            var name = IsDebug() ? "Debug" : "Default";
            
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseMySql(config.GetConnectionString(name),
                    new MySqlServerVersion(new Version(8, 0, 27)));
            
            return new DiscordBotDbContext(optionsBuilder.Options);
            
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