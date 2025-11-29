using System.ComponentModel.DataAnnotations;

public class CreateRentBookDto
{
    [Required]
    public string Title { get; set; }

    public string? Description { get; set; }

    public string? Publisher { get; set; }

    public string? Translator { get; set; }

    public string? Size { get; set; }

    public int Pages { get; set; }

    [Required]
    public decimal Price { get; set; }

    //[Required]
    //public int Quantity { get; set; }

    public IFormFile? ImageFile { get; set; } // ✅ Rename from ImageUrl

    public bool IsHidden { get; set; }  // ➕ thêm vào đây

    [Required]
    [MinLength(1)]
    public List<string> AuthorIds { get; set; }

    [Required]
    [MinLength(1)]
    public List<string> CategoryIds { get; set; }
}
