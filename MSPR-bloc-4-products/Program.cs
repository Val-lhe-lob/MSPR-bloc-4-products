using Microsoft.EntityFrameworkCore;
using MSPR_bloc_4_products.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Ajouter le DbContext ici
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();