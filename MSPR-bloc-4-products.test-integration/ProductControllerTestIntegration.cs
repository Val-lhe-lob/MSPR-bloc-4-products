using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using MSPR_bloc_4_products.Models;
using System.Net;
using System.Net.Http.Json;

public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        // Fix: Use the base class WebApplicationFactory<TStartup> to call CreateClient
        _client = factory.WithWebHostBuilder(builder => { }).CreateClient();
    }

    [Fact]
    public async Task GetAllProducts_ReturnsOkAndList()
    {
        var response = await _client.GetAsync("/api/Products");
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<Product[]>();
        products.Should().NotBeNull();
        products.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchProducts_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/Products/search?nom=ProduitTest");
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<Product[]>();
        products.Should().NotBeNull();
        products.Should().Contain(p => p.Nom == "ProduitTest");
    }

    [Fact]
    public async Task CreateProduct_ReturnsCreated()
    {
        var product = new Product
        {
            Nom = "NouveauProduit",
            Prix = 20,
            Description = "Test",
            Couleur = "Bleu",
            Stock = 10
        };

        var response = await _client.PostAsJsonAsync("/api/Products", product);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<Product>();
        created.Nom.Should().Be("NouveauProduit");
    }

    [Fact]
    public async Task UpdateProduct_ReturnsNoContent()
    {
        var products = await _client.GetFromJsonAsync<Product[]>("/api/Products");
        var product = products[0];
        product.Prix = 99;

        var response = await _client.PutAsJsonAsync($"/api/Products/{product.IdProduit}", product);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteProduct_ReturnsNoContent()
    {
        var product = new Product
        {
            Nom = "ASupprimer",
            Prix = 5,
            Description = "Suppression",
            Couleur = "Vert",
            Stock = 1
        };
        var createResponse = await _client.PostAsJsonAsync("/api/Products", product);
        var created = await createResponse.Content.ReadFromJsonAsync<Product>();

        var response = await _client.DeleteAsync($"/api/Products/{created.IdProduit}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}