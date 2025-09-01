using System.ComponentModel.DataAnnotations;
using Entities.Interfaces;

namespace Entities
{
    public class UserFollow : IEntity, ITrackable
    {
        public int Id { get; set; }
        
       
        public int FollowerId { get; set; }
        
        
        public int FollowingId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
     
        public User Follower { get; set; } = null!;
        public User Following { get; set; } = null!;
    }
}
