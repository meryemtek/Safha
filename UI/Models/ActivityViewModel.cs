using System;
using Entities;

namespace UI.Models
{
    public class ActivityViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public ReadingStatus Status { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}





