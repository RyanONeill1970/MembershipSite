#nullable disable

namespace MembershipSite.Datalayer.Migrations
{
    /// <inheritdoc />
    public partial class AuditName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLog_Email",
                table: "AuditLog");

            migrationBuilder.AddColumn<string>(
                name: "EventName",
                table: "AuditLog",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_EventOccurred",
                table: "AuditLog",
                column: "EventOccurred");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLog_EventOccurred",
                table: "AuditLog");

            migrationBuilder.DropColumn(
                name: "EventName",
                table: "AuditLog");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Email",
                table: "AuditLog",
                column: "Email",
                unique: true);
        }
    }
}
