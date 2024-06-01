namespace MembershipSite.Website.Controllers;

[Authorize(Roles = RoleNames.MemberAdmin)]
public class MemberAdminController(MemberAdminService memberAdminService) : Controller
{
    [ActionName("member-list")]
    [Route("member-list", Name = nameof(MemberList))]
    [HttpGet]
    public async Task<IActionResult> MemberList()
    {
        var model = memberAdminService.AllAsQueryable();
        return View(model);
    }
}
