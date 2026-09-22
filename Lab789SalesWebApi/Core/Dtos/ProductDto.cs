namespace Lab789SalesWebApi.Core.Dtos;
using global::Lab789SalesWebApi.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class ProductDto 
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
    public string ProductName { get; set; } = null!;

    [Required]
    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
    public string Category { get; set; } = null!;
    
    [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative integer.")]
    public int Quantity { get; set; }

    // Hỗ trợ cả Image và ImageUrl từ client form
    public IFormFile? Image { get; set; }
    public IFormFile? ImageUrl { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
