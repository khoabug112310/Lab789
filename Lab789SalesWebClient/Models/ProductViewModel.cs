using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Lab789SalesWebClient.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Product Name must be between 3 and 100 characters")]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Category must be between 3 and 50 characters")]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0, 100000000, ErrorMessage = "Price must be a positive value")]
        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, 100000, ErrorMessage = "Quantity must be between 0 and 100,000")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Upload Image")]
        public IFormFile? Image { get; set; }

        public IFormFile? ImageUrl { get; set; }
    }
}
