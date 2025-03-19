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
    /// <summary>
    /// Accepted by our email provider for delivery to the recipient's server.
    /// Not yet delivered.
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Route("accepted")]
    public async Task<IActionResult> AcceptedReport()
    {
        var payload = await new StreamReader(Request.Body).ReadToEndAsync();
        await emailWebhookHandler.HandleAcceptedAsync(payload);
        return Ok();
    }

    /// <summary>
    /// Delivered to the recipient's server.
    /// If they don't see it, it's in their spam folder.
    /// </summary>
    /// <returns></returns>
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

    [HttpPost]
    [Route("permanent-failure")]
    public async Task<IActionResult> PermanentFailure()
    {
        var payload = await new StreamReader(Request.Body).ReadToEndAsync();
        await emailWebhookHandler.HandlePermanentFailureAsync(payload);
        return Ok();
    }

    [HttpPost]
    [Route("temporary-failure")]
    public async Task<IActionResult> TemporaryFailure()
    {
        var payload = await new StreamReader(Request.Body).ReadToEndAsync();
        await emailWebhookHandler.HandleTemporaryFailureAsync(payload);
        return Ok();
    }

    [HttpPost]
    [Route("unsubscribe")]
    public async Task<IActionResult> Unsubscribe()
    {
        var payload = await new StreamReader(Request.Body).ReadToEndAsync();
        await emailWebhookHandler.HandleUnsubscribeAsync(payload);
        return Ok();
    }
}
