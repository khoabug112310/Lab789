using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lab789SalesWebClient.Models
{
    public class OrderViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a product")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid product")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Product Name")]
        public string? ProductName { get; set; }

        [Display(Name = "Order Date")]
        [DataType(DataType.DateTime)]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 100000, ErrorMessage = "Quantity must be between 1 and 100,000")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Amount")]
        [DataType(DataType.Currency)]
        public decimal Amount => Price * Quantity;

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        // Danh sách sản phẩm dùng để nạp vào Dropdownlist khi chọn mua hàng
        public List<Product>? Products { get; set; }
    }
}
