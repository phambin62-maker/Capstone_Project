using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BE_Capstone_Project.Migrations
{
    /// <inheritdoc />
    public partial class SyncIsReadProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "User",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "Duration",
                table: "Tour",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Notification",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedAuthor",
                table: "News",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "News",
                type: "datetime",
                nullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "CancelStatus",
                table: "CancelCondition",
                type: "tinyint",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "CustomerType",
                table: "BookingCustomer",
                type: "tinyint",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    CompanyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LicenseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TaxCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FoundedYear = table.Column<int>(type: "int", nullable: true),
                    AboutUsTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AboutUsDescription1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AboutUsDescription2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AboutUsImageUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AboutUsImageAlt = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExperienceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExperienceText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HappyTravelersCount = table.Column<int>(type: "int", nullable: true),
                    CountriesCoveredCount = table.Column<int>(type: "int", nullable: true),
                    YearsExperienceCount = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Company__4D913760", x => x.CompanyID);
                });

            migrationBuilder.CreateTable(
                name: "Feature",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Delay = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Feature__3214EC27", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Company");

            migrationBuilder.DropTable(
                name: "Feature");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "User");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "UpdatedAuthor",
                table: "News");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "News");

            migrationBuilder.AlterColumn<string>(
                name: "Duration",
                table: "Tour",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<bool>(
                name: "CancelStatus",
                table: "CancelCondition",
                type: "bit",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerType",
                table: "BookingCustomer",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
