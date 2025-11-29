// Entities/Comment.cs
using ShopThueBanSach.Server.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Comment

{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string CommentId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string Content { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public string BookId { get; set; }

    public string? ParentCommentId { get; set; }

    // Navigation
    public User? User { get; set; }

    [ForeignKey("User")]
    public string? CreatedById { get; set; } // ✅ thay thế cho UserId
}
