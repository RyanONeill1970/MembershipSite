namespace MembershipSite.Datalayer.Migrations;

/// <inheritdoc />
public partial class PasswordResetExpiry : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "PasswordResetTokenExpiry",
            table: "Member",
            type: "datetimeoffset",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PasswordResetTokenExpiry",
            table: "Member");
    }
}
