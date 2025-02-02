namespace MembershipSite.Website.Controllers;

/// <summary>
/// Handles events posted to us by transactional email delivery services.
/// 
/// This version is for Mailgun only.
/// 
/// A future improvement would be to make it generic and handle multiple services.
/// </summary>
[AllowAnonymous]
[Route("email-webhooks")]
[ApiController]
public class EmailWebhooksController(IEmailWebhookHandler emailWebhookHandler) : ControllerBase
{
    [HttpPost]
    [Route("delivery-report")]
    public async Task<IActionResult> DeliveryReport()
    {
        var payload = await new StreamReader(Request.Body).ReadToEndAsync();
        await emailWebhookHandler.HandleDeliveryReportAsync(payload);
        return Ok();
    }

    [HttpPost]
    [Route("spam-report")]
    public async Task<IActionResult> SpamReport()
    {
        var payload = await new StreamReader(Request.Body).ReadToEndAsync();
        await emailWebhookHandler.HandleSpamReportAsync(payload);
        return Ok();
    }
}
