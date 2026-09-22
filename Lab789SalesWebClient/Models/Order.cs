using System;
using System.ComponentModel.DataAnnotations;

namespace Lab789SalesWebClient.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a product")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid product")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Display(Name = "Order Date")]
        [DataType(DataType.DateTime)]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 100000, ErrorMessage = "Quantity must be between 1 and 100,000")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        private decimal? _amount;
        [Display(Name = "Amount")]
        [DataType(DataType.Currency)]
        public decimal Amount
        {
            get => _amount ?? (Price * Quantity);
            set => _amount = value;
        }

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }
    }
}
