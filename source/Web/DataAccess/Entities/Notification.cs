using System;
using System.ComponentModel.DataAnnotations;
using AspNetSkeleton.Service.Contract.DataObjects;

namespace AspNetSkeleton.DataAccess.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public NotificationState State { get; set; }

        public DateTime CreatedAt { get; set; }

        [StringLength(64)]
        [Required]
        public string Code { get; set; }

        [Required]
        public string Data { get; set; }
    }
}
