using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MSPR_bloc_4_products.Data;
using System.Linq;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Supprimer le DbContext SQL Server
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ProductDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Ajouter le DbContext InMemory
            services.AddDbContext<ProductDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");

            });

            // Créer la base et ajouter des données de test
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
            db.Database.EnsureCreated();
            if (!db.Products.Any())
            {
                db.Products.Add(new MSPR_bloc_4_products.Models.Product
                {
                    Nom = "ProduitTest",
                    Prix = 10,
                    Description = "Desc",
                    Couleur = "Rouge",
                    Stock = 5,
                    CreatedAt = DateTime.Now
                });
                db.SaveChanges();
            }
        });
    }
}