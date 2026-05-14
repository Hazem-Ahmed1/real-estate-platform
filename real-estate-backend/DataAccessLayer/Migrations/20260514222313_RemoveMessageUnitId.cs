using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMessageUnitId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Units_UnitId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_UnitId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "Messages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_UnitId",
                table: "Messages",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Units_UnitId",
                table: "Messages",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "UnitId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
