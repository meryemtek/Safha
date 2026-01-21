namespace Entities
{
    public class Class1
    {

    }

    public class QuoteLike : Interfaces.IEntity, Interfaces.ITrackable
    {
        public int Id { get; set; }
        public int QuoteId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public Quote Quote { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
