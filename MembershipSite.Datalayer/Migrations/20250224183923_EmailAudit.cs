#nullable disable

namespace MembershipSite.Datalayer.Migrations
{
    /// <inheritdoc />
    public partial class EmailAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmailLastFailed",
                table: "Member",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmailLastSucceeded",
                table: "Member",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventOccurred = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Success = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Email",
                table: "AuditLog",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropColumn(
                name: "EmailLastFailed",
                table: "Member");

            migrationBuilder.DropColumn(
                name: "EmailLastSucceeded",
                table: "Member");
        }
    }
}
