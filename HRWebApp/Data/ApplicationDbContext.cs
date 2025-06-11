using HRWebApp.Entities;
using HRWebApp.Mappings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HRWebApp.Data
{

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int, IdentityUserClaim<int>,
     UserRole, IdentityUserLogin<int>,
     IdentityRoleClaim<int>, IdentityUserToken<int>>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        //public void ConfigureServices(IServiceCollection services)
        //{
        //    services.AddControllersWithViews();

        //    services.AddDbContext<ApplicationDbContext>(options =>
        //        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
        //}

        #region SaveChanges Overrides

        /// <inheritdoc cref="IdentityDbContext"/>
        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        /// <inheritdoc cref="IdentityDbContext"/>
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        /// <inheritdoc cref="IdentityDbContext"/>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc cref="IdentityDbContext"/>
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        #endregion SaveChanges Overrides

        #region Utilities

        /// <inheritdoc cref="DbContext"/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //dynamically load all entity and query type configurations
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var typeConfigurations = assemblies.SelectMany(a => a.GetTypes()).Where(type =>
                (type.BaseType?.IsGenericType ?? false)
                && type.BaseType.GetGenericTypeDefinition() == typeof(Mappings.EntityTypeConfiguration<>));

            var mapList = new List<IMappingConfiguration?>();
            foreach (var typeConfiguration in typeConfigurations)
            {
                var configuration = Activator.CreateInstance(typeConfiguration) as IMappingConfiguration;
                mapList.Add(configuration);
            }
            foreach (var map in mapList)
            {
                map?.ApplyConfiguration(modelBuilder);
            }
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UserRole>(userRole =>
            {
                userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

                userRole.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                userRole.HasOne(ur => ur.User)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<UserClaim>(userRole =>
            {
                userRole.HasOne(ur => ur.User)
                    .WithMany(r => r.UserClaims)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ApplicationUser>(applicationUser =>
            {
                applicationUser.HasKey(au => au.Id);
                applicationUser.Property(mapping => mapping.FirstName).IsRequired();
                applicationUser.Property(mapping => mapping.LastName).IsRequired();

                modelBuilder.Entity<ApplicationUser>()
                            .HasMany(u => u.UserRoles)
                            .WithOne(ur => ur.User)
                            .IsRequired(false)
                            .OnDelete(DeleteBehavior.Cascade);

                applicationUser.HasOne(au => au.UpdatedBy)
                               .WithMany()
                               .HasForeignKey(au => au.UpdatedById)
                               .OnDelete(DeleteBehavior.Restrict);

                applicationUser.HasOne(au => au.DeletedBy)
                               .WithMany()
                               .HasForeignKey(au => au.DeletedById)
                               .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ApplicationRole>(applicationRole =>
            {
                applicationRole.HasKey(ar => ar.Id);
            });
        }

        #endregion Utilities

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>().HavePrecision(22, 6);

            base.ConfigureConventions(configurationBuilder);
        }

        #region Methods

        /// <summary>
        /// Creates a DbSet that can be used to query and save instances of entity.
        /// </summary>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <returns>A set for the given entity type.</returns>
        public virtual new DbSet<TEntity> Set<TEntity>() where TEntity : class
        {

            return base.Set<TEntity>();
        }

        /// <summary>
        /// Detach an entity from the context.
        /// </summary>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <param name="entity">Entity.</param>
        public virtual void Detach<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entityEntry = Entry(entity);
            if (entityEntry == null)
                return;

            //set the entity is not being tracked by the context
            entityEntry.State = EntityState.Detached;
        }




        #endregion Methods


    }
}
