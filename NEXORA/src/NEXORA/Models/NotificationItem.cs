using System;

namespace NEXORA.Models
{
    public sealed class NotificationItem
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }

        public NotificationItem()
        {
            CreatedAt = DateTime.Now;
        }
    }
}