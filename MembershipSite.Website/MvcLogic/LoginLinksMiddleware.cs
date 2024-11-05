namespace MembershipSite.Website.MvcLogic;

/// <summary>
/// Monitors the response for HTML content and injects login links.
/// </summary>
/// <param name="next"></param>
public class LoginLinksMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ILogger<LoginLinksMiddleware> logger)
    {
        var path = context.Request.Path.Value;

        if (path != null && path.EndsWith(".html"))
        {
            using var memoryStream = new MemoryStream();

            var originalResponseBody = context.Response.Body;
            context.Response.Body = memoryStream;

            try
            {
                await next(context);

                // Content type can only be read after the rest of the middleware has run (serve static files etc.).
                if (context.Response.ContentType?.Contains("text/html") == true)
                {
                    // Capture the response content as a string
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    using var reader = new StreamReader(memoryStream);
                    var htmlContent = await reader.ReadToEndAsync();

                    // TODO: Handle space before self closing tag and capitalisation.
                    // TODO: Could we push this markup through the tag helper to process it and keep both uses in the same place?
                    if (htmlContent.Contains("<mws:loginLinks />"))
                    {
                        // Modify the content based on login state
                        var loginStateText = context.User.Identity?.IsAuthenticated ?? false
                            ? "<a href='/auth/logout'>Logout</a>"
                            : "<a href='/auth/login'>Login</a>";

                        htmlContent = htmlContent.Replace("<mws:loginLinks />", loginStateText);

                        // The pages that this middleware is used on could be cached in the client browser.
                        // If we change the login state, we need to invalidate the client cache.
                        // To do this we need to set the Vary header to "Cookie" to ensure caching is handled based on the login (and all other) cookies.
                        context.Response.Headers.Vary = "Cookie";

                        // Set cache headers to ensure fresh content for dynamically modified pages
                        context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
                        context.Response.Headers.Pragma = "no-cache";
                        context.Response.Headers.Expires = "0";
                    }

                    // Write the modified content back to the original response body
                    context.Response.Body = originalResponseBody;
                    context.Response.Headers.Remove("Content-Length");
                    await context.Response.WriteAsync(htmlContent);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while processing the request.");
            }
            finally
            {
                context.Response.Body = originalResponseBody;
            }
        }
        else
        {
            await next(context); // Proceed if it's not an HTML file
        }
    }
}
