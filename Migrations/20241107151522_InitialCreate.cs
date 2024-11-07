using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Alias = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.Id);
                    table.UniqueConstraint("AK_Permission_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    RefreshTokenId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Token = table.Column<string>(type: "text", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.RefreshTokenId);
                });

            migrationBuilder.CreateTable(
                name: "RoleMasters",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMasters", x => x.RoleId);
                    table.UniqueConstraint("AK_RoleMasters_RoleName", x => x.RoleName);
                });

            migrationBuilder.CreateTable(
                name: "RouteMaster",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Alias = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Flag = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteMaster", x => x.Id);
                    table.UniqueConstraint("AK_RouteMaster_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "UserMasters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    EmailVerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true),
                    RememberToken = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdateAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PhotoFilename = table.Column<string>(type: "text", nullable: true),
                    PhotoPath = table.Column<string>(type: "text", nullable: true),
                    VerificationToken = table.Column<string>(type: "text", nullable: true),
                    ResetToken = table.Column<string>(type: "text", nullable: true),
                    ResetTokenExpires = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PasswordReset = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Department = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMasters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleHasPermission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleName = table.Column<string>(type: "text", nullable: true),
                    PermissionName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleHasPermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleHasPermission_Permission_PermissionName",
                        column: x => x.PermissionName,
                        principalTable: "Permission",
                        principalColumn: "Name");
                    table.ForeignKey(
                        name: "FK_RoleHasPermission_RoleMasters_RoleName",
                        column: x => x.RoleName,
                        principalTable: "RoleMasters",
                        principalColumn: "RoleName");
                });

            migrationBuilder.CreateTable(
                name: "PermissionHasRoute",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteName = table.Column<string>(type: "text", nullable: true),
                    PermissionName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionHasRoute", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionHasRoute_Permission_PermissionName",
                        column: x => x.PermissionName,
                        principalTable: "Permission",
                        principalColumn: "Name");
                    table.ForeignKey(
                        name: "FK_PermissionHasRoute_RouteMaster_RouteName",
                        column: x => x.RouteName,
                        principalTable: "RouteMaster",
                        principalColumn: "Name");
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRole_RoleMasters_RoleId",
                        column: x => x.RoleId,
                        principalTable: "RoleMasters",
                        principalColumn: "RoleId");
                    table.ForeignKey(
                        name: "FK_UserRole_UserMasters_UserId",
                        column: x => x.UserId,
                        principalTable: "UserMasters",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "RoleMasters",
                columns: new[] { "RoleId", "RoleName" },
                values: new object[,]
                {
                    { new Guid("018f5cbe-e037-73c2-8f35-27c9e4a6b8e5"), "Superadmin" },
                    { new Guid("503dac6f-1c23-496c-a4cd-cc7c7f61dee0"), "User" }
                });

            migrationBuilder.InsertData(
                table: "UserMasters",
                columns: new[] { "Id", "CreatedAt", "Department", "Email", "EmailVerifiedAt", "Name", "Password", "PasswordReset", "PhotoFilename", "PhotoPath", "RememberToken", "ResetToken", "ResetTokenExpires", "UpdateAt", "Username", "VerificationToken" },
                values: new object[] { new Guid("018f5cbe-69d4-78d8-96de-c2deb08e419d"), new DateTimeOffset(new DateTime(2024, 11, 7, 22, 15, 22, 327, DateTimeKind.Unspecified).AddTicks(3236), new TimeSpan(0, 7, 0, 0, 0)), null, "superadmin@example.com", new DateTimeOffset(new DateTime(2024, 11, 7, 22, 15, 22, 87, DateTimeKind.Unspecified).AddTicks(1528), new TimeSpan(0, 7, 0, 0, 0)), "superadmin", "$2b$10$i5IbGQtfkx5sTDrRNZ2IbexSS4DVN.nmKJcpmH3RIqACAENLjsVQS", null, null, null, null, null, null, new DateTimeOffset(new DateTime(2024, 11, 7, 22, 15, 22, 327, DateTimeKind.Unspecified).AddTicks(3294), new TimeSpan(0, 7, 0, 0, 0)), "superadmin", null });

            migrationBuilder.InsertData(
                table: "UserRole",
                columns: new[] { "Id", "RoleId", "UserId" },
                values: new object[] { new Guid("018f5cbf-7db3-7080-8a39-fb1c17574446"), new Guid("018f5cbe-e037-73c2-8f35-27c9e4a6b8e5"), new Guid("018f5cbe-69d4-78d8-96de-c2deb08e419d") });

            migrationBuilder.CreateIndex(
                name: "IX_Permission_Name",
                table: "Permission",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionHasRoute_PermissionName",
                table: "PermissionHasRoute",
                column: "PermissionName");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionHasRoute_RouteName",
                table: "PermissionHasRoute",
                column: "RouteName");

            migrationBuilder.CreateIndex(
                name: "IX_RoleHasPermission_PermissionName",
                table: "RoleHasPermission",
                column: "PermissionName");

            migrationBuilder.CreateIndex(
                name: "IX_RoleHasPermission_RoleName",
                table: "RoleHasPermission",
                column: "RoleName");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMasters_RoleName",
                table: "RoleMasters",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RouteMaster_Flag",
                table: "RouteMaster",
                column: "Flag");

            migrationBuilder.CreateIndex(
                name: "IX_RouteMaster_Name",
                table: "RouteMaster",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMasters_Username",
                table: "UserMasters",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserId",
                table: "UserRole",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermissionHasRoute");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RoleHasPermission");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "RouteMaster");

            migrationBuilder.DropTable(
                name: "Permission");

            migrationBuilder.DropTable(
                name: "RoleMasters");

            migrationBuilder.DropTable(
                name: "UserMasters");
        }
    }
}
