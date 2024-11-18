namespace MembershipSite.Datalayer.Configuration;

public static partial class TableConfiguration
{
    public static ModelBuilder ConfigureMembers(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Member>(entity =>
        {
            entity.ToTable(nameof(Member));

            entity.HasKey(m => m.MemberNumber);

            entity.Property(m => m.DateRegistered)
                .IsRequired(true);

            entity.Property(m => m.Email)
                .HasMaxLength(MemberFieldLimits.Email)
                .IsRequired(true)
                .IsUnicode(true);

            entity.Property(m => m.MemberNumber)
                .HasMaxLength(MemberFieldLimits.MemberNumber)
                .IsUnicode(false);

            entity.Property(m => m.Name)
                .HasMaxLength(MemberFieldLimits.Name)
                .IsRequired(true)
                .IsUnicode(true);

            entity.Property(m => m.PasswordHash)
                .HasMaxLength(MemberFieldLimits.PasswordHash)
                .IsRequired(true)
                .IsUnicode(true);
        });

        return modelBuilder;
    }
}
