namespace Lab789SalesWebApi.Core.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public partial class OrderDto
{
    [Range(1,int.MaxValue, ErrorMessage = "ProductId must be a positive integer.")]
    public int ProductId { get; set; }

    public DateTime OrderDate { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive integer.")]
    public int Quantity { get; set; }
}
