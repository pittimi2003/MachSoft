using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class InitMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DateFormats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DateFormats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DecimalSeparators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecimalSeparators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InputOrderProcessesClosing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessType = table.Column<string>(type: "text", nullable: false),
                    Worker = table.Column<string>(type: "text", nullable: false),
                    EquipmentGroup = table.Column<string>(type: "text", nullable: false),
                    InitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InputOrder = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputOrderProcessesClosing", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ObjectsCanvas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CanvasObjectType = table.Column<string>(type: "text", nullable: true),
                    ScaleX = table.Column<float>(type: "real", nullable: true),
                    ScaleY = table.Column<float>(type: "real", nullable: true),
                    Left = table.Column<float>(type: "real", nullable: true),
                    Top = table.Column<float>(type: "real", nullable: true),
                    Angle = table.Column<float>(type: "real", nullable: true),
                    Selectable = table.Column<bool>(type: "boolean", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: true),
                    fontFamily = table.Column<string>(type: "text", nullable: true),
                    FontSize = table.Column<float>(type: "real", nullable: true),
                    FontWeight = table.Column<string>(type: "text", nullable: true),
                    FontStyle = table.Column<string>(type: "text", nullable: true),
                    TextAlign = table.Column<string>(type: "text", nullable: true),
                    Underline = table.Column<bool>(type: "boolean", nullable: true),
                    Fill = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectsCanvas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemOfMeasurements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemOfMeasurements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThousandsSeparators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThousandsSeparators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimeZones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    OffSet = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeZones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "YardAppointmentsNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentCode = table.Column<string>(type: "text", nullable: false),
                    YardCode = table.Column<string>(type: "text", nullable: true),
                    VehicleCode = table.Column<string>(type: "text", nullable: false),
                    VehicleType = table.Column<string>(type: "text", nullable: false),
                    DockCode = table.Column<string>(type: "text", nullable: true),
                    AppointmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Customer = table.Column<string>(type: "text", nullable: false),
                    License = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YardAppointmentsNotifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    AddressLine = table.Column<string>(type: "text", nullable: true),
                    ZIPCode = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "text", nullable: true),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: true),
                    AddressComment = table.Column<string>(type: "text", nullable: true),
                    ContactName = table.Column<string>(type: "text", nullable: true),
                    Telephone = table.Column<string>(type: "text", nullable: true),
                    Extension = table.Column<string>(type: "text", nullable: true),
                    Fax = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    ContactComment = table.Column<string>(type: "text", nullable: true),
                    DecimalSeparatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThousandsSeparatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateFormatId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageId = table.Column<Guid>(type: "uuid", nullable: false),
                    SystemOfMeasurementId = table.Column<Guid>(type: "uuid", nullable: false),
                    Logo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organizations_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Organizations_DateFormats_DateFormatId",
                        column: x => x.DateFormatId,
                        principalTable: "DateFormats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Organizations_DecimalSeparators_DecimalSeparatorId",
                        column: x => x.DecimalSeparatorId,
                        principalTable: "DecimalSeparators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Organizations_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Organizations_SystemOfMeasurements_SystemOfMeasurementId",
                        column: x => x.SystemOfMeasurementId,
                        principalTable: "SystemOfMeasurements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Organizations_ThousandsSeparators_ThousandsSeparatorId",
                        column: x => x.ThousandsSeparatorId,
                        principalTable: "ThousandsSeparators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DockSelectionStrategies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DockSelectionStrategies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DockSelectionStrategies_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TimeZoneId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    AddressLine = table.Column<string>(type: "text", nullable: true),
                    ZIPCode = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "text", nullable: true),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: true),
                    AddressComment = table.Column<string>(type: "text", nullable: true),
                    ContactName = table.Column<string>(type: "text", nullable: true),
                    Telephone = table.Column<string>(type: "text", nullable: true),
                    Extension = table.Column<string>(type: "text", nullable: true),
                    Telephone2 = table.Column<string>(type: "text", nullable: true),
                    Fax = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    ContactComment = table.Column<string>(type: "text", nullable: true),
                    MeasureSystem = table.Column<string>(type: "text", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Warehouses_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Warehouses_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Warehouses_TimeZones_TimeZoneId",
                        column: x => x.TimeZoneId,
                        principalTable: "TimeZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityCode = table.Column<string>(type: "text", nullable: false),
                    EntityField = table.Column<string>(type: "text", nullable: false),
                    Operator = table.Column<int>(type: "integer", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: true),
                    FixedValue = table.Column<string>(type: "text", nullable: true),
                    IsFixed = table.Column<bool>(type: "boolean", nullable: false),
                    IsMail = table.Column<bool>(type: "boolean", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alerts_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BreakProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    AllowInInboundFlow = table.Column<bool>(type: "boolean", nullable: false),
                    AllowInOutboundFlow = table.Column<bool>(type: "boolean", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreakProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BreakProfiles_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationSequenceHeaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationSequenceHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigurationSequenceHeaders_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InboundFlowGraphs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    AverageItemsPerOrder = table.Column<int>(type: "integer", nullable: false),
                    AverageLinesPerOrder = table.Column<int>(type: "integer", nullable: false),
                    Group = table.Column<bool>(type: "boolean", nullable: true),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewPort = table.Column<string>(type: "text", nullable: true),
                    MaxVehicleTime = table.Column<double>(type: "double precision", nullable: true),
                    DockSelectionStrategyId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboundFlowGraphs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InboundFlowGraphs_DockSelectionStrategies_DockSelectionStra~",
                        column: x => x.DockSelectionStrategyId,
                        principalTable: "DockSelectionStrategies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InboundFlowGraphs_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Layouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Height = table.Column<double>(type: "double precision", nullable: true),
                    Width = table.Column<double>(type: "double precision", nullable: true),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Viewport = table.Column<string>(type: "text", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Layouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Layouts_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoadProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    AllowInInboundFlow = table.Column<bool>(type: "boolean", nullable: false),
                    AllowInOutboundFlow = table.Column<bool>(type: "boolean", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadProfiles_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderPriority",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderPriority", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderPriority_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OutboundFlowGraphs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    AverageItemsPerOrder = table.Column<int>(type: "integer", nullable: false),
                    AverageLinesPerOrder = table.Column<int>(type: "integer", nullable: false),
                    Group = table.Column<bool>(type: "boolean", nullable: true),
                    PartialClosed = table.Column<bool>(type: "boolean", nullable: true),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecurityLoadTime = table.Column<double>(type: "double precision", nullable: true),
                    ViewPort = table.Column<string>(type: "text", nullable: true),
                    MaxVehicleTime = table.Column<double>(type: "double precision", nullable: true),
                    DockSelectionStrategyId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboundFlowGraphs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutboundFlowGraphs_DockSelectionStrategies_DockSelectionStr~",
                        column: x => x.DockSelectionStrategyId,
                        principalTable: "DockSelectionStrategies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OutboundFlowGraphs_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Plannings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsStored = table.Column<bool>(type: "boolean", nullable: false),
                    IsWorkforcePlanning = table.Column<bool>(type: "boolean", nullable: false),
                    SLAWorkOrdersOnTimePercentage = table.Column<double>(type: "double precision", nullable: true),
                    SLAShippedStock = table.Column<double>(type: "double precision", nullable: true),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plannings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plannings_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostprocessProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    AllowInInboundFlow = table.Column<bool>(type: "boolean", nullable: false),
                    AllowInOutboundFlow = table.Column<bool>(type: "boolean", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostprocessProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostprocessProfiles_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreprocessProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    AllowInInboundFlow = table.Column<bool>(type: "boolean", nullable: false),
                    AllowInOutboundFlow = table.Column<bool>(type: "boolean", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreprocessProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreprocessProfiles_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessPriorityOrder",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessPriorityOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessPriorityOrder_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PutawayProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    AllowInInboundFlow = table.Column<bool>(type: "boolean", nullable: false),
                    AllowInOutboundFlow = table.Column<bool>(type: "boolean", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PutawayProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PutawayProfiles_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    InitHour = table.Column<double>(type: "double precision", nullable: false),
                    EndHour = table.Column<double>(type: "double precision", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shifts_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SLAConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SLAConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SLAConfigs_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Strategies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Strategies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Strategies_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TypeEquipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    LoadingWaitTime = table.Column<int>(type: "integer", nullable: true),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewPort = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeEquipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TypeEquipment_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Lastname = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DecimalSeparatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ThousandsSeparatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DateFormatId = table.Column<Guid>(type: "uuid", nullable: true),
                    LanguageId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastAccessDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseDefaultId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_DateFormats_DateFormatId",
                        column: x => x.DateFormatId,
                        principalTable: "DateFormats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Users_DecimalSeparators_DecimalSeparatorId",
                        column: x => x.DecimalSeparatorId,
                        principalTable: "DecimalSeparators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Users_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Users_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_ThousandsSeparators_ThousandsSeparatorId",
                        column: x => x.ThousandsSeparatorId,
                        principalTable: "ThousandsSeparators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Users_Warehouses_WarehouseDefaultId",
                        column: x => x.WarehouseDefaultId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "VehicleProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    AllowInInboundFlow = table.Column<bool>(type: "boolean", nullable: false),
                    AllowInOutboundFlow = table.Column<bool>(type: "boolean", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleProfiles_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Widgets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Entity = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    XML = table.Column<string>(type: "text", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Widgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Widgets_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlertConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertId = table.Column<Guid>(type: "uuid", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertConfigurations_Alerts_AlertId",
                        column: x => x.AlertId,
                        principalTable: "Alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlertFilters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertId = table.Column<Guid>(type: "uuid", nullable: false),
                    Operator = table.Column<int>(type: "integer", nullable: false),
                    FilterField = table.Column<string>(type: "text", nullable: false),
                    FilterReference = table.Column<string>(type: "text", nullable: true),
                    FilterFixedValue = table.Column<string>(type: "text", nullable: true),
                    IsFixed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertFilters_Alerts_AlertId",
                        column: x => x.AlertId,
                        principalTable: "Alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlertMails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertId = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertMails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertMails_Alerts_AlertId",
                        column: x => x.AlertId,
                        principalTable: "Alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Breaks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    InitBreak = table.Column<double>(type: "double precision", nullable: false),
                    EndBreak = table.Column<double>(type: "double precision", nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: true),
                    IsRequiered = table.Column<bool>(type: "boolean", nullable: true),
                    BreakProfileId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Breaks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Breaks_BreakProfiles_BreakProfileId",
                        column: x => x.BreakProfileId,
                        principalTable: "BreakProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationSequences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    ConfigurationSequenceHeaderId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationSequences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigurationSequences_ConfigurationSequenceHeaders_Configu~",
                        column: x => x.ConfigurationSequenceHeaderId,
                        principalTable: "ConfigurationSequenceHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    xInit = table.Column<double>(type: "double precision", nullable: true),
                    yInit = table.Column<double>(type: "double precision", nullable: true),
                    xEnd = table.Column<double>(type: "double precision", nullable: true),
                    yEnd = table.Column<double>(type: "double precision", nullable: true),
                    AlternativeAreaId = table.Column<Guid>(type: "uuid", nullable: true),
                    DelayedTimePerUnit = table.Column<int>(type: "integer", nullable: true),
                    NarrowAisle = table.Column<bool>(type: "boolean", nullable: false),
                    IsAutomatic = table.Column<bool>(type: "boolean", nullable: false),
                    LayoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewPort = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Areas_Areas_AlternativeAreaId",
                        column: x => x.AlternativeAreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Areas_Layouts_LayoutId",
                        column: x => x.LayoutId,
                        principalTable: "Layouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Objects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    xInit = table.Column<double>(type: "double precision", nullable: true),
                    yInit = table.Column<double>(type: "double precision", nullable: true),
                    xEnd = table.Column<double>(type: "double precision", nullable: true),
                    yEnd = table.Column<double>(type: "double precision", nullable: true),
                    LayoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    Left = table.Column<double>(type: "double precision", nullable: true),
                    Top = table.Column<double>(type: "double precision", nullable: true),
                    Width = table.Column<double>(type: "double precision", nullable: true),
                    Height = table.Column<double>(type: "double precision", nullable: true),
                    Angle = table.Column<double>(type: "double precision", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: true),
                    ViewPort = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Objects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Objects_Layouts_LayoutId",
                        column: x => x.LayoutId,
                        principalTable: "Layouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlertResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanningId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggerDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertResponses_Alerts_AlertId",
                        column: x => x.AlertId,
                        principalTable: "Alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlertResponses_Plannings_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Plannings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StrategySequences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    StrategyCode = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Comparation = table.Column<string>(type: "text", nullable: false),
                    StrategyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategySequences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StrategySequences_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Workers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    RolId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerNumber = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workers_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Workers_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserWarehouse",
                columns: table => new
                {
                    UsersId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehousesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWarehouse", x => new { x.UsersId, x.WarehousesId });
                    table.ForeignKey(
                        name: "FK_UserWarehouse_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserWarehouse_Warehouses_WarehousesId",
                        column: x => x.WarehousesId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderLoadRatios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoadId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoadInVehicle = table.Column<int>(type: "integer", nullable: false),
                    OrderInLoad = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLoadRatios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderLoadRatios_LoadProfiles_LoadId",
                        column: x => x.LoadId,
                        principalTable: "LoadProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderLoadRatios_VehicleProfiles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "VehicleProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InitHour = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndHour = table.Column<TimeSpan>(type: "interval", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumberVehicles = table.Column<double>(type: "double precision", nullable: false),
                    LoadId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsOut = table.Column<bool>(type: "boolean", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderSchedules_LoadProfiles_LoadId",
                        column: x => x.LoadId,
                        principalTable: "LoadProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderSchedules_VehicleProfiles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "VehicleProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderSchedules_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Equipments = table.Column<int>(type: "integer", nullable: false),
                    TypeEquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AreaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewPort = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentGroups_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentGroups_TypeEquipment_TypeEquipmentId",
                        column: x => x.TypeEquipmentId,
                        principalTable: "TypeEquipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Processes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    MinTime = table.Column<int>(type: "integer", nullable: false),
                    PreprocessTime = table.Column<int>(type: "integer", nullable: true),
                    PostprocessTime = table.Column<int>(type: "integer", nullable: true),
                    IsWarehouseProcess = table.Column<bool>(type: "boolean", nullable: false),
                    IsOut = table.Column<bool>(type: "boolean", nullable: false),
                    IsIn = table.Column<bool>(type: "boolean", nullable: false),
                    IsInitProcess = table.Column<bool>(type: "boolean", nullable: false),
                    PercentageInitProcess = table.Column<double>(type: "double precision", nullable: true),
                    AreaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewPort = table.Column<string>(type: "text", nullable: true),
                    IsEffective = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Processes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Processes_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DepartureAreaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArrivalAreaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Bidirectional = table.Column<bool>(type: "boolean", nullable: false),
                    TimeMin = table.Column<int>(type: "integer", nullable: true),
                    ViewPort = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Routes_Areas_ArrivalAreaId",
                        column: x => x.ArrivalAreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Routes_Areas_DepartureAreaId",
                        column: x => x.DepartureAreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Zones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    xInit = table.Column<double>(type: "double precision", nullable: true),
                    yInit = table.Column<double>(type: "double precision", nullable: true),
                    xEnd = table.Column<double>(type: "double precision", nullable: true),
                    yEnd = table.Column<double>(type: "double precision", nullable: true),
                    MaxStockToBook = table.Column<int>(type: "integer", nullable: false),
                    IsLimitStock = table.Column<bool>(type: "boolean", nullable: false),
                    MaxStock = table.Column<int>(type: "integer", nullable: true),
                    MaxContainers = table.Column<int>(type: "integer", nullable: true),
                    InitStockToBook = table.Column<int>(type: "integer", nullable: false),
                    AreaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewPort = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Zones_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AvailableWorkers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailableWorkers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvailableWorkers_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WFMLaborEquipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeEquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Equipments = table.Column<int>(type: "integer", nullable: false),
                    InitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlanningId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkTime = table.Column<double>(type: "double precision", nullable: false),
                    Efficiency = table.Column<double>(type: "double precision", nullable: false),
                    Productivity = table.Column<double>(type: "double precision", nullable: false),
                    Utility = table.Column<double>(type: "double precision", nullable: false),
                    TotalProductivity = table.Column<double>(type: "double precision", nullable: false),
                    TotalUtility = table.Column<double>(type: "double precision", nullable: false),
                    TotalOrders = table.Column<int>(type: "integer", nullable: false),
                    ClosedOrders = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFMLaborEquipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WFMLaborEquipment_EquipmentGroups_EquipmentGroupId",
                        column: x => x.EquipmentGroupId,
                        principalTable: "EquipmentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborEquipment_Plannings_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Plannings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborEquipment_TypeEquipment_TypeEquipmentId",
                        column: x => x.TypeEquipmentId,
                        principalTable: "TypeEquipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomProcesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InitHour = table.Column<double>(type: "double precision", nullable: true),
                    EndHour = table.Column<double>(type: "double precision", nullable: true),
                    Percentage = table.Column<double>(type: "double precision", nullable: true),
                    NumPossibleTimes = table.Column<double>(type: "double precision", nullable: true),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewPort = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomProcesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomProcesses_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inbounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "text", nullable: true),
                    VehiclePerHour = table.Column<int>(type: "integer", nullable: false),
                    TruckPerDay = table.Column<int>(type: "integer", nullable: false),
                    MinTimeInBuffer = table.Column<int>(type: "integer", nullable: true),
                    LoadTime = table.Column<int>(type: "integer", nullable: true),
                    AdditionalTimeInBuffer = table.Column<int>(type: "integer", nullable: true),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    Viewport = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inbounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inbounds_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Loadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Dock = table.Column<string>(type: "text", nullable: true),
                    AutomaticLoadingTime = table.Column<int>(type: "integer", nullable: true),
                    AdditionalTimeInBuffer = table.Column<int>(type: "integer", nullable: true),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    Viewport = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Loadings_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pickings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PickingRoadTime = table.Column<int>(type: "integer", nullable: true),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewPort = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pickings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pickings_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessDirectionProperties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Percentage = table.Column<double>(type: "double precision", nullable: false),
                    InitProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    EndProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnd = table.Column<bool>(type: "boolean", nullable: false),
                    ViewPort = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessDirectionProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessDirectionProperties_Processes_EndProcessId",
                        column: x => x.EndProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcessDirectionProperties_Processes_InitProcessId",
                        column: x => x.InitProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessHours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    Hour = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessHours_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Putaways",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdditionTmeToPutaway = table.Column<int>(type: "integer", nullable: true),
                    MinHour = table.Column<int>(type: "integer", nullable: true),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewPort = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Putaways", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Putaways_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Receptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BreakageFactor = table.Column<int>(type: "integer", nullable: true),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewPort = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Receptions_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Replenishments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Percentage = table.Column<int>(type: "integer", nullable: true),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewPort = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Replenishments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Replenishments_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolProcessSequences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RolId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolProcessSequences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolProcessSequences_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolProcessSequences_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shippings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewPort = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shippings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shippings_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TimeMin = table.Column<int>(type: "integer", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    InitProcess = table.Column<bool>(type: "boolean", nullable: false),
                    EndProcess = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewPort = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Steps_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseProcessPlanning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    LimitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WorkTime = table.Column<double>(type: "double precision", nullable: false),
                    IsStored = table.Column<bool>(type: "boolean", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    IsStarted = table.Column<bool>(type: "boolean", nullable: false),
                    PlanningId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uuid", nullable: true),
                    EquipmentGroupId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseProcessPlanning", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseProcessPlanning_EquipmentGroups_EquipmentGroupId",
                        column: x => x.EquipmentGroupId,
                        principalTable: "EquipmentGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WarehouseProcessPlanning_Plannings_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Plannings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WarehouseProcessPlanning_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WarehouseProcessPlanning_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Aisles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaxMHE = table.Column<int>(type: "integer", nullable: true),
                    AdditionalTimePerUnitEntry = table.Column<int>(type: "integer", nullable: true),
                    AdditionalTimePerUnitExit = table.Column<int>(type: "integer", nullable: true),
                    MaxTasks = table.Column<int>(type: "integer", nullable: true),
                    AisleChangeTime = table.Column<int>(type: "integer", nullable: true),
                    NarrowAisle = table.Column<bool>(type: "boolean", nullable: false),
                    MaxPickers = table.Column<int>(type: "integer", nullable: true),
                    EndPicking = table.Column<bool>(type: "boolean", nullable: false),
                    Bidirectional = table.Column<bool>(type: "boolean", nullable: false),
                    WidthPerDirection = table.Column<int>(type: "integer", nullable: true),
                    MaxMHEPickOrPutaway = table.Column<int>(type: "integer", nullable: true),
                    ReplenishmentControl = table.Column<bool>(type: "boolean", nullable: false),
                    MaxMovement = table.Column<int>(type: "integer", nullable: true),
                    ZoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewPort = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aisles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aisles_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AutomaticStorages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    ZoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewPort = table.Column<string>(type: "text", nullable: true),
                    NumCrossAisles = table.Column<int>(type: "integer", nullable: true),
                    NumShelves = table.Column<int>(type: "integer", nullable: true),
                    IsVertical = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomaticStorages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomaticStorages_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Buffers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryCapacity = table.Column<int>(type: "integer", nullable: false),
                    ExitCapacity = table.Column<int>(type: "integer", nullable: false),
                    CapacityByMaterial = table.Column<bool>(type: "boolean", nullable: false),
                    Excess = table.Column<bool>(type: "boolean", nullable: false),
                    MaxEquipments = table.Column<int>(type: "integer", nullable: false),
                    ZoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewPort = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Buffers_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChaoticStorages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    ZoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewPort = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChaoticStorages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChaoticStorages_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Docks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaxEquipments = table.Column<int>(type: "integer", nullable: false),
                    OperatesFromBuffer = table.Column<bool>(type: "boolean", nullable: false),
                    OverloadHandling = table.Column<double>(type: "double precision", nullable: true),
                    InboundRange = table.Column<int>(type: "integer", nullable: true),
                    OutboundRange = table.Column<int>(type: "integer", nullable: true),
                    MaxStockCrossdocking = table.Column<int>(type: "integer", nullable: false),
                    AllowInbound = table.Column<bool>(type: "boolean", nullable: false),
                    AllowOutbound = table.Column<bool>(type: "boolean", nullable: false),
                    ZoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewPort = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Docks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Docks_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DriveIns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    ZoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewPort = table.Column<string>(type: "text", nullable: true),
                    NumCrossAisles = table.Column<int>(type: "integer", nullable: true),
                    NumShelves = table.Column<int>(type: "integer", nullable: true),
                    IsVertical = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriveIns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriveIns_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Racks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ZoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityHeigth = table.Column<int>(type: "integer", nullable: true),
                    QuantityWidth = table.Column<int>(type: "integer", nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    ViewPort = table.Column<string>(type: "text", nullable: true),
                    NumCrossAisles = table.Column<int>(type: "integer", nullable: true),
                    NumShelves = table.Column<int>(type: "integer", nullable: true),
                    IsVertical = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Racks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Racks_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AvailableWorkerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: false),
                    BreakProfileId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schedules_AvailableWorkers_AvailableWorkerId",
                        column: x => x.AvailableWorkerId,
                        principalTable: "AvailableWorkers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Schedules_BreakProfiles_BreakProfileId",
                        column: x => x.BreakProfileId,
                        principalTable: "BreakProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Schedules_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WFMLaborEquipmentPerFlow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeEquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsOutbound = table.Column<bool>(type: "boolean", nullable: false),
                    Equipments = table.Column<int>(type: "integer", nullable: false),
                    InitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlanningId = table.Column<Guid>(type: "uuid", nullable: false),
                    WFMLaborEquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkTime = table.Column<double>(type: "double precision", nullable: false),
                    Efficiency = table.Column<double>(type: "double precision", nullable: false),
                    Productivity = table.Column<double>(type: "double precision", nullable: false),
                    Utility = table.Column<double>(type: "double precision", nullable: false),
                    TotalProductivity = table.Column<double>(type: "double precision", nullable: false),
                    TotalUtility = table.Column<double>(type: "double precision", nullable: false),
                    TotalOrders = table.Column<int>(type: "integer", nullable: false),
                    ClosedOrders = table.Column<int>(type: "integer", nullable: false),
                    TotalProcesses = table.Column<int>(type: "integer", nullable: false),
                    ClosedProcesses = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFMLaborEquipmentPerFlow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WFMLaborEquipmentPerFlow_EquipmentGroups_EquipmentGroupId",
                        column: x => x.EquipmentGroupId,
                        principalTable: "EquipmentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborEquipmentPerFlow_Plannings_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Plannings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborEquipmentPerFlow_TypeEquipment_TypeEquipmentId",
                        column: x => x.TypeEquipmentId,
                        principalTable: "TypeEquipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborEquipmentPerFlow_WFMLaborEquipment_WFMLaborEquipmen~",
                        column: x => x.WFMLaborEquipmentId,
                        principalTable: "WFMLaborEquipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InputOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderCode = table.Column<string>(type: "text", nullable: false),
                    IsStarted = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<string>(type: "text", nullable: true),
                    IsOutbound = table.Column<bool>(type: "boolean", nullable: false),
                    AllowPartialClosed = table.Column<bool>(type: "boolean", nullable: false),
                    AllowGroup = table.Column<bool>(type: "boolean", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RealArrivalTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Carrier = table.Column<string>(type: "text", nullable: true),
                    Account = table.Column<string>(type: "text", nullable: true),
                    Supplier = table.Column<string>(type: "text", nullable: true),
                    Trailer = table.Column<string>(type: "text", nullable: true),
                    IsEstimated = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedDockId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreferredDockId = table.Column<Guid>(type: "uuid", nullable: true),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: true),
                    BlockDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Progress = table.Column<double>(type: "double precision", nullable: true),
                    EndBlockDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VehicleCode = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InputOrders_Docks_AssignedDockId",
                        column: x => x.AssignedDockId,
                        principalTable: "Docks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InputOrders_Docks_PreferredDockId",
                        column: x => x.PreferredDockId,
                        principalTable: "Docks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InputOrders_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YardMetricsPerDock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessType = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DockId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendedAppointments = table.Column<int>(type: "integer", nullable: false),
                    TotalAppointments = table.Column<int>(type: "integer", nullable: false),
                    Saturation = table.Column<double>(type: "double precision", nullable: false),
                    PlanningId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YardMetricsPerDock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YardMetricsPerDock_Docks_DockId",
                        column: x => x.DockId,
                        principalTable: "Docks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YardMetricsPerDock_Plannings_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Plannings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WFMLaborWorker",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanningId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkTime = table.Column<double>(type: "double precision", nullable: false),
                    Efficiency = table.Column<double>(type: "double precision", nullable: false),
                    Productivity = table.Column<double>(type: "double precision", nullable: false),
                    Utility = table.Column<double>(type: "double precision", nullable: false),
                    TotalProductivity = table.Column<double>(type: "double precision", nullable: false),
                    TotalUtility = table.Column<double>(type: "double precision", nullable: false),
                    Breaks = table.Column<int>(type: "integer", nullable: false),
                    Ranking = table.Column<double>(type: "double precision", nullable: false),
                    TotalOrders = table.Column<int>(type: "integer", nullable: false),
                    ClosedOrders = table.Column<int>(type: "integer", nullable: false),
                    TotalProcesses = table.Column<int>(type: "integer", nullable: false),
                    ClosedProcesses = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFMLaborWorker", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WFMLaborWorker_Plannings_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Plannings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborWorker_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborWorker_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WFMLaborEquipmentPerProcessType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeEquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Equipments = table.Column<int>(type: "integer", nullable: false),
                    InitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessType = table.Column<string>(type: "text", nullable: false),
                    PlanningId = table.Column<Guid>(type: "uuid", nullable: false),
                    WFMLaborEquipmentPerFlowId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkTime = table.Column<double>(type: "double precision", nullable: false),
                    Efficiency = table.Column<double>(type: "double precision", nullable: false),
                    Productivity = table.Column<double>(type: "double precision", nullable: false),
                    Utility = table.Column<double>(type: "double precision", nullable: false),
                    TotalProductivity = table.Column<double>(type: "double precision", nullable: false),
                    TotalUtility = table.Column<double>(type: "double precision", nullable: false),
                    TotalOrders = table.Column<int>(type: "integer", nullable: false),
                    ClosedOrders = table.Column<int>(type: "integer", nullable: false),
                    TotalProcesses = table.Column<int>(type: "integer", nullable: false),
                    ClosedProcesses = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFMLaborEquipmentPerProcessType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WFMLaborEquipmentPerProcessType_EquipmentGroups_EquipmentGr~",
                        column: x => x.EquipmentGroupId,
                        principalTable: "EquipmentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborEquipmentPerProcessType_Plannings_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Plannings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborEquipmentPerProcessType_TypeEquipment_TypeEquipment~",
                        column: x => x.TypeEquipmentId,
                        principalTable: "TypeEquipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborEquipmentPerProcessType_WFMLaborEquipmentPerFlow_WF~",
                        column: x => x.WFMLaborEquipmentPerFlowId,
                        principalTable: "WFMLaborEquipmentPerFlow",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InputOrderLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Product = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<double>(type: "double precision", nullable: true),
                    UnitOfMeasure = table.Column<string>(type: "text", nullable: true),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: true),
                    InputOutboundOrderId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputOrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InputOrderLines_InputOrders_InputOutboundOrderId",
                        column: x => x.InputOutboundOrderId,
                        principalTable: "InputOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrdersPlanning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsOutbound = table.Column<bool>(type: "boolean", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WorkTime = table.Column<double>(type: "double precision", nullable: false),
                    IsEstimated = table.Column<bool>(type: "boolean", nullable: false),
                    IsStored = table.Column<bool>(type: "boolean", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    IsStarted = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<string>(type: "text", nullable: true),
                    Progress = table.Column<double>(type: "double precision", nullable: false),
                    PlanningId = table.Column<Guid>(type: "uuid", nullable: false),
                    InputOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedDockId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsOnTime = table.Column<bool>(type: "boolean", nullable: false),
                    IsInVehicleTime = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrdersPlanning", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrdersPlanning_Docks_AssignedDockId",
                        column: x => x.AssignedDockId,
                        principalTable: "Docks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkOrdersPlanning_InputOrders_InputOrderId",
                        column: x => x.InputOrderId,
                        principalTable: "InputOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkOrdersPlanning_Plannings_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Plannings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YardMetricsAppointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentCode = table.Column<string>(type: "text", nullable: false),
                    ProcessType = table.Column<string>(type: "text", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Customer = table.Column<string>(type: "text", nullable: false),
                    YardCode = table.Column<string>(type: "text", nullable: false),
                    DockId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleType = table.Column<string>(type: "text", nullable: false),
                    License = table.Column<string>(type: "text", nullable: false),
                    Progress = table.Column<double>(type: "double precision", nullable: false),
                    TotalOrders = table.Column<int>(type: "integer", nullable: false),
                    CompletedOrders = table.Column<int>(type: "integer", nullable: false),
                    PlanningId = table.Column<Guid>(type: "uuid", nullable: false),
                    YardMetricsPerDockId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YardMetricsAppointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YardMetricsAppointments_Docks_DockId",
                        column: x => x.DockId,
                        principalTable: "Docks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YardMetricsAppointments_Plannings_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Plannings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YardMetricsAppointments_YardMetricsPerDock_YardMetricsPerDo~",
                        column: x => x.YardMetricsPerDockId,
                        principalTable: "YardMetricsPerDock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WFMLaborWorkerPerFlow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsOutbound = table.Column<bool>(type: "boolean", nullable: false),
                    PlanningId = table.Column<Guid>(type: "uuid", nullable: false),
                    WFMLaborId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkTime = table.Column<double>(type: "double precision", nullable: false),
                    Efficiency = table.Column<double>(type: "double precision", nullable: false),
                    Productivity = table.Column<double>(type: "double precision", nullable: false),
                    Utility = table.Column<double>(type: "double precision", nullable: false),
                    TotalProductivity = table.Column<double>(type: "double precision", nullable: false),
                    TotalUtility = table.Column<double>(type: "double precision", nullable: false),
                    Breaks = table.Column<int>(type: "integer", nullable: false),
                    Ranking = table.Column<double>(type: "double precision", nullable: false),
                    TotalOrders = table.Column<int>(type: "integer", nullable: false),
                    ClosedOrders = table.Column<int>(type: "integer", nullable: false),
                    TotalProcesses = table.Column<int>(type: "integer", nullable: false),
                    ClosedProcesses = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFMLaborWorkerPerFlow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WFMLaborWorkerPerFlow_Plannings_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Plannings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborWorkerPerFlow_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborWorkerPerFlow_WFMLaborWorker_WFMLaborId",
                        column: x => x.WFMLaborId,
                        principalTable: "WFMLaborWorker",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborWorkerPerFlow_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemsPlanning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsOutbound = table.Column<bool>(type: "boolean", nullable: false),
                    LimitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WorkTime = table.Column<double>(type: "double precision", nullable: false),
                    IsStored = table.Column<bool>(type: "boolean", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    IsStarted = table.Column<bool>(type: "boolean", nullable: false),
                    WorkOrderPlanningId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uuid", nullable: true),
                    EquipmentGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsFaked = table.Column<bool>(type: "boolean", nullable: true),
                    Progress = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsPlanning", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemsPlanning_EquipmentGroups_EquipmentGroupId",
                        column: x => x.EquipmentGroupId,
                        principalTable: "EquipmentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ItemsPlanning_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemsPlanning_WorkOrdersPlanning_WorkOrderPlanningId",
                        column: x => x.WorkOrderPlanningId,
                        principalTable: "WorkOrdersPlanning",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemsPlanning_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WFMLaborWorkerPerProcessType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessType = table.Column<string>(type: "text", nullable: false),
                    PlanningId = table.Column<Guid>(type: "uuid", nullable: false),
                    WFMLaborPerFlowId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkTime = table.Column<double>(type: "double precision", nullable: false),
                    Efficiency = table.Column<double>(type: "double precision", nullable: false),
                    Productivity = table.Column<double>(type: "double precision", nullable: false),
                    Utility = table.Column<double>(type: "double precision", nullable: false),
                    TotalProductivity = table.Column<double>(type: "double precision", nullable: false),
                    TotalUtility = table.Column<double>(type: "double precision", nullable: false),
                    Breaks = table.Column<int>(type: "integer", nullable: false),
                    Ranking = table.Column<double>(type: "double precision", nullable: false),
                    TotalOrders = table.Column<int>(type: "integer", nullable: false),
                    ClosedOrders = table.Column<int>(type: "integer", nullable: false),
                    TotalProcesses = table.Column<int>(type: "integer", nullable: false),
                    ClosedProcesses = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFMLaborWorkerPerProcessType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WFMLaborWorkerPerProcessType_Plannings_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Plannings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborWorkerPerProcessType_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborWorkerPerProcessType_WFMLaborWorkerPerFlow_WFMLabor~",
                        column: x => x.WFMLaborPerFlowId,
                        principalTable: "WFMLaborWorkerPerFlow",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborWorkerPerProcessType_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WFMLaborItemPlanning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    InitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EquipmentGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    InputOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    WFMLaborPerProcessTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    WFMLaborEquipmentPerProcessTypeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFMLaborItemPlanning", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WFMLaborItemPlanning_EquipmentGroups_EquipmentGroupId",
                        column: x => x.EquipmentGroupId,
                        principalTable: "EquipmentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborItemPlanning_InputOrders_InputOrderId",
                        column: x => x.InputOrderId,
                        principalTable: "InputOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborItemPlanning_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborItemPlanning_WFMLaborEquipmentPerProcessType_WFMLab~",
                        column: x => x.WFMLaborEquipmentPerProcessTypeId,
                        principalTable: "WFMLaborEquipmentPerProcessType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborItemPlanning_WFMLaborWorkerPerProcessType_WFMLaborP~",
                        column: x => x.WFMLaborPerProcessTypeId,
                        principalTable: "WFMLaborWorkerPerProcessType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFMLaborItemPlanning_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aisles_ZoneId",
                table: "Aisles",
                column: "ZoneId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlertConfigurations_AlertId",
                table: "AlertConfigurations",
                column: "AlertId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertFilters_AlertId",
                table: "AlertFilters",
                column: "AlertId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertMails_AlertId",
                table: "AlertMails",
                column: "AlertId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertResponses_AlertId",
                table: "AlertResponses",
                column: "AlertId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertResponses_PlanningId",
                table: "AlertResponses",
                column: "PlanningId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_WarehouseId",
                table: "Alerts",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Areas_AlternativeAreaId",
                table: "Areas",
                column: "AlternativeAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Areas_LayoutId",
                table: "Areas",
                column: "LayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticStorages_ZoneId",
                table: "AutomaticStorages",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailableWorkers_WorkerId",
                table: "AvailableWorkers",
                column: "WorkerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BreakProfiles_WarehouseId",
                table: "BreakProfiles",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Breaks_BreakProfileId",
                table: "Breaks",
                column: "BreakProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Buffers_ZoneId",
                table: "Buffers",
                column: "ZoneId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChaoticStorages_ZoneId",
                table: "ChaoticStorages",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationSequenceHeaders_WarehouseId",
                table: "ConfigurationSequenceHeaders",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationSequences_ConfigurationSequenceHeaderId",
                table: "ConfigurationSequences",
                column: "ConfigurationSequenceHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomProcesses_ProcessId",
                table: "CustomProcesses",
                column: "ProcessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Docks_ZoneId",
                table: "Docks",
                column: "ZoneId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DockSelectionStrategies_OrganizationId",
                table: "DockSelectionStrategies",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_DriveIns_ZoneId",
                table: "DriveIns",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentGroups_AreaId",
                table: "EquipmentGroups",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentGroups_TypeEquipmentId",
                table: "EquipmentGroups",
                column: "TypeEquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_InboundFlowGraphs_DockSelectionStrategyId",
                table: "InboundFlowGraphs",
                column: "DockSelectionStrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_InboundFlowGraphs_WarehouseId",
                table: "InboundFlowGraphs",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Inbounds_ProcessId",
                table: "Inbounds",
                column: "ProcessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InputOrderLines_InputOutboundOrderId",
                table: "InputOrderLines",
                column: "InputOutboundOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_InputOrders_AssignedDockId",
                table: "InputOrders",
                column: "AssignedDockId");

            migrationBuilder.CreateIndex(
                name: "IX_InputOrders_PreferredDockId",
                table: "InputOrders",
                column: "PreferredDockId");

            migrationBuilder.CreateIndex(
                name: "IX_InputOrders_WarehouseId",
                table: "InputOrders",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsPlanning_EquipmentGroupId",
                table: "ItemsPlanning",
                column: "EquipmentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsPlanning_ProcessId",
                table: "ItemsPlanning",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsPlanning_WorkerId",
                table: "ItemsPlanning",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsPlanning_WorkOrderPlanningId",
                table: "ItemsPlanning",
                column: "WorkOrderPlanningId");

            migrationBuilder.CreateIndex(
                name: "IX_Layouts_WarehouseId",
                table: "Layouts",
                column: "WarehouseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Loadings_ProcessId",
                table: "Loadings",
                column: "ProcessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoadProfiles_WarehouseId",
                table: "LoadProfiles",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Objects_LayoutId",
                table: "Objects",
                column: "LayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLoadRatios_LoadId",
                table: "OrderLoadRatios",
                column: "LoadId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLoadRatios_VehicleId",
                table: "OrderLoadRatios",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPriority_WarehouseId",
                table: "OrderPriority",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderSchedules_LoadId",
                table: "OrderSchedules",
                column: "LoadId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderSchedules_VehicleId",
                table: "OrderSchedules",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderSchedules_WarehouseId",
                table: "OrderSchedules",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_CountryId",
                table: "Organizations",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_DateFormatId",
                table: "Organizations",
                column: "DateFormatId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_DecimalSeparatorId",
                table: "Organizations",
                column: "DecimalSeparatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_LanguageId",
                table: "Organizations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_SystemOfMeasurementId",
                table: "Organizations",
                column: "SystemOfMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_ThousandsSeparatorId",
                table: "Organizations",
                column: "ThousandsSeparatorId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboundFlowGraphs_DockSelectionStrategyId",
                table: "OutboundFlowGraphs",
                column: "DockSelectionStrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboundFlowGraphs_WarehouseId",
                table: "OutboundFlowGraphs",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Pickings_ProcessId",
                table: "Pickings",
                column: "ProcessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plannings_WarehouseId",
                table: "Plannings",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_PostprocessProfiles_WarehouseId",
                table: "PostprocessProfiles",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_PreprocessProfiles_WarehouseId",
                table: "PreprocessProfiles",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessDirectionProperties_EndProcessId",
                table: "ProcessDirectionProperties",
                column: "EndProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessDirectionProperties_InitProcessId",
                table: "ProcessDirectionProperties",
                column: "InitProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_AreaId",
                table: "Processes",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessHours_ProcessId",
                table: "ProcessHours",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessPriorityOrder_WarehouseId",
                table: "ProcessPriorityOrder",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_PutawayProfiles_WarehouseId",
                table: "PutawayProfiles",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Putaways_ProcessId",
                table: "Putaways",
                column: "ProcessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Racks_ZoneId",
                table: "Racks",
                column: "ZoneId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Receptions_ProcessId",
                table: "Receptions",
                column: "ProcessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Replenishments_ProcessId",
                table: "Replenishments",
                column: "ProcessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_WarehouseId",
                table: "Roles",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_RolProcessSequences_ProcessId",
                table: "RolProcessSequences",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_RolProcessSequences_RolId",
                table: "RolProcessSequences",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_ArrivalAreaId",
                table: "Routes",
                column: "ArrivalAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_DepartureAreaId",
                table: "Routes",
                column: "DepartureAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_AvailableWorkerId",
                table: "Schedules",
                column: "AvailableWorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_BreakProfileId",
                table: "Schedules",
                column: "BreakProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_ShiftId",
                table: "Schedules",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_WarehouseId",
                table: "Shifts",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Shippings_ProcessId",
                table: "Shippings",
                column: "ProcessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SLAConfigs_WarehouseId",
                table: "SLAConfigs",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Steps_ProcessId",
                table: "Steps",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_Strategies_WarehouseId",
                table: "Strategies",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StrategySequences_StrategyId",
                table: "StrategySequences",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_WarehouseId",
                table: "Teams",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_TypeEquipment_WarehouseId",
                table: "TypeEquipment",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DateFormatId",
                table: "Users",
                column: "DateFormatId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DecimalSeparatorId",
                table: "Users",
                column: "DecimalSeparatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LanguageId",
                table: "Users",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId",
                table: "Users",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ThousandsSeparatorId",
                table: "Users",
                column: "ThousandsSeparatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_WarehouseDefaultId",
                table: "Users",
                column: "WarehouseDefaultId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWarehouse_WarehousesId",
                table: "UserWarehouse",
                column: "WarehousesId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleProfiles_WarehouseId",
                table: "VehicleProfiles",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseProcessPlanning_EquipmentGroupId",
                table: "WarehouseProcessPlanning",
                column: "EquipmentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseProcessPlanning_PlanningId",
                table: "WarehouseProcessPlanning",
                column: "PlanningId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseProcessPlanning_ProcessId",
                table: "WarehouseProcessPlanning",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseProcessPlanning_WorkerId",
                table: "WarehouseProcessPlanning",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_CountryId",
                table: "Warehouses",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_OrganizationId",
                table: "Warehouses",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_TimeZoneId",
                table: "Warehouses",
                column: "TimeZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborEquipment_EquipmentGroupId",
                table: "WFMLaborEquipment",
                column: "EquipmentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborEquipment_PlanningId",
                table: "WFMLaborEquipment",
                column: "PlanningId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborEquipment_TypeEquipmentId",
                table: "WFMLaborEquipment",
                column: "TypeEquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborEquipmentPerFlow_EquipmentGroupId",
                table: "WFMLaborEquipmentPerFlow",
                column: "EquipmentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborEquipmentPerFlow_PlanningId",
                table: "WFMLaborEquipmentPerFlow",
                column: "PlanningId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborEquipmentPerFlow_TypeEquipmentId",
                table: "WFMLaborEquipmentPerFlow",
                column: "TypeEquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborEquipmentPerFlow_WFMLaborEquipmentId",
                table: "WFMLaborEquipmentPerFlow",
                column: "WFMLaborEquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborEquipmentPerProcessType_EquipmentGroupId",
                table: "WFMLaborEquipmentPerProcessType",
                column: "EquipmentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborEquipmentPerProcessType_PlanningId",
                table: "WFMLaborEquipmentPerProcessType",
                column: "PlanningId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborEquipmentPerProcessType_TypeEquipmentId",
                table: "WFMLaborEquipmentPerProcessType",
                column: "TypeEquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborEquipmentPerProcessType_WFMLaborEquipmentPerFlowId",
                table: "WFMLaborEquipmentPerProcessType",
                column: "WFMLaborEquipmentPerFlowId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborItemPlanning_EquipmentGroupId",
                table: "WFMLaborItemPlanning",
                column: "EquipmentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborItemPlanning_InputOrderId",
                table: "WFMLaborItemPlanning",
                column: "InputOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborItemPlanning_ScheduleId",
                table: "WFMLaborItemPlanning",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborItemPlanning_WFMLaborEquipmentPerProcessTypeId",
                table: "WFMLaborItemPlanning",
                column: "WFMLaborEquipmentPerProcessTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborItemPlanning_WFMLaborPerProcessTypeId",
                table: "WFMLaborItemPlanning",
                column: "WFMLaborPerProcessTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborItemPlanning_WorkerId",
                table: "WFMLaborItemPlanning",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborWorker_PlanningId",
                table: "WFMLaborWorker",
                column: "PlanningId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborWorker_ScheduleId",
                table: "WFMLaborWorker",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborWorker_WorkerId",
                table: "WFMLaborWorker",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborWorkerPerFlow_PlanningId",
                table: "WFMLaborWorkerPerFlow",
                column: "PlanningId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborWorkerPerFlow_ScheduleId",
                table: "WFMLaborWorkerPerFlow",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborWorkerPerFlow_WFMLaborId",
                table: "WFMLaborWorkerPerFlow",
                column: "WFMLaborId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborWorkerPerFlow_WorkerId",
                table: "WFMLaborWorkerPerFlow",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborWorkerPerProcessType_PlanningId",
                table: "WFMLaborWorkerPerProcessType",
                column: "PlanningId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborWorkerPerProcessType_ScheduleId",
                table: "WFMLaborWorkerPerProcessType",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborWorkerPerProcessType_WFMLaborPerFlowId",
                table: "WFMLaborWorkerPerProcessType",
                column: "WFMLaborPerFlowId");

            migrationBuilder.CreateIndex(
                name: "IX_WFMLaborWorkerPerProcessType_WorkerId",
                table: "WFMLaborWorkerPerProcessType",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_Widgets_WarehouseId",
                table: "Widgets",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Workers_RolId",
                table: "Workers",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_Workers_TeamId",
                table: "Workers",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrdersPlanning_AssignedDockId",
                table: "WorkOrdersPlanning",
                column: "AssignedDockId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrdersPlanning_InputOrderId",
                table: "WorkOrdersPlanning",
                column: "InputOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrdersPlanning_PlanningId",
                table: "WorkOrdersPlanning",
                column: "PlanningId");

            migrationBuilder.CreateIndex(
                name: "IX_YardMetricsAppointments_DockId",
                table: "YardMetricsAppointments",
                column: "DockId");

            migrationBuilder.CreateIndex(
                name: "IX_YardMetricsAppointments_PlanningId",
                table: "YardMetricsAppointments",
                column: "PlanningId");

            migrationBuilder.CreateIndex(
                name: "IX_YardMetricsAppointments_YardMetricsPerDockId",
                table: "YardMetricsAppointments",
                column: "YardMetricsPerDockId");

            migrationBuilder.CreateIndex(
                name: "IX_YardMetricsPerDock_DockId",
                table: "YardMetricsPerDock",
                column: "DockId");

            migrationBuilder.CreateIndex(
                name: "IX_YardMetricsPerDock_PlanningId",
                table: "YardMetricsPerDock",
                column: "PlanningId");

            migrationBuilder.CreateIndex(
                name: "IX_Zones_AreaId",
                table: "Zones",
                column: "AreaId");

            migrationBuilder.Sql(@"
CREATE OR REPLACE VIEW public.planningresourcesview
AS WITH plan AS (
         SELECT DISTINCT ""Plannings"".""Id"",
            ""Plannings"".""CreationDate""
           FROM ""Plannings""
          ORDER BY ""Plannings"".""CreationDate"" DESC
         LIMIT 1
        ), itemplanning AS (
         SELECT ip.""Id"",
            ip.""ProcessId"",
            ip.""IsOutbound"",
            ip.""LimitDate"",
            ip.""InitDate"",
            ip.""EndDate"",
            ip.""WorkTime"",
            ip.""IsStored"",
            ip.""IsBlocked"",
            ip.""IsStarted"",
            ip.""WorkOrderPlanningId"",
            ip.""WorkerId"",
            ip.""EquipmentGroupId"",
            p.""Id""
           FROM plan p
             JOIN ""WorkOrdersPlanning"" wo ON p.""Id"" = wo.""PlanningId""
             JOIN ""ItemsPlanning"" ip ON wo.""Id"" = ip.""WorkOrderPlanningId""
        ), limits AS (
         SELECT date_trunc('day'::text, now()) AS firsthour,
            date_trunc('hour'::text, max(itemplanning.""EndDate"")) + '01:00:00'::interval AS lasthour
           FROM itemplanning itemplanning(""Id"", ""ProcessId"", ""IsOutbound"", ""LimitDate"", ""InitDate"", ""EndDate"", ""WorkTime"", ""IsStored"", ""IsBlocked"", ""IsStarted"", ""WorkOrderPlanningId"", ""WorkerId"", ""EquipmentGroupId"", ""Id_1"")
        ), hours AS (
         SELECT generate_series(( SELECT limits.firsthour
                   FROM limits), ( SELECT limits.lasthour
                   FROM limits), '01:00:00'::interval) AS hora
        ), worksbyhour AS (
         SELECT h.hora,
            i.""InitDate"",
            i.""EndDate"",
            i.""ProcessId"",
            i.""EquipmentGroupId"",
            i.""WorkerId""
           FROM hours h
             LEFT JOIN itemplanning i(""Id"", ""ProcessId"", ""IsOutbound"", ""LimitDate"", ""InitDate"", ""EndDate"", ""WorkTime"", ""IsStored"", ""IsBlocked"", ""IsStarted"", ""WorkOrderPlanningId"", ""WorkerId"", ""EquipmentGroupId"", ""Id_1"") ON i.""InitDate"" < (h.hora + '01:00:00'::interval) AND i.""EndDate"" >= h.hora
        ), warehousebyhour AS (
         SELECT h.hora,
            i.""InitDate"",
            i.""EndDate"",
            i.""ProcessId"",
            i.""EquipmentGroupId"",
            i.""WorkerId""
           FROM hours h
             LEFT JOIN ""WarehouseProcessPlanning"" i ON i.""InitDate"" < (h.hora + '01:00:00'::interval) AND i.""EndDate"" >= h.hora
        ), usedbyhour AS (
         SELECT warehousebyhour.hora,
            warehousebyhour.""InitDate"",
            warehousebyhour.""EndDate"",
            warehousebyhour.""ProcessId"",
            warehousebyhour.""EquipmentGroupId"",
            warehousebyhour.""WorkerId""
           FROM warehousebyhour
        UNION ALL
         SELECT worksbyhour.hora,
            worksbyhour.""InitDate"",
            worksbyhour.""EndDate"",
            worksbyhour.""ProcessId"",
            worksbyhour.""EquipmentGroupId"",
            worksbyhour.""WorkerId""
           FROM worksbyhour
        ), workersbyrol AS (
         SELECT w.""RolId"",
            count(DISTINCT aw.""WorkerId"") AS ""AvailableWorkers""
           FROM ""Workers"" w
             JOIN ""AvailableWorkers"" aw ON w.""Id"" = aw.""WorkerId""
          GROUP BY w.""RolId""
        ), allinformation AS (
         SELECT ua.""When"",
            ua.""InitDate"",
            ua.""EndDate"",
            ua.""ProcessId"",
            ua.""ProcessName"",
            ua.""ResourceType"",
            ua.""ResourceId"",
            ua.""ResourceName"",
            ua.""AvailableResources""
           FROM ( SELECT u.hora AS ""When"",
                    u.""InitDate"",
                    u.""EndDate"",
                    u.""ProcessId"",
                    pc.""Name"" AS ""ProcessName"",
                    'Operator'::text AS ""ResourceType"",
                    u.""WorkerId"" AS ""ResourceId"",
                    ro.""Name"" AS ""ResourceName"",
                    wr.""AvailableWorkers"" AS ""AvailableResources""
                   FROM usedbyhour u
                     LEFT JOIN ""Workers"" w ON u.""WorkerId"" = w.""Id""
                     FULL JOIN ""Roles"" ro ON w.""RolId"" = ro.""Id""
                     FULL JOIN ""Processes"" pc ON u.""ProcessId"" = pc.""Id""
                     LEFT JOIN ""EquipmentGroups"" eg ON pc.""AreaId"" = eg.""AreaId"" AND eg.""Id"" = u.""EquipmentGroupId""
                     LEFT JOIN workersbyrol wr ON w.""RolId"" = wr.""RolId""
                UNION ALL
                 SELECT u.hora AS ""When"",
                    u.""InitDate"",
                    u.""EndDate"",
                    u.""ProcessId"",
                    pc.""Name"" AS ""ProcessName"",
                    'Equipment'::text AS ""ResourceType"",
                    eg.""TypeEquipmentId"" AS ""ResourceId"",
                    eg.""Name"" AS ""ResourceName"",
                    eg.""Equipments"" AS ""AvailableResources""
                   FROM usedbyhour u
                     FULL JOIN ""Processes"" pc ON u.""ProcessId"" = pc.""Id""
                     LEFT JOIN ""EquipmentGroups"" eg ON pc.""AreaId"" = eg.""AreaId"" AND eg.""Id"" = u.""EquipmentGroupId"") ua
        ), resourcesandprocess AS (
         SELECT DISTINCT h.""When"",
            a_1.""Name"" AS ""ProcessName"",
            b.""Name"" AS ""ResourceName""
           FROM allinformation h
             CROSS JOIN ""Processes"" a_1
             CROSS JOIN ""Roles"" b
        ), availableresourcesnulls AS (
         SELECT DISTINCT ai.""ProcessName"",
            ai.""ResourceName"",
            COALESCE(max(ai.""AvailableResources""), 0::bigint) AS ""AvailableResources""
           FROM allinformation ai
          GROUP BY ai.""ProcessName"", ai.""ResourceName""
        )
 SELECT r.""When"",
    a.""InitDate"",
    a.""EndDate"",
    a.""ProcessId"",
    r.""ProcessName"",
    a.""ResourceType"",
    a.""ResourceId"",
    r.""ResourceName"",
    COALESCE(a.""AvailableResources"", an.""AvailableResources"", 0::bigint) AS ""AvailableResources""
   FROM resourcesandprocess r
     LEFT JOIN allinformation a ON r.""When"" = a.""When"" AND r.""ProcessName"" = a.""ProcessName"" AND r.""ResourceName"" = a.""ResourceName""
     LEFT JOIN availableresourcesnulls an ON r.""ProcessName"" = an.""ProcessName"" AND r.""ResourceName"" = an.""ResourceName""
  WHERE r.""When"" IS NOT NULL
  ORDER BY r.""When"", r.""ProcessName"", r.""ResourceName"";
            ");

            migrationBuilder.Sql(@"
            DROP VIEW IF EXISTS public.processinterleavingview ;
CREATE OR REPLACE VIEW public.processinterleavingview
AS WITH plan AS (
         SELECT DISTINCT ""Plannings"".""Id"",
            ""Plannings"".""CreationDate""
           FROM ""Plannings""
          ORDER BY ""Plannings"".""CreationDate"" DESC
         LIMIT 1
        ), itemplanning AS (
         SELECT ip.""Id"",
            ip.""ProcessId"",
            ip.""IsOutbound"",
            ip.""LimitDate"",
            ip.""InitDate"",
            ip.""EndDate"",
            ip.""WorkTime"",
            ip.""IsStored"",
            ip.""IsBlocked"",
            ip.""IsStarted"",
            ip.""WorkOrderPlanningId"",
            ip.""WorkerId"",
            ip.""EquipmentGroupId""
           FROM plan p
             JOIN ""WorkOrdersPlanning"" wo ON p.""Id"" = wo.""PlanningId""
             JOIN ""ItemsPlanning"" ip ON wo.""Id"" = ip.""WorkOrderPlanningId""
        ), limits AS (
         SELECT date_trunc('day'::text, now()) AS firsthour,
            date_trunc('hour'::text, max(itemplanning.""EndDate"")) + '01:00:00'::interval AS lasthour
           FROM itemplanning
        ), horas AS (
         SELECT generate_series(( SELECT limits.firsthour
                   FROM limits), ( SELECT limits.lasthour
                   FROM limits), '01:00:00'::interval) AS hour
        ), intervals AS (
         SELECT DISTINCT itemplanning.""InitDate"",
            itemplanning.""EndDate"",
            itemplanning.""IsOutbound""
           FROM itemplanning
        ), intervals_c AS (
         SELECT h_1.hour,
            i.""IsOutbound"",
            GREATEST(i.""InitDate"", h_1.hour) AS ""InitDate"",
            LEAST(i.""EndDate"", h_1.hour + '01:00:00'::interval) AS ""EndDate""
           FROM intervals i
             JOIN horas h_1 ON i.""InitDate"" < (h_1.hour + '01:00:00'::interval) AND i.""EndDate"" >= h_1.hour
        ), intervals_dup AS (
         SELECT DISTINCT second.hour,
            second.""IsOutbound"",
            second.""InitDate"",
            second.""EndDate""
           FROM ( SELECT first.hour,
                    first.""IsOutbound"",
                    first.""InitDate"",
                    first.""EndDate"",
                    first.rn1,
                    row_number() OVER (PARTITION BY first.""IsOutbound"", first.hour, first.""EndDate"" ORDER BY first.""InitDate"") AS rn2
                   FROM ( SELECT intervals_c.hour,
                            intervals_c.""IsOutbound"",
                            intervals_c.""InitDate"",
                            intervals_c.""EndDate"",
                            row_number() OVER (PARTITION BY intervals_c.""IsOutbound"", intervals_c.hour, intervals_c.""InitDate"" ORDER BY intervals_c.""EndDate"" DESC) AS rn1
                           FROM intervals_c) first
                  WHERE first.rn1 = 1) second
          WHERE second.rn2 = 1
          ORDER BY second.""IsOutbound"", second.hour, second.""InitDate"", second.""EndDate""
        ), intervals_lag AS (
         SELECT intervals_dup.hour,
            intervals_dup.""IsOutbound"",
            intervals_dup.""InitDate"",
            intervals_dup.""EndDate"",
            lag(intervals_dup.""InitDate"") OVER (PARTITION BY intervals_dup.hour, intervals_dup.""IsOutbound"" ORDER BY intervals_dup.""InitDate"") AS prev_init_date,
            max(intervals_dup.""EndDate"") OVER (PARTITION BY intervals_dup.hour, intervals_dup.""IsOutbound"" ORDER BY intervals_dup.""InitDate"" ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING) AS prev_end_date
           FROM intervals_dup
        ), intervals_withgroups AS (
         SELECT intervals_lag.hour,
            intervals_lag.""IsOutbound"",
            intervals_lag.""InitDate"",
            intervals_lag.""EndDate"",
            intervals_lag.prev_init_date,
            intervals_lag.prev_end_date,
                CASE
                    WHEN intervals_lag.prev_end_date IS NULL THEN 1
                    WHEN intervals_lag.""InitDate"" > intervals_lag.prev_end_date THEN 1
                    ELSE 0
                END AS is_new_group
           FROM intervals_lag
        ), intervals_groups AS (
         SELECT intervals_withgroups.hour,
            intervals_withgroups.""IsOutbound"",
            intervals_withgroups.""InitDate"",
            intervals_withgroups.""EndDate"",
            intervals_withgroups.prev_init_date,
            intervals_withgroups.prev_end_date,
            intervals_withgroups.is_new_group,
            sum(intervals_withgroups.is_new_group) OVER (PARTITION BY intervals_withgroups.hour, intervals_withgroups.""IsOutbound"" ORDER BY intervals_withgroups.""InitDate"") AS grupo
           FROM intervals_withgroups
        ), intervals_merged AS (
         SELECT intervals_groups.hour,
            intervals_groups.""IsOutbound"",
            min(intervals_groups.""InitDate"") AS ""InitDate"",
            max(intervals_groups.""EndDate"") AS ""EndDate""
           FROM intervals_groups
          GROUP BY intervals_groups.hour, intervals_groups.""IsOutbound"", intervals_groups.grupo
        ), intervals_relag AS (
         SELECT intervals_merged.hour,
            intervals_merged.""IsOutbound"",
            intervals_merged.""InitDate"",
            intervals_merged.""EndDate"",
            lag(intervals_merged.""EndDate"") OVER (PARTITION BY intervals_merged.hour, intervals_merged.""IsOutbound"" ORDER BY intervals_merged.""InitDate"") AS prev_end_date,
            lag(intervals_merged.""InitDate"") OVER (PARTITION BY intervals_merged.hour, intervals_merged.""IsOutbound"" ORDER BY intervals_merged.""EndDate"") AS prev_init_date
           FROM intervals_merged
        ), intervals_regrouped AS (
         SELECT intervals_relag.hour,
            intervals_relag.""IsOutbound"",
            intervals_relag.""InitDate"",
            intervals_relag.""EndDate"",
            intervals_relag.prev_end_date,
            intervals_relag.prev_init_date,
                CASE
                    WHEN intervals_relag.prev_end_date IS NULL THEN 1
                    WHEN intervals_relag.""InitDate"" > intervals_relag.prev_end_date THEN 1
                    ELSE 0
                END AS is_new_group
           FROM intervals_relag
        ), intervals_regrupo AS (
         SELECT intervals_regrouped.hour,
            intervals_regrouped.""IsOutbound"",
            intervals_regrouped.""InitDate"",
            intervals_regrouped.""EndDate"",
            intervals_regrouped.prev_end_date,
            intervals_regrouped.prev_init_date,
            intervals_regrouped.is_new_group,
            sum(intervals_regrouped.is_new_group) OVER (PARTITION BY intervals_regrouped.hour, intervals_regrouped.""IsOutbound"" ORDER BY intervals_regrouped.""InitDate"") AS grupo
           FROM intervals_regrouped
        ), intervals_final AS (
         SELECT intervals_regrupo.hour,
            intervals_regrupo.""IsOutbound"",
            min(intervals_regrupo.""InitDate"") AS ""InitDate"",
            max(intervals_regrupo.""EndDate"") AS ""EndDate""
           FROM intervals_regrupo
          GROUP BY intervals_regrupo.hour, intervals_regrupo.""IsOutbound"", intervals_regrupo.grupo
        ), combined AS (
         SELECT t.hour,
            GREATEST(t.""InitDate"", f.""InitDate"") AS ""InitDate"",
            LEAST(t.""EndDate"", f.""EndDate"") AS ""EndDate""
           FROM intervals_final t
             JOIN intervals_final f ON t.""IsOutbound"" = true AND f.""IsOutbound"" = false AND t.""InitDate"" < f.""EndDate"" AND t.""EndDate"" > f.""InitDate""
        )
 SELECT h.hour AS ""When"",
    (COALESCE(b.inboundtime, 0::numeric) - COALESCE(c.interleavingtime, 0::numeric)) / 3600::numeric AS ""InboundSec"",
    (COALESCE(a.outboundtime, 0::numeric) - COALESCE(c.interleavingtime, 0::numeric)) / 3600::numeric AS ""OutboundSec"",
    COALESCE(c.interleavingtime, 0::numeric) / 3600::numeric AS ""InterleavingSec"",
    (3600::numeric - (COALESCE(a.outboundtime, 0::numeric) - COALESCE(c.interleavingtime, 0::numeric) + (COALESCE(b.inboundtime, 0::numeric) - COALESCE(c.interleavingtime, 0::numeric)) + COALESCE(c.interleavingtime, 0::numeric))) / 3600::numeric AS ""NoWorkingSec""
   FROM horas h
     LEFT JOIN ( SELECT intervals_final.hour,
            sum(
                CASE
                    WHEN intervals_final.""IsOutbound"" = true THEN EXTRACT(epoch FROM intervals_final.""EndDate"" - intervals_final.""InitDate"")
                    ELSE 0::numeric
                END) AS outboundtime
           FROM intervals_final
          GROUP BY intervals_final.hour) a ON h.hour = a.hour
     LEFT JOIN ( SELECT intervals_final.hour,
            sum(
                CASE
                    WHEN intervals_final.""IsOutbound"" = false THEN EXTRACT(epoch FROM intervals_final.""EndDate"" - intervals_final.""InitDate"")
                    ELSE 0::numeric
                END) AS inboundtime
           FROM intervals_final
          GROUP BY intervals_final.hour) b ON COALESCE(a.hour, h.hour) = b.hour
     LEFT JOIN ( SELECT combined.hour,
            sum(EXTRACT(epoch FROM combined.""EndDate"" - combined.""InitDate"")) AS interleavingtime
           FROM combined
          GROUP BY combined.hour) c ON COALESCE(a.hour, b.hour, h.hour) = c.hour
  ORDER BY a.hour;
 
            ");

            migrationBuilder.Sql(@"
CREATE OR REPLACE VIEW public.areaview
AS WITH plan AS (
         SELECT DISTINCT ""Plannings"".""Id"",
            ""Plannings"".""CreationDate""
           FROM ""Plannings""
          ORDER BY ""Plannings"".""CreationDate"" DESC
         LIMIT 1
        ), itemplanning AS (
         SELECT ip.""Id"",
            a.""Name"" AS ""AreaName"",
            ip.""InitDate"",
            ip.""EndDate"",
            ip.""WorkTime"",
            ip.""IsStored"",
            ip.""IsBlocked"",
            ip.""IsStarted"",
            ip.""WorkOrderPlanningId"",
            ip.""WorkerId"",
            ip.""EquipmentGroupId""
           FROM plan p
             JOIN ""WorkOrdersPlanning"" wo ON p.""Id"" = wo.""PlanningId""
             JOIN ""ItemsPlanning"" ip ON wo.""Id"" = ip.""WorkOrderPlanningId""
             JOIN ""Processes"" po ON ip.""ProcessId"" = po.""Id""
             JOIN ""Areas"" a ON po.""AreaId"" = a.""Id""
        ), limits AS (
         SELECT date_trunc('day'::text, now()) AS firsthour,
            date_trunc('hour'::text, max(itemplanning.""EndDate"")) + '01:00:00'::interval AS lasthour
           FROM itemplanning
        ), horas AS (
         SELECT generate_series(( SELECT limits.firsthour
                   FROM limits), ( SELECT limits.lasthour
                   FROM limits), '01:00:00'::interval) AS hora
        ), intervals AS (
         SELECT DISTINCT itemplanning.""InitDate"",
            itemplanning.""EndDate"",
            itemplanning.""AreaName""
           FROM itemplanning
        ), cortes AS (
         SELECT h_1.hora,
            i.""AreaName"",
            GREATEST(i.""InitDate"", h_1.hora) AS ""InitDate"",
            LEAST(i.""EndDate"", h_1.hora + '01:00:00'::interval) AS ""EndDate""
           FROM intervals i
              JOIN horas h_1 ON i.""InitDate"" < (h_1.hora + '01:00:00'::interval) AND i.""EndDate"" >= h_1.hora
        ), groups AS (
         SELECT sub.""AreaName"",
            sub.""InitDate"",
            sub.""EndDate"",
            sum(sub.change) OVER (PARTITION BY sub.""AreaName"" ORDER BY sub.""InitDate"") AS group_id
           FROM ( SELECT cortes.hora,
                    cortes.""AreaName"",
                    cortes.""InitDate"",
                    cortes.""EndDate"",
                        CASE
                            WHEN cortes.""InitDate"" > COALESCE(lag(cortes.""EndDate"") OVER (PARTITION BY cortes.""AreaName"" ORDER BY cortes.""InitDate""), cortes.""InitDate"") THEN 1
                            ELSE 0
                        END AS change
                   FROM cortes) sub
        ), grouped_intervals AS (
         SELECT groups.""AreaName"",
            min(groups.""InitDate"") AS ""InitDate"",
            max(groups.""EndDate"") AS ""EndDate""
           FROM groups
          GROUP BY groups.""AreaName"", groups.group_id
        ), final_intervals AS (
         SELECT sub.""AreaName"",
            sub.truncated_date AS hour,
            GREATEST(sub.""InitDate"", sub.truncated_date) AS ""InitDate"",
            LEAST(sub.""EndDate"", sub.truncated_date + '01:00:00'::interval) AS ""EndDate""
           FROM ( SELECT grouped_intervals.""AreaName"",
                    grouped_intervals.""InitDate"",
                    grouped_intervals.""EndDate"",
                    generate_series(date_trunc('hour'::text, grouped_intervals.""InitDate""), date_trunc('hour'::text, grouped_intervals.""EndDate""), '01:00:00'::interval) AS truncated_date
                   FROM grouped_intervals) sub
        ), unified_groups AS (
         SELECT sub.""AreaName"",
            sub.hour AS ""When"",
            sub.""InitDate"",
            sub.""EndDate"",
            sum(sub.change) OVER (PARTITION BY sub.""AreaName"" ORDER BY sub.""InitDate"", sub.""EndDate"") AS group_id
           FROM ( SELECT final_intervals.""AreaName"",
                    final_intervals.hour,
                    final_intervals.""InitDate"",
                    final_intervals.""EndDate"",
                        CASE
                            WHEN final_intervals.""InitDate"" > COALESCE(lag(final_intervals.""EndDate"") OVER (PARTITION BY final_intervals.""AreaName"" ORDER BY final_intervals.""InitDate"", final_intervals.""EndDate""), final_intervals.""InitDate"") THEN 1
                            ELSE 0
                        END AS change
                   FROM final_intervals) sub
        ), final_unified_intervals AS (
         SELECT unified_groups.""When"",
            min(unified_groups.""InitDate"") AS ""InitDate"",
            max(unified_groups.""EndDate"") AS ""EndDate"",
            unified_groups.""AreaName""
           FROM unified_groups
          GROUP BY unified_groups.""AreaName"", unified_groups.""When"", unified_groups.group_id
        )
 SELECT h.hora as ""When"",
    a.""AreaName"",
    coalesce(""AreasPercUt"",0) AS AreasPercUt
   FROM horas h
   cross join (select distinct ""Name"" as ""AreaName"" from ""Areas"") a
   left join (
        select distinct ""When"", 
        ""AreaName"",
        sum(EXTRACT(epoch FROM ""EndDate"" - ""InitDate"")) / 3600::numeric as ""AreasPercUt"" 
        from  final_unified_intervals
        GROUP BY ""When"", ""AreaName""
        ORDER BY ""When"", ""AreaName"") fi
    on h.hora= fi.""When"" and a.""AreaName""= fi.""AreaName""
  ORDER BY ""When"", ""AreaName"";            ");

            migrationBuilder.Sql(@"
CREATE OR REPLACE VIEW public.resourcesusageview
as WITH plan AS (
         SELECT DISTINCT ""Plannings"".""Id"",
            ""Plannings"".""CreationDate""
           FROM ""Plannings""
          ORDER BY ""Plannings"".""CreationDate"" DESC
         LIMIT 1
        ), itemplanning AS (
         SELECT ip.""Id"",
            ip.""InitDate"",
            ip.""EndDate"",
            ip.""WorkTime"",
            ip.""IsStored"",
            ip.""IsBlocked"",
            ip.""IsStarted"",
            ip.""WorkOrderPlanningId"",
            ip.""WorkerId"",
            ro.""Name"" as ""ResourceName""
           FROM plan p
             JOIN ""WorkOrdersPlanning"" wo ON p.""Id"" = wo.""PlanningId""
             JOIN ""ItemsPlanning"" ip ON wo.""Id"" = ip.""WorkOrderPlanningId""
             JOIN ""Processes"" po ON ip.""ProcessId"" = po.""Id""
             JOIN ""Areas"" a_1 ON po.""AreaId"" = a_1.""Id""
             LEFT JOIN ""Workers"" w ON ip.""WorkerId"" = w.""Id""
             FULL JOIN ""Roles"" ro ON w.""RolId"" = ro.""Id""
        ), limits AS (
         SELECT date_trunc('day'::text, now()) AS firsthour,
            date_trunc('hour'::text, max(itemplanning.""EndDate"")) + '01:00:00'::interval AS lasthour
           FROM itemplanning
        ), horas AS (
         SELECT generate_series(( SELECT limits.firsthour
                   FROM limits), ( SELECT limits.lasthour
                   FROM limits), '01:00:00'::interval) AS hora
        ), intervals AS (
         SELECT DISTINCT itemplanning.""InitDate"",
            itemplanning.""EndDate"",
            itemplanning.""ResourceName""
           FROM itemplanning
        ) , cortes AS (
         SELECT h_1.hora,
            i.""ResourceName"",
            GREATEST(i.""InitDate"", h_1.hora) AS ""InitDate"",
            LEAST(i.""EndDate"", h_1.hora + '01:00:00'::interval) AS ""EndDate""
           FROM intervals i
             JOIN horas h_1 ON i.""InitDate"" < (h_1.hora + '01:00:00'::interval) AND i.""EndDate"" >= h_1.hora
        ), groups AS (
         SELECT sub.""ResourceName"",
            sub.""InitDate"",
            sub.""EndDate"",
            sum(sub.change) OVER (PARTITION BY sub.""ResourceName"" ORDER BY sub.""InitDate"") AS group_id
           FROM ( SELECT cortes.hora,
                    cortes.""ResourceName"",
                    cortes.""InitDate"",
                    cortes.""EndDate"",
                        CASE
                            WHEN cortes.""InitDate"" > COALESCE(lag(cortes.""EndDate"") OVER (PARTITION BY cortes.""ResourceName"" ORDER BY cortes.""InitDate""), cortes.""InitDate"") THEN 1
                            ELSE 0
                        END AS change
                   FROM cortes) sub
        ), grouped_intervals AS (
         SELECT groups.""ResourceName"",
            min(groups.""InitDate"") AS ""InitDate"",
            max(groups.""EndDate"") AS ""EndDate""
           FROM groups
          GROUP BY groups.""ResourceName"", groups.group_id
        )   , final_intervals AS (
         SELECT sub.""ResourceName"",
            sub.truncated_date AS hour,
            GREATEST(sub.""InitDate"", sub.truncated_date) AS ""InitDate"",
            LEAST(sub.""EndDate"", sub.truncated_date + '01:00:00'::interval) AS ""EndDate""
           FROM ( SELECT grouped_intervals.""ResourceName"",
                    grouped_intervals.""InitDate"",
                    grouped_intervals.""EndDate"",
                    generate_series(date_trunc('hour'::text, grouped_intervals.""InitDate""), date_trunc('hour'::text, grouped_intervals.""EndDate""), '01:00:00'::interval) AS truncated_date
                   FROM grouped_intervals) sub
        ), unified_groups AS (
         SELECT sub.""ResourceName"",
            sub.hour AS ""When"",
            sub.""InitDate"",
            sub.""EndDate"",
            sum(sub.change) OVER (PARTITION BY sub.""ResourceName"" ORDER BY sub.""InitDate"", sub.""EndDate"") AS group_id
           FROM ( SELECT final_intervals.""ResourceName"",
                    final_intervals.hour,
                    final_intervals.""InitDate"",
                    final_intervals.""EndDate"",
                        CASE
                            WHEN final_intervals.""InitDate"" > COALESCE(lag(final_intervals.""EndDate"") OVER (PARTITION BY final_intervals.""ResourceName"" ORDER BY final_intervals.""InitDate"", final_intervals.""EndDate""), final_intervals.""InitDate"") THEN 1
                            ELSE 0
                        END AS change
                   FROM final_intervals) sub
        ), final_unified_intervals AS (
         SELECT unified_groups.""When"",
            min(unified_groups.""InitDate"") AS ""InitDate"",
            max(unified_groups.""EndDate"") AS ""EndDate"",
            unified_groups.""ResourceName""
           FROM unified_groups
          GROUP BY unified_groups.""ResourceName"", unified_groups.""When"", unified_groups.group_id
        ) SELECT h.hora AS ""When"",
    a.""ResourceName"",
    COALESCE(fi.""ResourcesUt"", 0::numeric) AS ResourcesUt
   FROM horas h
     CROSS JOIN ( SELECT DISTINCT ""Roles"".""Name"" AS ""ResourceName""
           FROM ""Roles"") a
     LEFT JOIN ( SELECT DISTINCT final_unified_intervals.""When"",
            final_unified_intervals.""ResourceName"",
            sum(EXTRACT(epoch FROM final_unified_intervals.""EndDate"" - final_unified_intervals.""InitDate"")) / 3600::numeric AS ""ResourcesUt""
           FROM final_unified_intervals
          GROUP BY final_unified_intervals.""When"", final_unified_intervals.""ResourceName""
          ORDER BY final_unified_intervals.""When"", final_unified_intervals.""ResourceName"") fi ON h.hora = fi.""When"" AND a.""ResourceName"" = fi.""ResourceName""
  ORDER BY h.hora, a.""ResourceName"";            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aisles");

            migrationBuilder.DropTable(
                name: "AlertConfigurations");

            migrationBuilder.DropTable(
                name: "AlertFilters");

            migrationBuilder.DropTable(
                name: "AlertMails");

            migrationBuilder.DropTable(
                name: "AlertResponses");

            migrationBuilder.DropTable(
                name: "AutomaticStorages");

            migrationBuilder.DropTable(
                name: "Breaks");

            migrationBuilder.DropTable(
                name: "Buffers");

            migrationBuilder.DropTable(
                name: "ChaoticStorages");

            migrationBuilder.DropTable(
                name: "ConfigurationSequences");

            migrationBuilder.DropTable(
                name: "CustomProcesses");

            migrationBuilder.DropTable(
                name: "DriveIns");

            migrationBuilder.DropTable(
                name: "InboundFlowGraphs");

            migrationBuilder.DropTable(
                name: "Inbounds");

            migrationBuilder.DropTable(
                name: "InputOrderLines");

            migrationBuilder.DropTable(
                name: "InputOrderProcessesClosing");

            migrationBuilder.DropTable(
                name: "ItemsPlanning");

            migrationBuilder.DropTable(
                name: "Loadings");

            migrationBuilder.DropTable(
                name: "Objects");

            migrationBuilder.DropTable(
                name: "ObjectsCanvas");

            migrationBuilder.DropTable(
                name: "OrderLoadRatios");

            migrationBuilder.DropTable(
                name: "OrderPriority");

            migrationBuilder.DropTable(
                name: "OrderSchedules");

            migrationBuilder.DropTable(
                name: "OutboundFlowGraphs");

            migrationBuilder.DropTable(
                name: "Pickings");

            migrationBuilder.DropTable(
                name: "PostprocessProfiles");

            migrationBuilder.DropTable(
                name: "PreprocessProfiles");

            migrationBuilder.DropTable(
                name: "ProcessDirectionProperties");

            migrationBuilder.DropTable(
                name: "ProcessHours");

            migrationBuilder.DropTable(
                name: "ProcessPriorityOrder");

            migrationBuilder.DropTable(
                name: "PutawayProfiles");

            migrationBuilder.DropTable(
                name: "Putaways");

            migrationBuilder.DropTable(
                name: "Racks");

            migrationBuilder.DropTable(
                name: "Receptions");

            migrationBuilder.DropTable(
                name: "Replenishments");

            migrationBuilder.DropTable(
                name: "RolProcessSequences");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "Shippings");

            migrationBuilder.DropTable(
                name: "SLAConfigs");

            migrationBuilder.DropTable(
                name: "Steps");

            migrationBuilder.DropTable(
                name: "StrategySequences");

            migrationBuilder.DropTable(
                name: "UserWarehouse");

            migrationBuilder.DropTable(
                name: "WarehouseProcessPlanning");

            migrationBuilder.DropTable(
                name: "WFMLaborItemPlanning");

            migrationBuilder.DropTable(
                name: "Widgets");

            migrationBuilder.DropTable(
                name: "YardAppointmentsNotifications");

            migrationBuilder.DropTable(
                name: "YardMetricsAppointments");

            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "ConfigurationSequenceHeaders");

            migrationBuilder.DropTable(
                name: "WorkOrdersPlanning");

            migrationBuilder.DropTable(
                name: "LoadProfiles");

            migrationBuilder.DropTable(
                name: "VehicleProfiles");

            migrationBuilder.DropTable(
                name: "DockSelectionStrategies");

            migrationBuilder.DropTable(
                name: "Strategies");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Processes");

            migrationBuilder.DropTable(
                name: "WFMLaborEquipmentPerProcessType");

            migrationBuilder.DropTable(
                name: "WFMLaborWorkerPerProcessType");

            migrationBuilder.DropTable(
                name: "YardMetricsPerDock");

            migrationBuilder.DropTable(
                name: "InputOrders");

            migrationBuilder.DropTable(
                name: "WFMLaborEquipmentPerFlow");

            migrationBuilder.DropTable(
                name: "WFMLaborWorkerPerFlow");

            migrationBuilder.DropTable(
                name: "Docks");

            migrationBuilder.DropTable(
                name: "WFMLaborEquipment");

            migrationBuilder.DropTable(
                name: "WFMLaborWorker");

            migrationBuilder.DropTable(
                name: "Zones");

            migrationBuilder.DropTable(
                name: "EquipmentGroups");

            migrationBuilder.DropTable(
                name: "Plannings");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "Areas");

            migrationBuilder.DropTable(
                name: "TypeEquipment");

            migrationBuilder.DropTable(
                name: "AvailableWorkers");

            migrationBuilder.DropTable(
                name: "BreakProfiles");

            migrationBuilder.DropTable(
                name: "Shifts");

            migrationBuilder.DropTable(
                name: "Layouts");

            migrationBuilder.DropTable(
                name: "Workers");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "TimeZones");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "DateFormats");

            migrationBuilder.DropTable(
                name: "DecimalSeparators");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "SystemOfMeasurements");

            migrationBuilder.DropTable(
                name: "ThousandsSeparators");

            migrationBuilder.Sql("DROP VIEW IF EXISTS public.processinterleavingview ;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS public.planningresourcesview;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS public.areaview;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS public.resourcesusageview;");
        }
    }
}
