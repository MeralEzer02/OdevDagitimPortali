using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ÖdevDağıtım.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedbackToSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "Submissions");
        }
    }
}
