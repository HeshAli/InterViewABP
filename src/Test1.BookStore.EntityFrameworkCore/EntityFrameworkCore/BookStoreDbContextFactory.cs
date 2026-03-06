using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Test1.BookStore.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class BookStoreDbContextFactory : IDesignTimeDbContextFactory<BookStoreDbContext>
{
    public BookStoreDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        BookStoreEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<BookStoreDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new BookStoreDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Test1.BookStore.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
