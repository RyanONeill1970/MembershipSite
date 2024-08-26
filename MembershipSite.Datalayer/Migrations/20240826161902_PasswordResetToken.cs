using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MembershipSite.Datalayer.Migrations
{
    /// <inheritdoc />
    public partial class PasswordResetToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PasswordResetToken",
                table: "Member",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Member");
        }
    }
}
