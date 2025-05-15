using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MetroShip.Repository.Migrations
{
    /// <inheritdoc />
    public partial class init_052025_313 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParcelCategories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CategoryName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsBulk = table.Column<bool>(type: "boolean", nullable: false),
                    WeightLimitKg = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    VolumeLimitCm3 = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    LengthLimitCm = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    WidthLimitCm = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    HeightLimitCm = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    RegionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RegionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfigs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ConfigKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConfigValue = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ConfigType = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BankId = table.Column<int>(type: "integer", maxLength: 6, nullable: true),
                    AccountNo = table.Column<string>(type: "character varying(19)", maxLength: 19, nullable: true),
                    AccountName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Verified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OTP = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MetroLines",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    RegionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LineNameVi = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    LineNameEn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    LineCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LineType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    LineOwner = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TotalKm = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    TotalStations = table.Column<int>(type: "integer", nullable: false),
                    MinHeadwayMin = table.Column<int>(type: "integer", nullable: true),
                    MaxHeadwayMin = table.Column<int>(type: "integer", nullable: true),
                    RouteTimeMin = table.Column<int>(type: "integer", nullable: true),
                    DwellTimeMin = table.Column<int>(type: "integer", nullable: true),
                    TopSpeedKmH = table.Column<int>(type: "integer", nullable: true),
                    TopSpeedUdgKmH = table.Column<int>(type: "integer", nullable: true),
                    CarriagesPerTrain = table.Column<int>(type: "integer", nullable: true),
                    CarriageLenghtMeter = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    CarriageWidthMeter = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    CarriageHeightMeter = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    CarriageWeightTons = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    BasePriceVndPerKm = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ColorHex = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetroLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetroLines_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    StationCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StationNameVi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StationNameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsUnderground = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RegionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stations_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ToUserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NotificationType = table.Column<int>(type: "integer", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    Expires = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReportType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shipments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    TrackingCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DepartureStationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ArrivalStationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ShipmentStatus = table.Column<int>(type: "integer", nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ScheduledDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TimeSlot = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BookingAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PickupTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SenderId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SenderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SenderPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RecipientId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RecipientName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecipientPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RecipientNationalId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    PaymentTransactionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PaymentDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PaymentAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    InsuranceAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shipments_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetroTimeSlots",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: true),
                    SpecialDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsAbnormal = table.Column<bool>(type: "boolean", nullable: false),
                    OpenTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    CloseTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Shift = table.Column<int>(type: "integer", nullable: false),
                    MetroLineId = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetroTimeSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetroTimeSlots_MetroLines_MetroLineId",
                        column: x => x.MetroLineId,
                        principalTable: "MetroLines",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    RouteCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FromStationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ToStationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RouteNameVi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RouteNameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SeqOrder = table.Column<int>(type: "integer", nullable: false),
                    TravelTimeMin = table.Column<int>(type: "integer", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    LengthKm = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Routes_MetroLines_LineId",
                        column: x => x.LineId,
                        principalTable: "MetroLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Routes_Stations_FromStationId",
                        column: x => x.FromStationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Routes_Stations_ToStationId",
                        column: x => x.ToStationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Parcels",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ParcelCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ShipmentId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ParcelCategoryId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    LengthCm = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    WidthCm = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    HeightCm = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    IsBulk = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parcels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parcels_ParcelCategories_ParcelCategoryId",
                        column: x => x.ParcelCategoryId,
                        principalTable: "ParcelCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Parcels_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentTrackings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ShipmentId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EventTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentTrackings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipmentTrackings_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShipmentTrackings_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupportingTickets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ShipmentId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RespondedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportingTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportingTickets_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupportingTickets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetroSchedules",
                columns: table => new
                {
                    LineId = table.Column<string>(type: "text", nullable: false),
                    TimeSlotId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetroSchedules", x => new { x.LineId, x.TimeSlotId });
                    table.ForeignKey(
                        name: "FK_MetroSchedules_MetroLines_LineId",
                        column: x => x.LineId,
                        principalTable: "MetroLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetroSchedules_MetroTimeSlots_TimeSlotId",
                        column: x => x.TimeSlotId,
                        principalTable: "MetroTimeSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentItineraries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ShipmentId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LegOrder = table.Column<int>(type: "integer", nullable: false),
                    RouteId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EstimatedDeparture = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EstimatedArrival = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentItineraries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipmentItineraries_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShipmentItineraries_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0bfab0b7-75e9-4024-b5c7-18c3478f61a8", "5c86df02-195b-41f3-86b5-fae2562d7fc6", "Staff", "STAFF" },
                    { "ab5b94b3-ec6a-4a9b-8097-8d55f51e29b7", "29cced99-ac0e-4d34-b4f8-361de4ef0ef5", "Customer", "CUSTOMER" },
                    { "e7c3bf55-d9a7-44db-98cc-0ba0a136e7a5", "3871ddf7-da32-40c4-b356-57a874a32258", "Admin", "ADMIN" }
                });

            migrationBuilder.CreateIndex(
                name: "Index_CreatedAt1",
                table: "MetroLines",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "Index_Id1",
                table: "MetroLines",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MetroLines_RegionId",
                table: "MetroLines",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_MetroSchedules_TimeSlotId",
                table: "MetroSchedules",
                column: "TimeSlotId");

            migrationBuilder.CreateIndex(
                name: "Index_CreatedAt2",
                table: "MetroTimeSlots",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "Index_Id2",
                table: "MetroTimeSlots",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MetroTimeSlots_MetroLineId",
                table: "MetroTimeSlots",
                column: "MetroLineId");

            migrationBuilder.CreateIndex(
                name: "Index_CreatedAt3",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "Index_Id3",
                table: "Notifications",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ToUserId",
                table: "Notifications",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "Index_CreatedAt5",
                table: "ParcelCategories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "Index_Id5",
                table: "ParcelCategories",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Index_CreatedAt4",
                table: "Parcels",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "Index_Id4",
                table: "Parcels",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parcels_ParcelCategoryId",
                table: "Parcels",
                column: "ParcelCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Parcels_ShipmentId",
                table: "Parcels",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "UQ__ParcelCode__A2A2A54B1E001898",
                table: "Parcels",
                column: "ParcelCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Index_CreatedAt",
                table: "RefreshTokens",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "Index_Id",
                table: "RefreshTokens",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "Index_CreatedAt6",
                table: "Regions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "Index_Id6",
                table: "Regions",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Regions_RegionCode",
                table: "Regions",
                column: "RegionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Index_CreatedAt7",
                table: "Reports",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "Index_Id7",
                table: "Reports",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_UserId",
                table: "Reports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Index_CreatedAt8",
                table: "Routes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "Index_Id8",
                table: "Routes",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_FromStationId",
                table: "Routes",
                column: "FromStationId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_LineId",
                table: "Routes",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_ToStationId",
                table: "Routes",
                column: "ToStationId");

            migrationBuilder.CreateIndex(
                name: "UQ__Routes__A25C5AA7CC95834E",
                table: "Routes",
                column: "RouteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Index_CreatedAt10",
                table: "ShipmentItineraries",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "Index_Id10",
                table: "ShipmentItineraries",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItineraries_RouteId",
                table: "ShipmentItineraries",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItineraries_ShipmentId",
                table: "ShipmentItineraries",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "Index_CreatedAt9",
                table: "Shipments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "Index_Id9",
                table: "Shipments",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_SenderId",
                table: "Shipments",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "UQ__Shipment__A2A2A54B59D458B2",
                table: "Shipments",
                column: "TrackingCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Index_CreatedAt11",
                table: "ShipmentTrackings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "Index_Id11",
                table: "ShipmentTrackings",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentTrackings_ShipmentId",
                table: "ShipmentTrackings",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentTrackings_StationId",
                table: "ShipmentTrackings",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "Index_CreatedAt12",
                table: "Stations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "Index_Id12",
                table: "Stations",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stations_RegionId",
                table: "Stations",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "UQ__Stations__A25C5AA7E98BD6EC",
                table: "Stations",
                column: "StationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Index_CreatedAt13",
                table: "SupportingTickets",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "Index_Id13",
                table: "SupportingTickets",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportingTickets_ShipmentId",
                table: "SupportingTickets",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportingTickets_UserId",
                table: "SupportingTickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "Index_CreatedAt14",
                table: "SystemConfigs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "Index_Id14",
                table: "SystemConfigs",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedTime",
                table: "Users",
                column: "CreatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetroSchedules");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Parcels");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "ShipmentItineraries");

            migrationBuilder.DropTable(
                name: "ShipmentTrackings");

            migrationBuilder.DropTable(
                name: "SupportingTickets");

            migrationBuilder.DropTable(
                name: "SystemConfigs");

            migrationBuilder.DropTable(
                name: "UserClaims");

            migrationBuilder.DropTable(
                name: "UserLogins");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "MetroTimeSlots");

            migrationBuilder.DropTable(
                name: "ParcelCategories");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "Shipments");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "MetroLines");

            migrationBuilder.DropTable(
                name: "Stations");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Regions");
        }
    }
}
