namespace MembershipSite.Website.Controllers;

[Authorize(Roles = RoleNames.MemberAdmin)]
[Route("backstage")]
public class BackstageController(AuditService auditService, MemberAdminService memberAdminService) : Controller
{
    [ActionName("audit")]
    [Route("audit", Name = nameof(Audit))]
    [HttpGet]
    public IActionResult Audit()
    {
        return View();
    }

    [ActionName("audit-grid-data")]
    [Route("audit-grid-data", Name = nameof(AuditGridDataAsync))]
    [HttpGet]
    public async Task<JsonResult> AuditGridDataAsync([FromQuery] AuditGridQueryParameters query)
    {
        if (ModelState.IsValid)
        {
            var response = await auditService.AuditAdminSummaryAsync(query);

            return Json(response);
        }

        var errors = ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var modelStateSummary = string.Join(", ", errors.Select(e => $"{e.Key}: {string.Join(", ", e.Value ?? [])}"));
        AppLogging.Write($"AuditGridDataAsync ModelState invalid - {modelStateSummary}");

        return new JsonResult(new { errors })
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
    }

    [ActionName("member-list")]
    [Route("")]
    [Route("member-list", Name = nameof(MemberList))]
    [HttpGet]
    public IActionResult MemberList()
    {
        return View();
    }

    [ActionName("member-grid-data")]
    [Route("member-grid-data", Name = nameof(MemberGridDataAsync))]
    [HttpGet]
    public async Task<JsonResult> MemberGridDataAsync()
    {
        var members = await memberAdminService.MemberAdminSummaryAsync();
        return Json(members);
    }

    [ActionName("save-member-data")]
    [Route("save-member-data")]
    [HttpPost]
    public async Task<IActionResult> SaveMemberDataAsync([FromBody] List<MemberSummaryRow> model)
    {
        if (ModelState.IsValid)
        {
            await memberAdminService.SaveMemberDataAsync(model);
            return Ok();
        }

        return BadRequest();
    }

    [ActionName("upload-members")]
    [Route("upload-members")]
    [HttpGet]
    public IActionResult UploadMembers()
    {
        return View();
    }

    [ActionName("upload-members")]
    [Route("upload-members")]
    [HttpPost]
    public async Task<IActionResult> UploadMembersAsync(UploadMembers model)
    {
        if (ModelState.IsValid)
        {
            var result = await memberAdminService.UploadMembersAsync(model.File!);

            return View("upload-members-result", result);
        }

        return View(model);
    }
}
