namespace MembershipSite.Website.Controllers;

[Authorize(Roles = RoleNames.MemberAdmin)]
[Route("backstage")]
public class BackstageController(MemberAdminService memberAdminService) : Controller
{
    [ActionName("member-list")]
    [Route("member-list", Name = nameof(MemberList))]
    [HttpGet]
    public IActionResult MemberList()
    {
        var model = memberAdminService.AllAsQueryable();
        return View(model);
    }
}
