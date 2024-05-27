namespace MembershipSite.Datalayer.Dal;

public class BaseDal<T> where T : class, new()
{
    protected readonly MembershipContext  context;

    public BaseDal(MembershipContext context)
    {
        this.context = context;
    }

    public T Add()
    {
        var row = new T();

        context.Add(row);

        return row;
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
