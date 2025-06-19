using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MSPR_bloc_4_products.Data;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Supprimer tous les DbContext enregistrés
            var descriptors = services.Where(
                d => d.ServiceType == typeof(DbContextOptions<ProductDbContext>)).ToList();

            foreach (var descriptor in descriptors)
                services.Remove(descriptor);

            // Ajouter InMemory pour les tests
            services.AddDbContext<ProductDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            // Créer la base
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
            if (db.Database.IsInMemory())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }

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