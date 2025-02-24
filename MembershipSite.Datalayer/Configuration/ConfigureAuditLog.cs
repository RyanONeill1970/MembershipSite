namespace MembershipSite.Datalayer.Configuration;

public static partial class TableConfiguration
{
    public static ModelBuilder ConfigureAuditLog(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable(nameof(AuditLog));

            entity.HasKey(m => m.Id);

            entity.Property(m => m.EventOccurred)
                .IsRequired(true);

            entity.Property(m => m.Email)
                .HasMaxLength(AuditLogFieldLimits.Email)
                .IsRequired(true)
                .IsUnicode(true);

            entity.Property(m => m.EventName)
                .HasMaxLength(AuditLogFieldLimits.EventName)
                .IsRequired(true)
                .IsUnicode(true);

            entity.Property(m => m.EventOccurred)
                .IsRequired(true);

            entity.HasIndex(m => m.EventOccurred);

            entity.Property(m => m.Id)
                .IsRequired(true);

            entity.Property(m => m.Success);

            entity.Property(m => m.Payload)
                .IsRequired(true);
        });

        return modelBuilder;
    }
}
