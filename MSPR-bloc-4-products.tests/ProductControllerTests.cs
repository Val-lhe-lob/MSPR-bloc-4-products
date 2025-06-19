using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MSPR_bloc_4_products.Controllers;
using MSPR_bloc_4_products.Data;
using MSPR_bloc_4_products.Models;

namespace MSPR_bloc_4_products.Tests
{
    public class ProductControllerTests
    {
        private ProductDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ProductDbContext(options);
        }

        [Fact]
        public async Task GetAllProducts_ReturnsAllProducts()
        {
            // Arrange
            var context = GetDbContext();
            context.Products.Add(new Product { IdProduit = 1, Nom = "Produit1", Prix = 10 });
            context.Products.Add(new Product { IdProduit = 2, Nom = "Produit2", Prix = 20 });
            context.SaveChanges();

            var controller = new ProductsController(context);

            // Act
            var result = await controller.GetAllProducts();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
            var products = Assert.IsType<List<Product>>(actionResult.Value);
            Assert.Equal(2, products.Count);
        }

        [Fact]
        public async Task GetProductById_ReturnsProduct_WhenExists()
        {
            var context = GetDbContext();
            context.Products.Add(new Product { IdProduit = 1, Nom = "Produit1", Prix = 10 });
            context.SaveChanges();

            var controller = new ProductsController(context);

            var result = await controller.GetProductById(1);

            var actionResult = Assert.IsType<ActionResult<Product>>(result);
            var product = Assert.IsType<Product>(actionResult.Value);
            Assert.Equal("Produit1", product.Nom);
        }

        [Fact]
        public async Task GetProductById_ReturnsNotFound_WhenNotExists()
        {
            var context = GetDbContext();
            var controller = new ProductsController(context);

            var result = await controller.GetProductById(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task SearchProducts_ReturnsMatchingProducts()
        {
            var context = GetDbContext();
            context.Products.Add(new Product { IdProduit = 1, Nom = "TestProduit", Prix = 10 });
            context.Products.Add(new Product { IdProduit = 2, Nom = "Autre", Prix = 20 });
            context.SaveChanges();

            var controller = new ProductsController(context);

            var result = await controller.SearchProducts("Test");

            var actionResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
            var products = Assert.IsType<List<Product>>(actionResult.Value);
            Assert.Single(products);
            Assert.Equal("TestProduit", products[0].Nom);
        }

        [Fact]
        public async Task SearchProducts_ReturnsBadRequest_WhenNomIsNull()
        {
            var context = GetDbContext();
            var controller = new ProductsController(context);

            var result = await controller.SearchProducts(null);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateProduct_AddsProduct()
        {
            var context = GetDbContext();
            var controller = new ProductsController(context);

            var product = new Product { IdProduit = 1, Nom = "Nouveau", Prix = 15 };

            var result = await controller.CreateProduct(product);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdProduct = Assert.IsType<Product>(createdAtActionResult.Value);
            Assert.Equal("Nouveau", createdProduct.Nom);
            Assert.NotEqual(default(DateTime), createdProduct.CreatedAt);
        }

        [Fact]
        public async Task UpdateProduct_UpdatesProduct_WhenExists()
        {
            var context = GetDbContext();
            context.Products.Add(new Product { IdProduit = 1, Nom = "Ancien", Prix = 10 });
            context.SaveChanges();

            var controller = new ProductsController(context);

            var updatedProduct = new Product { IdProduit = 1, Nom = "Modifié", Prix = 20 };

            var result = await controller.UpdateProduct(1, updatedProduct);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal("Modifié", context.Products.Find(1).Nom);
        }

        [Fact]
        public async Task UpdateProduct_ReturnsBadRequest_WhenIdMismatch()
        {
            var context = GetDbContext();
            var controller = new ProductsController(context);

            var product = new Product { IdProduit = 2, Nom = "Produit", Prix = 10 };

            var result = await controller.UpdateProduct(1, product);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task UpdateProduct_ReturnsNotFound_WhenProductNotExists()
        {
            var context = GetDbContext();
            var controller = new ProductsController(context);

            var product = new Product { IdProduit = 1, Nom = "Produit", Prix = 10 };

            var result = await controller.UpdateProduct(1, product);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_RemovesProduct_WhenExists()
        {
            var context = GetDbContext();
            context.Products.Add(new Product { IdProduit = 1, Nom = "Produit", Prix = 10 });
            context.SaveChanges();

            var controller = new ProductsController(context);

            var result = await controller.DeleteProduct(1);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(context.Products);
        }

        [Fact]
        public async Task DeleteProduct_ReturnsNotFound_WhenNotExists()
        {
            var context = GetDbContext();
            var controller = new ProductsController(context);

            var result = await controller.DeleteProduct(99);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}