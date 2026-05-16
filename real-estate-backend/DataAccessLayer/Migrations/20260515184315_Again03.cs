using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class Again03 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nearby_Facilities_Projects_ProjectId",
                table: "Nearby_Facilities");

            migrationBuilder.DropIndex(
                name: "IX_Nearby_Facilities_ProjectId",
                table: "Nearby_Facilities");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Nearby_Facilities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Nearby_Facilities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Nearby_Facilities_ProjectId",
                table: "Nearby_Facilities",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Nearby_Facilities_Projects_ProjectId",
                table: "Nearby_Facilities",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId");
        }
    }
}
