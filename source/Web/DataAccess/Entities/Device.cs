using System;
using System.ComponentModel.DataAnnotations;

namespace AspNetSkeleton.DataAccess.Entities
{
    public class Device
    {
        public int UserId { get; set; }

        [StringLength(172)]
        public string DeviceId { get; set; }

        public virtual Profile Profile { get; set; }

        public DateTime ConnectedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        [StringLength(20)]
        public string DeviceName { get; set; }
    }
}