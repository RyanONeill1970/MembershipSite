namespace MembershipSite.Datalayer.Dal;

public class MemberDal(MembershipContext context) : BaseDal(context)
{
    public Member Add(string email)
    {
        var row = new Member { Email = email };

        context.Members.Add(row);

        return row;
    }

    public IQueryable<Member> AllAsQueryable()
    {
        return context.Members.AsQueryable();
    }

    public async Task<Member?> ByEmailAsync(string email)
    {
        return await context
            .Members
            .Where(m => m.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task<Member?> ByPasswordResetTokenAsync(Guid passwordResetToken)
    {
        return await context
            .Members
            .Where(m => m.PasswordResetToken == passwordResetToken)
            .FirstOrDefaultAsync();
    }

    public void Delete(string memberNumber)
    {
        context
            .Members
            .Where(m => m.MemberNumber == memberNumber)
            .ExecuteDelete();
    }
}
