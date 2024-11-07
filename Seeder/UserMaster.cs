using App.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApi.Models;

namespace WebApi.Seeder;
public class UserMasterSeeder
{
    public void DbSeeder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserMaster>().HasData(
        new UserMaster
        {
            Id = new Guid("018f5cbe-69d4-78d8-96de-c2deb08e419d"),
            Username = "superadmin",
            Name = "superadmin",
            Email = "superadmin@example.com",
            EmailVerifiedAt = DateTime.Now,
            Password = BCrypt.Net.BCrypt.HashPassword("Super@dmin123"),
            CreatedAt = DateTime.Now,
            UpdateAt = DateTime.Now,
        }
        );
    }

}