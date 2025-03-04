using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FHealthSphere.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFHealthSphereDBContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BandTypeId",
                table: "Bands");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Metrics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedTime",
                table: "Metrics",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Metrics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedTime",
                table: "Metrics",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                table: "Metrics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastUpdatedTime",
                table: "Metrics",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Metrics");

            migrationBuilder.DropColumn(
                name: "CreatedTime",
                table: "Metrics");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Metrics");

            migrationBuilder.DropColumn(
                name: "DeletedTime",
                table: "Metrics");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "Metrics");

            migrationBuilder.DropColumn(
                name: "LastUpdatedTime",
                table: "Metrics");

            migrationBuilder.AddColumn<int>(
                name: "BandTypeId",
                table: "Bands",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
