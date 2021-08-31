using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace eShopSolution.Data.Migrations
{
    public partial class ChangeFileLengthType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AppRoles",
                keyColumn: "Id",
                keyValue: new Guid("8d04dce2-969a-435d-bba4-df3f325983dc"),
                column: "ConcurrencyStamp",
                value: "0e2461ad-d6e5-403d-873c-c2aa0a60d7a1");

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: new Guid("69bd714f-9576-45ba-b5b7-f00649be00de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "a5297042-2b19-42ba-9060-ea4be815c23f", "AQAAAAEAACcQAAAAELo9aAj5pnrKvl4MYjFEYWP0k9vShve/F3o17qskcRsQWJYAn/7aqp6ClOe+tVsmEQ==" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "DateCreated",
                value: new DateTime(2021, 8, 31, 12, 30, 30, 924, DateTimeKind.Local).AddTicks(4047));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AppRoles",
                keyColumn: "Id",
                keyValue: new Guid("8d04dce2-969a-435d-bba4-df3f325983dc"),
                column: "ConcurrencyStamp",
                value: "a5070fb5-eb5c-45b8-93e5-113168b5e38d");

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: new Guid("69bd714f-9576-45ba-b5b7-f00649be00de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "c81db28b-a712-4edc-8f56-a8cf74cb5cd4", "AQAAAAEAACcQAAAAEAXdU2B53GTlwBzceNYCas/tb73o4WEHhjnJ1uGmMBKqy4cE4DPg/WOXVA0nkh2QOQ==" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "DateCreated",
                value: new DateTime(2021, 8, 31, 11, 29, 10, 258, DateTimeKind.Local).AddTicks(8413));
        }
    }
}
