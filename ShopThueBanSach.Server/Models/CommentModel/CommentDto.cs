namespace ShopThueBanSach.Server.Models.CommentModel
{
    public class CommentDto
    {
        public string? CommentId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public string BookId { get; set; }
        public string? ParentCommentId { get; set; }
        // Không cần UserId nữa
        public string? CreatedById { get; set; }
    }
}
