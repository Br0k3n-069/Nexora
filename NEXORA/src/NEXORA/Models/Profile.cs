using System;

namespace NEXORA.Models
{
    public class Profile
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; }

        public Profile()
        {
            CreatedAt = DateTime.Now;
        }

        public override string ToString()
        {
            return Name + "  —  " + Address;
        }
    }
}
