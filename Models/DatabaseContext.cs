using App.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Collections.Generic;
using WebApi.Seeder;

namespace WebApi.Models
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {

        }
        public DbSet<UserMaster> UserMasters { get; set; } = null!;
        public DbSet<UserRole> UserRole { get; set; } = null!;
        public DbSet<RoleMaster> RoleMasters { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<Permission> Permission { get; set; } = null!;
        public DbSet<RoleHasPermission> RoleHasPermission { get; set; } = null!;
        public DbSet<RouteMaster> RouteMaster { get; set; } = null!;
        public DbSet<PermissionHasRoute> PermissionHasRoute { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Konfigurasi relasi UserMaster dan UserRole
            modelBuilder.Entity<UserMaster>()
                .HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            // Konfigurasi relasi UserRole dan RoleMaster
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<RoleHasPermission>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.Permissions)
                .HasForeignKey(ur => ur.RoleName)
                .HasPrincipalKey(p => p.RoleName);

            modelBuilder.Entity<RoleHasPermission>()
                .HasOne(ur => ur.Permission)
                .WithMany(r => r.Roles)
                .HasForeignKey(ur => ur.PermissionName)
                .HasPrincipalKey(p => p.Name);

            modelBuilder.Entity<PermissionHasRoute>()
                .HasOne(ur => ur.Route)
                .WithMany(r => r.Permissions)
                .HasForeignKey(ur => ur.RouteName)
                .HasPrincipalKey(p => p.Name);

            modelBuilder.Entity<PermissionHasRoute>()
                .HasOne(ur => ur.Permission)
                .WithMany(r => r.Routes)
                .HasForeignKey(ur => ur.PermissionName)
                .HasPrincipalKey(p => p.Name);

            // DB SEEDER
            var UserMaster = new UserMasterSeeder { };
            UserMaster.DbSeeder(modelBuilder);
            var RoleMaster = new RoleMasterSeeder { };
            RoleMaster.DbSeeder(modelBuilder);
            var UserRole = new UserRoleSeeder { };
            UserRole.DbSeeder(modelBuilder);
        }

    }

}