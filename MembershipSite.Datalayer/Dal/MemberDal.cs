namespace MembershipSite.Datalayer.Dal;

public class MemberDal(MembershipContext context) : BaseDal<Member>(context)
{
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

    public async Task<Member?> ByMembershipNumberAsync(string memberNumber)
    {
        return await context
            .Members
            .Where(m => m.MemberNumber == memberNumber)
            .FirstOrDefaultAsync();
    }

    public async Task<Member?> ByPasswordResetTokenAsync(Guid passwordResetToken)
    {
        return await context
            .Members
            .Where(m => m.PasswordResetToken == passwordResetToken)
            .FirstOrDefaultAsync();
    }
}
