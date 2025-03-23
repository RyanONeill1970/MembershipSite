namespace MembershipSite.Datalayer.Migrations
{
    /// <inheritdoc />
    public partial class EmailAsKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Deduplicate email addresses
            migrationBuilder.Sql(@"
                WITH CTE_Duplicates AS (
                    SELECT 
                        Email,
                        ROW_NUMBER() OVER (PARTITION BY Email ORDER BY (SELECT NULL)) AS RowNum
                    FROM 
                        Member
                )
                UPDATE Member
                SET Email = CONCAT(Member.Email, '-dupe-', LEFT(NEWID(), 8))
                FROM Member
                INNER JOIN CTE_Duplicates
                ON Member.Email = CTE_Duplicates.Email
                WHERE CTE_Duplicates.RowNum > 1
                AND LEN(CONCAT(Member.Email, '-dupe-', LEFT(NEWID(), 8))) <= 100;
              ");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Member",
                table: "Member");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Member",
                table: "Member",
                column: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Member",
                table: "Member");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Member",
                table: "Member",
                column: "MemberNumber");
        }
    }
}
