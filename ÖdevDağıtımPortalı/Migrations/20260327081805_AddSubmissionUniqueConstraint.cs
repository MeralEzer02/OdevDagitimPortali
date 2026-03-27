using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ÖdevDağıtım.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Submissions_AssignmentId_StudentId",
                table: "Submissions",
                columns: new[] { "AssignmentId", "StudentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Submissions_AssignmentId_StudentId",
                table: "Submissions");
        }
    }
}
