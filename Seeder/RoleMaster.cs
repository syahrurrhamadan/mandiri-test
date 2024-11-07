using App.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApi.Models;

namespace WebApi.Seeder;
public class RoleMasterSeeder
{
    public void DbSeeder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoleMaster>().HasData(
        new RoleMaster
        {
            RoleId = new Guid("018f5cbe-e037-73c2-8f35-27c9e4a6b8e5"),
            RoleName = "Superadmin"
        },
        new RoleMaster
        {
            RoleId = new Guid("503DAC6F-1C23-496C-A4CD-CC7C7F61DEE0"),
            RoleName = "User"
        }
        );
    }

}