namespace MembershipSite.Logic.Services;

using MembershipSite.Datalayer.Models;

public class MemberAdminService(MemberDal memberDal)
{
    public IQueryable<Member> AllAsQueryable()
    {
        return memberDal.AllAsQueryable();
    }
}
