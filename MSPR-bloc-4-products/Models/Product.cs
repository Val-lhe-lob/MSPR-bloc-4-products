using System;
using System.Collections.Generic;

namespace MSPR_bloc_4_products.Models;

public partial class Product
{
    public int IdProduit { get; set; }

    public string Nom { get; set; } = null!;

    public decimal Prix { get; set; }

    public string? Description { get; set; }

    public string? Couleur { get; set; }

    public int? Stock { get; set; }

    public DateTime CreatedAt { get; set; }
}
