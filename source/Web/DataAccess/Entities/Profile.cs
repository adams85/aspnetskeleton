using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AspNetSkeleton.DataAccess.Entities
{
    public class Profile
    {
        public int UserId { get; set; }

        public virtual User User { get; set; }

        [StringLength(100)]
        public string FirstName { get; set; }

        [StringLength(100)]
        public string LastName { get; set; }

        [StringLength(50)]
        public string PhoneNumber { get; set; }

        public int DeviceLimit { get; set; }

        ICollection<Device> _devices;
        public virtual ICollection<Device> Devices
        {
            get => _devices ?? (_devices = new HashSet<Device>());
            set => _devices = value;
        }
    }
}