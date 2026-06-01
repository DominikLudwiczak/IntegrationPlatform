using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Progress",
                table: "OperationRecords",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Progress_Range",
                table: "OperationRecords",
                sql: "\"Progress\" >= 0 AND \"Progress\" <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RetryCount",
                table: "OperationRecords",
                sql: "\"RetryCount\" <= \"MaxRetries\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Progress_Range",
                table: "OperationRecords");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RetryCount",
                table: "OperationRecords");

            migrationBuilder.AlterColumn<int>(
                name: "Progress",
                table: "OperationRecords",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
