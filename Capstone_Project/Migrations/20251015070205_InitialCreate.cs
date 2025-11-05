using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BE_Capstone_Project.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CancelCondition",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MinDaysBeforeTrip = table.Column<byte>(type: "tinyint", nullable: true),
                    RefundPercent = table.Column<byte>(type: "tinyint", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    CancelStatus = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CancelCo__3214EC275035C402", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Location__3214EC271C940BCA", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Role__3214EC27574568B1", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TourCategory",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TourCate__3214EC2742F6D3C1", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Image = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RoleID = table.Column<int>(type: "int", nullable: false),
                    UserStatus = table.Column<byte>(type: "tinyint", nullable: true),
                    PasswordResetTokenHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordResetExpires = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User__3214EC274AAAED85", x => x.ID);
                    table.ForeignKey(
                        name: "FK__User__RoleID__5441852A",
                        column: x => x.RoleID,
                        principalTable: "Role",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Tour",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Duration = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartLocationID = table.Column<int>(type: "int", nullable: false),
                    EndLocationID = table.Column<int>(type: "int", nullable: false),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    CancelConditionID = table.Column<int>(type: "int", nullable: false),
                    ChildDiscount = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    GroupDiscount = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    GroupNumber = table.Column<byte>(type: "tinyint", nullable: true),
                    TourStatus = table.Column<bool>(type: "bit", nullable: true),
                    MinSeats = table.Column<short>(type: "smallint", nullable: true),
                    MaxSeats = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Tour__3214EC27922073D7", x => x.ID);
                    table.ForeignKey(
                        name: "FK__Tour__CancelCond__60A75C0F",
                        column: x => x.CancelConditionID,
                        principalTable: "CancelCondition",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK__Tour__CategoryID__5FB337D6",
                        column: x => x.CategoryID,
                        principalTable: "TourCategory",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK__Tour__EndLocatio__5EBF139D",
                        column: x => x.EndLocationID,
                        principalTable: "Location",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK__Tour__StartLocat__5DCAEF64",
                        column: x => x.StartLocationID,
                        principalTable: "Location",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Chat",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffID = table.Column<int>(type: "int", nullable: false),
                    CustomerID = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChatType = table.Column<byte>(type: "tinyint", nullable: true),
                    SentDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Chat__3214EC27C2840407", x => x.ID);
                    table.ForeignKey(
                        name: "FK__Chat__CustomerID__5812160E",
                        column: x => x.CustomerID,
                        principalTable: "User",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK__Chat__StaffID__571DF1D5",
                        column: x => x.StaffID,
                        principalTable: "User",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    NewsStatus = table.Column<byte>(type: "tinyint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__News__3214EC27F261B932", x => x.ID);
                    table.ForeignKey(
                        name: "FK__News__UserID__5535A963",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    NotificationType = table.Column<byte>(type: "tinyint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__3214EC278158AC28", x => x.ID);
                    table.ForeignKey(
                        name: "FK__Notificat__UserI__5629CD9C",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TourImage",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TourID = table.Column<int>(type: "int", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TourImag__3214EC275B48A654", x => x.ID);
                    table.ForeignKey(
                        name: "FK__TourImage__TourI__619B8048",
                        column: x => x.TourID,
                        principalTable: "Tour",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TourPriceHistory",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TourID = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ChildrenDiscount = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    GroupDiscount = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    GroupNumber = table.Column<int>(type: "int", nullable: true),
                    StartPriceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndPriceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TourPric__3214EC27E8D812A6", x => x.ID);
                    table.ForeignKey(
                        name: "FK__TourPrice__TourI__66603565",
                        column: x => x.TourID,
                        principalTable: "Tour",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TourSchedule",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TourID = table.Column<int>(type: "int", nullable: false),
                    DepartureDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ArrivalDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ScheduleStatus = table.Column<byte>(type: "tinyint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TourSche__3214EC27D9098D1C", x => x.ID);
                    table.ForeignKey(
                        name: "FK__TourSched__TourI__628FA481",
                        column: x => x.TourID,
                        principalTable: "Tour",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Wishlist",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    TourID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Wishlist__3214EC2741A85014", x => x.ID);
                    table.ForeignKey(
                        name: "FK__Wishlist__TourID__5CD6CB2B",
                        column: x => x.TourID,
                        principalTable: "Tour",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK__Wishlist__UserID__5BE2A6F2",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Booking",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    TourScheduleID = table.Column<int>(type: "int", nullable: false),
                    PaymentStatus = table.Column<byte>(type: "tinyint", nullable: true),
                    RefundDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RefundAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    PaymentMethod = table.Column<byte>(type: "tinyint", nullable: true),
                    PaymentDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpirationTime = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CertificateID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    BookingDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    BookingStatus = table.Column<byte>(type: "tinyint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Booking__3214EC27666AD904", x => x.ID);
                    table.ForeignKey(
                        name: "FK__Booking__TourSch__6477ECF3",
                        column: x => x.TourScheduleID,
                        principalTable: "TourSchedule",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK__Booking__UserID__6383C8BA",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "BookingCustomer",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingID = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    IdentityID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CustomerType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BookingC__3214EC271E35F1B3", x => x.ID);
                    table.ForeignKey(
                        name: "FK__BookingCu__Booki__656C112C",
                        column: x => x.BookingID,
                        principalTable: "Booking",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Review",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    TourID = table.Column<int>(type: "int", nullable: false),
                    BookingID = table.Column<int>(type: "int", nullable: false),
                    Stars = table.Column<byte>(type: "tinyint", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ReviewStatus = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Review__3214EC2742A083D7", x => x.ID);
                    table.ForeignKey(
                        name: "FK__Review__BookingI__5AEE82B9",
                        column: x => x.BookingID,
                        principalTable: "Booking",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK__Review__TourID__59FA5E80",
                        column: x => x.TourID,
                        principalTable: "Tour",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK__Review__UserID__59063A47",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Booking_TourScheduleID",
                table: "Booking",
                column: "TourScheduleID");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_UserID",
                table: "Booking",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_BookingCustomer_BookingID",
                table: "BookingCustomer",
                column: "BookingID");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_CustomerID",
                table: "Chat",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_StaffID",
                table: "Chat",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "IX_News_UserID",
                table: "News",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserID",
                table: "Notification",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Review_BookingID",
                table: "Review",
                column: "BookingID");

            migrationBuilder.CreateIndex(
                name: "IX_Review_TourID",
                table: "Review",
                column: "TourID");

            migrationBuilder.CreateIndex(
                name: "IX_Review_UserID",
                table: "Review",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Tour_CancelConditionID",
                table: "Tour",
                column: "CancelConditionID");

            migrationBuilder.CreateIndex(
                name: "IX_Tour_CategoryID",
                table: "Tour",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Tour_EndLocationID",
                table: "Tour",
                column: "EndLocationID");

            migrationBuilder.CreateIndex(
                name: "IX_Tour_StartLocationID",
                table: "Tour",
                column: "StartLocationID");

            migrationBuilder.CreateIndex(
                name: "IX_TourImage_TourID",
                table: "TourImage",
                column: "TourID");

            migrationBuilder.CreateIndex(
                name: "IX_TourPriceHistory_TourID",
                table: "TourPriceHistory",
                column: "TourID");

            migrationBuilder.CreateIndex(
                name: "IX_TourSchedule_TourID",
                table: "TourSchedule",
                column: "TourID");

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleID",
                table: "User",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlist_TourID",
                table: "Wishlist",
                column: "TourID");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlist_UserID",
                table: "Wishlist",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingCustomer");

            migrationBuilder.DropTable(
                name: "Chat");

            migrationBuilder.DropTable(
                name: "News");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "Review");

            migrationBuilder.DropTable(
                name: "TourImage");

            migrationBuilder.DropTable(
                name: "TourPriceHistory");

            migrationBuilder.DropTable(
                name: "Wishlist");

            migrationBuilder.DropTable(
                name: "Booking");

            migrationBuilder.DropTable(
                name: "TourSchedule");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Tour");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "CancelCondition");

            migrationBuilder.DropTable(
                name: "TourCategory");

            migrationBuilder.DropTable(
                name: "Location");
        }
    }
}
