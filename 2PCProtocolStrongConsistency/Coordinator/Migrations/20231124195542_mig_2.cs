using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Coordinator.Migrations
{
    /// <inheritdoc />
    public partial class mig2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Nodes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("670532a1-05ba-4b00-9807-97ae797ded50"), "Stock.API" },
                    { new Guid("c599393b-7375-4345-9858-da0e768051f7"), "Order.API" },
                    { new Guid("c8a3226f-144a-425f-851f-0bbfe1aac27d"), "Payment.API" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("670532a1-05ba-4b00-9807-97ae797ded50"));

            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("c599393b-7375-4345-9858-da0e768051f7"));

            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("c8a3226f-144a-425f-851f-0bbfe1aac27d"));
        }
    }
}
