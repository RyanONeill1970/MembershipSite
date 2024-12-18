namespace MembershipSite.Datalayer.Dal;

public class BaseDal
{
    protected readonly MembershipContext  context;

    public BaseDal(MembershipContext context)
    {
        this.context = context;
    }

    public void Commit()
    {
        context.SaveChanges();
    }

    public async Task CommitAsync()
    {
        await context.SaveChangesAsync();
    }
}
