using Capstone.API.Middleware;
using Capstone.DAL.Repository;
using Microsoft.EntityFrameworkCore;

namespace Capstone.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            var resolvedConnectionString = ResolveSqliteConnectionString(
                rawConnectionString,
                builder.Environment.ContentRootPath); 

            builder.Services.AddDbContext<HelpDeskDbContext>(options =>
                options.UseSqlite(resolvedConnectionString));

            builder.Services.AddScoped<IRepository, Repository>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static string ResolveSqliteConnectionString(string connectionString, string contentRootPath)
        {
            const string prefix = "Data Source=";
            if(!connectionString.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return connectionString;
            }
            
            var dataSource = connectionString[prefix.Length..].Trim();
            if(Path.IsPathRooted(dataSource))
            {
                return connectionString;
            }
            
            var fullPath = Path.GetFullPath(Path.Combine(contentRootPath, dataSource));
            return $"{prefix}{fullPath}";
        }
    }
}
