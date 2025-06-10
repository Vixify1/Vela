using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRWebApp.Entities
{
    public class ApplicationRole : IdentityRole<int>
    {
        /// <summary>
        /// Collection of users containing this role
        /// </summary>
        public ICollection<UserRole> UserRoles { get; set; }
        /// <summary>
        /// Gets or sets the UTC date time when the entity was deleted.
        /// </summary>
        public DateTime? DeletedOnUtc { get; set; }
        /// <summary>
        /// Gets or sets the Id of the user that deleted this entity.
        /// </summary>
        public int? DeletedById { get; set; }
        /// <summary>
        /// Gets or sets the user that deleted this entity.
        /// </summary>
        public virtual ApplicationUser? DeletedBy { get; set; } = null!;
    }
}
