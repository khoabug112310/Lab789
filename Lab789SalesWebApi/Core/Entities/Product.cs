using System;
using System.Collections.Generic;

namespace Lab789SalesWebApi.Core.Entities;

public partial class Product
{
    public int Id { get; set; }

    public string ProductName { get; set; } = null!;

    public string Category { get; set; } = null!;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public string? ImageURL { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
