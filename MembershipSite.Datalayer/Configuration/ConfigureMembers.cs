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

            entity.Property(m => m.MemberNumber)
                .HasMaxLength(5) // TODO: Move to common contracts layer so can be enforced in ViewModels.
                .IsUnicode(false);

            entity.Property(m => m.Email)
                .HasMaxLength(100) // TODO: Move to common contracts layer so can be enforced in ViewModels.
                .IsRequired(true)
                .IsUnicode(true);

            entity.Property(m => m.Name)
                .HasMaxLength(100) // TODO: Move to common contracts layer so can be enforced in ViewModels.
                .IsRequired(true)
                .IsUnicode(true);
        });

        return modelBuilder;
    }
}
