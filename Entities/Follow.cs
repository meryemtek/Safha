using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities
{
    public class Follow
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int FollowerId { get; set; }
        
        [Required]
        public int FollowingId { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        [ForeignKey("FollowerId")]
        public virtual User Follower { get; set; } = null!;
        
        [ForeignKey("FollowingId")]
        public virtual User Following { get; set; } = null!;
    }
}

