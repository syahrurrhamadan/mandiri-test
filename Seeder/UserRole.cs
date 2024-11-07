using App.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApi.Models;

namespace WebApi.Seeder;
public class UserRoleSeeder
{
    public void DbSeeder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>().HasData(
        new UserRole
        {
            Id = new Guid("018f5cbf-7db3-7080-8a39-fb1c17574446"),
            UserId = new Guid("018f5cbe-69d4-78d8-96de-c2deb08e419d"),
            RoleId = new Guid("018f5cbe-e037-73c2-8f35-27c9e4a6b8e5"),
        }
        );
    }

}