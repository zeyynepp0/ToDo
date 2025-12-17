using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get { return FirstName + " " + LastName; } }
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public bool GeneralNotificationActive { get; set; } = false;

        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

     
        public DateTime RegisteredDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime? EditedDate { get; set; }
        //public string? ProfilePicture { get; set; }
        public int NotificationCount { get; set; } = 0;


        public bool IsDeleted { get; set; } = false;

        //ilişkiler (Foreign Key)
        public Guid RoleId { get; set; }
        public Role Role { get; set; }
    }
}