using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace lab3_kpz.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConditionStates",
                columns: table => new
                {
                    ConditionStateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConditionStateName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConditionStates", x => x.ConditionStateId);
                });

            migrationBuilder.CreateTable(
                name: "EventTypes",
                columns: table => new
                {
                    EventTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypes", x => x.EventTypeId);
                });

            migrationBuilder.CreateTable(
                name: "MetricUnits",
                columns: table => new
                {
                    MetricUnitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitMinValue = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    UnitMaxValue = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricUnits", x => x.MetricUnitId);
                });

            migrationBuilder.CreateTable(
                name: "SeverityLevels",
                columns: table => new
                {
                    SeverityLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeverityName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeverityLevels", x => x.SeverityLevelId);
                });

            migrationBuilder.CreateTable(
                name: "Symptoms",
                columns: table => new
                {
                    SymptomId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SymptomName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Symptoms", x => x.SymptomId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserEmail = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserBirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    UserPhoneNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "DiaryEntries",
                columns: table => new
                {
                    DiaryEntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EntryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EntryNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EntryCreatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    EntryUpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    EntryRowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaryEntries", x => x.DiaryEntryId);
                    table.ForeignKey(
                        name: "FK_DiaryEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ReportPeriodFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ReportPeriodTo = table.Column<DateOnly>(type: "date", nullable: false),
                    ReportGeneratedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ReportSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_Reports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DayEvents",
                columns: table => new
                {
                    DayEventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiaryEntryId = table.Column<int>(type: "int", nullable: false),
                    EventTypeId = table.Column<int>(type: "int", nullable: false),
                    EventStartTime = table.Column<TimeOnly>(type: "time(0)", nullable: false),
                    EventEndTime = table.Column<TimeOnly>(type: "time(0)", nullable: false),
                    IsPreciseTime = table.Column<bool>(type: "bit", nullable: false),
                    EventNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayEvents", x => x.DayEventId);
                    table.ForeignKey(
                        name: "FK_DayEvents_DiaryEntries_DiaryEntryId",
                        column: x => x.DiaryEntryId,
                        principalTable: "DiaryEntries",
                        principalColumn: "DiaryEntryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DayEvents_EventTypes_EventTypeId",
                        column: x => x.EventTypeId,
                        principalTable: "EventTypes",
                        principalColumn: "EventTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DayIntervals",
                columns: table => new
                {
                    DayIntervalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiaryEntryId = table.Column<int>(type: "int", nullable: false),
                    IntervalStartTime = table.Column<TimeOnly>(type: "time(0)", nullable: false),
                    IntervalEndTime = table.Column<TimeOnly>(type: "time(0)", nullable: false),
                    ConditionStateId = table.Column<int>(type: "int", nullable: false),
                    IntervalNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayIntervals", x => x.DayIntervalId);
                    table.ForeignKey(
                        name: "FK_DayIntervals_ConditionStates_ConditionStateId",
                        column: x => x.ConditionStateId,
                        principalTable: "ConditionStates",
                        principalColumn: "ConditionStateId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DayIntervals_DiaryEntries_DiaryEntryId",
                        column: x => x.DiaryEntryId,
                        principalTable: "DiaryEntries",
                        principalColumn: "DiaryEntryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Measurements",
                columns: table => new
                {
                    MeasurementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiaryEntryId = table.Column<int>(type: "int", nullable: false),
                    MeasuringTime = table.Column<TimeOnly>(type: "time(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measurements", x => x.MeasurementId);
                    table.ForeignKey(
                        name: "FK_Measurements_DiaryEntries_DiaryEntryId",
                        column: x => x.DiaryEntryId,
                        principalTable: "DiaryEntries",
                        principalColumn: "DiaryEntryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntervalSymptoms",
                columns: table => new
                {
                    DayIntervalId = table.Column<int>(type: "int", nullable: false),
                    SymptomId = table.Column<int>(type: "int", nullable: false),
                    SeverityLevelId = table.Column<int>(type: "int", nullable: false),
                    SymptomNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntervalSymptoms", x => new { x.DayIntervalId, x.SymptomId });
                    table.ForeignKey(
                        name: "FK_IntervalSymptoms_DayIntervals_DayIntervalId",
                        column: x => x.DayIntervalId,
                        principalTable: "DayIntervals",
                        principalColumn: "DayIntervalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IntervalSymptoms_SeverityLevels_SeverityLevelId",
                        column: x => x.SeverityLevelId,
                        principalTable: "SeverityLevels",
                        principalColumn: "SeverityLevelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IntervalSymptoms_Symptoms_SymptomId",
                        column: x => x.SymptomId,
                        principalTable: "Symptoms",
                        principalColumn: "SymptomId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Metrics",
                columns: table => new
                {
                    MetricId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeasurementId = table.Column<int>(type: "int", nullable: false),
                    MetricUnitId = table.Column<int>(type: "int", nullable: false),
                    MetricName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MetricValue = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metrics", x => x.MetricId);
                    table.ForeignKey(
                        name: "FK_Metrics_Measurements_MeasurementId",
                        column: x => x.MeasurementId,
                        principalTable: "Measurements",
                        principalColumn: "MeasurementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Metrics_MetricUnits_MetricUnitId",
                        column: x => x.MetricUnitId,
                        principalTable: "MetricUnits",
                        principalColumn: "MetricUnitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ConditionStates",
                columns: new[] { "ConditionStateId", "ConditionStateName" },
                values: new object[,]
                {
                    { 1, "Critical" },
                    { 2, "Bad" },
                    { 3, "Fair" },
                    { 4, "Good" },
                    { 5, "Excellent" }
                });

            migrationBuilder.InsertData(
                table: "EventTypes",
                columns: new[] { "EventTypeId", "EventTypeName" },
                values: new object[,]
                {
                    { 1, "Medication Intake" },
                    { 2, "Physical Exercise" },
                    { 3, "Meal" },
                    { 4, "Sleep" },
                    { 5, "Emotional Stress" },
                    { 6, "Rest" }
                });

            migrationBuilder.InsertData(
                table: "MetricUnits",
                columns: new[] { "MetricUnitId", "UnitMaxValue", "UnitMinValue", "UnitName" },
                values: new object[,]
                {
                    { 1, 43.00m, 34.00m, "Degrees Celsius (°C)" },
                    { 2, 220.00m, 30.00m, "Beats Per Minute (bpm)" },
                    { 3, 260.00m, 40.00m, "Millimeters of Mercury (mmHg)" },
                    { 4, 100.00m, 50.00m, "Blood Oxygen Level (SpO2 %)" },
                    { 5, 35.00m, 1.00m, "Blood Glucose (mmol/L)" }
                });

            migrationBuilder.InsertData(
                table: "SeverityLevels",
                columns: new[] { "SeverityLevelId", "SeverityName" },
                values: new object[,]
                {
                    { 1, "Mild" },
                    { 2, "Moderate" },
                    { 3, "Severe" },
                    { 4, "Very Severe" }
                });

            migrationBuilder.InsertData(
                table: "Symptoms",
                columns: new[] { "SymptomId", "SymptomName" },
                values: new object[,]
                {
                    { 1, "Headache" },
                    { 2, "Fatigue" },
                    { 3, "Nausea" },
                    { 4, "Dizziness" },
                    { 5, "Dry Cough" },
                    { 6, "Fever" },
                    { 7, "Shortness of Breath" },
                    { 8, "Joint and Muscle Pain" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConditionStates_ConditionStateName",
                table: "ConditionStates",
                column: "ConditionStateName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DayEvents_DiaryEntryId",
                table: "DayEvents",
                column: "DiaryEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_DayEvents_EventTypeId",
                table: "DayEvents",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DayIntervals_ConditionStateId",
                table: "DayIntervals",
                column: "ConditionStateId");

            migrationBuilder.CreateIndex(
                name: "IX_DayIntervals_DiaryEntryId",
                table: "DayIntervals",
                column: "DiaryEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_DiaryEntries_UserId_EntryDate",
                table: "DiaryEntries",
                columns: new[] { "UserId", "EntryDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventTypes_EventTypeName",
                table: "EventTypes",
                column: "EventTypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntervalSymptoms_SeverityLevelId",
                table: "IntervalSymptoms",
                column: "SeverityLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_IntervalSymptoms_SymptomId",
                table: "IntervalSymptoms",
                column: "SymptomId");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_DiaryEntryId",
                table: "Measurements",
                column: "DiaryEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_Metrics_MeasurementId",
                table: "Metrics",
                column: "MeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_Metrics_MetricUnitId",
                table: "Metrics",
                column: "MetricUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MetricUnits_UnitName",
                table: "MetricUnits",
                column: "UnitName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_UserId",
                table: "Reports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SeverityLevels_SeverityName",
                table: "SeverityLevels",
                column: "SeverityName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Symptoms_SymptomName",
                table: "Symptoms",
                column: "SymptomName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserEmail",
                table: "Users",
                column: "UserEmail",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DayEvents");

            migrationBuilder.DropTable(
                name: "IntervalSymptoms");

            migrationBuilder.DropTable(
                name: "Metrics");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "EventTypes");

            migrationBuilder.DropTable(
                name: "DayIntervals");

            migrationBuilder.DropTable(
                name: "SeverityLevels");

            migrationBuilder.DropTable(
                name: "Symptoms");

            migrationBuilder.DropTable(
                name: "Measurements");

            migrationBuilder.DropTable(
                name: "MetricUnits");

            migrationBuilder.DropTable(
                name: "ConditionStates");

            migrationBuilder.DropTable(
                name: "DiaryEntries");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
