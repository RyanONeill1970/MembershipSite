namespace MembershipSite.Datalayer.Migrations
{
    /// <inheritdoc />
    public partial class UniqueEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update rows with duplicate email addresses so that the duplicated rows have
            // -dupe appended to the email address.
            var sql = """
                WITH DuplicateEmails AS (
                    SELECT 
                        Email,
                        ROW_NUMBER() OVER (PARTITION BY Email ORDER BY MemberNumber) AS RowNum
                    FROM 
                        Member
                )
                UPDATE
                	Member
                SET
                	Member.Email = Member.Email + '-dupe'
                FROM
                	Member
                Inner JOIN
                	DuplicateEmails
                ON	Member.Email = DuplicateEmails.Email
                WHERE
                	DuplicateEmails.RowNum > 1;
                """;
            migrationBuilder.Sql(sql);

            migrationBuilder.CreateIndex(
                name: "IX_Member_Email",
                table: "Member",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Member_Email",
                table: "Member");
        }
    }
}
