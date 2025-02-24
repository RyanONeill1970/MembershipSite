namespace MembershipSite.Datalayer;

public class MembershipContext : DbContext, IDataProtectionKeyContext
{
    public MembershipContext()
    {
    }

    public MembershipContext(DbContextOptions<MembershipContext> options)
        : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs { get; set; }

    public DbSet<Member> Members { get; set; }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ConfigureAuditLog()
            .ConfigureMembers();
    }
}
