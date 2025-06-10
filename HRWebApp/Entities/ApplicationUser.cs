using Microsoft.AspNetCore.Identity;

namespace HRWebApp.Entities
{

    public class ApplicationUser : IdentityUser<int>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ApplicationUser"/> object.
        /// </summary>
        public ApplicationUser()
        {
            FirstName = String.Empty;
            LastName = String.Empty;
        }
        /// <summary>
        ///  First name of the user     
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        ///  Last name of the user
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        ///  Full name of the user
        /// </summary>
        public string FullName => FirstName + " " + LastName;
        /// <summary>
        /// Shows if the current user is active
        /// </summary>
        /// 
        public bool IsActive { get; set; }
        /// <summary>
        /// Gets or sets the UTC date time when the entity was first created.
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
        /// <summary>
        /// Gets or sets the UTC date time when the entity was last updated.
        /// </summary>
        public DateTime? UpdatedOnUtc { get; set; }
        /// <summary>
        /// Gets or sets the Id of the user that last updated this entity.
        /// </summary>
        public int? UpdatedById { get; set; }
        /// <summary>
        /// Gets or sets the user that last updated this entity.
        /// </summary>
        public virtual ApplicationUser? UpdatedBy { get; set; }
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
        /// <summary>
        /// Object of the role
        /// </summary>
        public ICollection<UserRole> UserRoles { get; set; }
        /// <summary>
        /// User claims
        /// </summary>
        public ICollection<UserClaim> UserClaims { get; set; }
    }
}
