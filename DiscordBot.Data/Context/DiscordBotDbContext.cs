using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Data.Context
{
    public class DiscordBotDbContext : DbContext
    {
        public DiscordBotDbContext(DbContextOptions options)
            : base(options)
        {
        }
        // dotnet ef migrations add name
        // dotnet ef database update
        
    }
}