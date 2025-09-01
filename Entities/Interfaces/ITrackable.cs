namespace Entities.Interfaces
{
   
    public interface ITrackable
    {
       
        DateTime CreatedAt { get; set; }

        
        DateTime? UpdatedAt { get; set; }

       
        bool IsActive { get; set; }
    }
}

