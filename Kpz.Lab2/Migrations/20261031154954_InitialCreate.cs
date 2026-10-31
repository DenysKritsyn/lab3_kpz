using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kpz.Lab2.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventTypes",
                columns: table => new
                {
                    EventTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypes", x => x.EventTypeId);
                });

            migrationBuilder.CreateTable(
                name: "MetricTypes",
                columns: table => new
                {
                    MetricTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetricName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MetricUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MetricMinValue = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    MetricMaxValue = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricTypes", x => x.MetricTypeId);
                    table.CheckConstraint("CK_MetricTypes_Range", "[MetricMaxValue] > [MetricMinValue]");
                });

            migrationBuilder.CreateTable(
                name: "SeverityLevels",
                columns: table => new
                {
                    SeverityLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeverityName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SeverityRank = table.Column<byte>(type: "tinyint", nullable: false)
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
                    SymptomName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SymptomCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
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
                    ExternalAuthId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserBirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    UserRole = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UserCreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "WellbeingLevels",
                columns: table => new
                {
                    WellbeingLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WellbeingName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WellbeingRank = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WellbeingLevels", x => x.WellbeingLevelId);
                });

            migrationBuilder.CreateTable(
                name: "DiaryEntries",
                columns: table => new
                {
                    DiaryEntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EntryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EntryNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    EntryCreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntryUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    ReportGeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportContent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.ReportId);
                    table.CheckConstraint("CK_Reports_Period", "[ReportPeriodTo] >= [ReportPeriodFrom]");
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
                    EventHour = table.Column<byte>(type: "tinyint", nullable: true),
                    EventNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayEvents", x => x.DayEventId);
                    table.CheckConstraint("CK_DayEvents_Hour", "[EventHour] IS NULL OR [EventHour] <= 23");
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
                    IntervalStartHour = table.Column<byte>(type: "tinyint", nullable: false),
                    IntervalEndHour = table.Column<byte>(type: "tinyint", nullable: false),
                    WellbeingLevelId = table.Column<int>(type: "int", nullable: false),
                    IntervalNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayIntervals", x => x.DayIntervalId);
                    table.CheckConstraint("CK_DayIntervals_Hours", "[IntervalStartHour] <= 23 AND [IntervalEndHour] >= 1 AND [IntervalEndHour] <= 24 AND [IntervalEndHour] > [IntervalStartHour]");
                    table.ForeignKey(
                        name: "FK_DayIntervals_DiaryEntries_DiaryEntryId",
                        column: x => x.DiaryEntryId,
                        principalTable: "DiaryEntries",
                        principalColumn: "DiaryEntryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DayIntervals_WellbeingLevels_WellbeingLevelId",
                        column: x => x.WellbeingLevelId,
                        principalTable: "WellbeingLevels",
                        principalColumn: "WellbeingLevelId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Measurements",
                columns: table => new
                {
                    MeasurementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiaryEntryId = table.Column<int>(type: "int", nullable: false),
                    MetricTypeId = table.Column<int>(type: "int", nullable: false),
                    MeasurementValue = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    MeasurementHour = table.Column<byte>(type: "tinyint", nullable: true),
                    MeasurementNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measurements", x => x.MeasurementId);
                    table.CheckConstraint("CK_Measurements_Hour", "[MeasurementHour] IS NULL OR [MeasurementHour] <= 23");
                    table.ForeignKey(
                        name: "FK_Measurements_DiaryEntries_DiaryEntryId",
                        column: x => x.DiaryEntryId,
                        principalTable: "DiaryEntries",
                        principalColumn: "DiaryEntryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Measurements_MetricTypes_MetricTypeId",
                        column: x => x.MetricTypeId,
                        principalTable: "MetricTypes",
                        principalColumn: "MetricTypeId",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.InsertData(
                table: "EventTypes",
                columns: new[] { "EventTypeId", "EventName" },
                values: new object[,]
                {
                    { 1, "Stress" },
                    { 2, "Sleep Deprivation" },
                    { 3, "Medication Intake" },
                    { 4, "Physical Exercise" },
                    { 5, "Caffeine or Energy Drink" },
                    { 6, "Weather Change" }
                });

            migrationBuilder.InsertData(
                table: "MetricTypes",
                columns: new[] { "MetricTypeId", "MetricMaxValue", "MetricMinValue", "MetricName", "MetricUnit" },
                values: new object[,]
                {
                    { 1, 43.00m, 34.00m, "Body Temperature", "°C" },
                    { 2, 250.00m, 60.00m, "Systolic Blood Pressure", "mmHg" },
                    { 3, 160.00m, 40.00m, "Diastolic Blood Pressure", "mmHg" },
                    { 4, 220.00m, 35.00m, "Heart Rate", "bpm" },
                    { 5, 100.00m, 70.00m, "Oxygen Saturation", "%" }
                });

            migrationBuilder.InsertData(
                table: "SeverityLevels",
                columns: new[] { "SeverityLevelId", "SeverityName", "SeverityRank" },
                values: new object[,]
                {
                    { 1, "Moderate", (byte)1 },
                    { 2, "Severe", (byte)2 },
                    { 3, "Very Severe", (byte)3 }
                });

            migrationBuilder.InsertData(
                table: "Symptoms",
                columns: new[] { "SymptomId", "SymptomCategory", "SymptomName" },
                values: new object[,]
                {
                    { 1, "Neurological", "Headache" },
                    { 2, "Neurological", "Dizziness" },
                    { 3, "Respiratory", "Cough" },
                    { 4, "Respiratory", "Sore Throat" },
                    { 5, "Respiratory", "Runny Nose" },
                    { 6, "Gastrointestinal", "Nausea" },
                    { 7, "General", "Fatigue" },
                    { 8, "General", "Chills" }
                });

            migrationBuilder.InsertData(
                table: "WellbeingLevels",
                columns: new[] { "WellbeingLevelId", "WellbeingName", "WellbeingRank" },
                values: new object[,]
                {
                    { 1, "Good", (byte)1 },
                    { 2, "Moderate", (byte)2 },
                    { 3, "Poor", (byte)3 },
                    { 4, "Very Poor", (byte)4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DayEvents_DiaryEntryId_EventTypeId",
                table: "DayEvents",
                columns: new[] { "DiaryEntryId", "EventTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_DayEvents_EventTypeId",
                table: "DayEvents",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DayIntervals_DiaryEntryId_IntervalStartHour",
                table: "DayIntervals",
                columns: new[] { "DiaryEntryId", "IntervalStartHour" });

            migrationBuilder.CreateIndex(
                name: "IX_DayIntervals_WellbeingLevelId",
                table: "DayIntervals",
                column: "WellbeingLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_DiaryEntries_EntryDate",
                table: "DiaryEntries",
                column: "EntryDate");

            migrationBuilder.CreateIndex(
                name: "IX_DiaryEntries_UserId_EntryDate",
                table: "DiaryEntries",
                columns: new[] { "UserId", "EntryDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventTypes_EventName",
                table: "EventTypes",
                column: "EventName",
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
                name: "IX_Measurements_DiaryEntryId_MetricTypeId",
                table: "Measurements",
                columns: new[] { "DiaryEntryId", "MetricTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_MetricTypeId",
                table: "Measurements",
                column: "MetricTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MetricTypes_MetricName",
                table: "MetricTypes",
                column: "MetricName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_UserId_ReportGeneratedAt",
                table: "Reports",
                columns: new[] { "UserId", "ReportGeneratedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SeverityLevels_SeverityName",
                table: "SeverityLevels",
                column: "SeverityName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeverityLevels_SeverityRank",
                table: "SeverityLevels",
                column: "SeverityRank",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Symptoms_SymptomCategory",
                table: "Symptoms",
                column: "SymptomCategory");

            migrationBuilder.CreateIndex(
                name: "IX_Symptoms_SymptomName",
                table: "Symptoms",
                column: "SymptomName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ExternalAuthId",
                table: "Users",
                column: "ExternalAuthId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserEmail",
                table: "Users",
                column: "UserEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WellbeingLevels_WellbeingName",
                table: "WellbeingLevels",
                column: "WellbeingName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WellbeingLevels_WellbeingRank",
                table: "WellbeingLevels",
                column: "WellbeingRank",
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
                name: "Measurements");

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
                name: "MetricTypes");

            migrationBuilder.DropTable(
                name: "DiaryEntries");

            migrationBuilder.DropTable(
                name: "WellbeingLevels");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
