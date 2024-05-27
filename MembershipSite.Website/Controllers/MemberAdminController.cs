namespace MembershipSite.Website.Controllers;

[Authorize(Roles = "MemberAdmin")] // TODO: Use constant with obfuscated value.
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
