namespace MembershipSite.Website.MvcLogic;

/// <summary>
/// Middleware to monitor HTML responses and inject login/logout links dynamically.
/// </summary>
public class LoginLinksMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ILogger<LoginLinksMiddleware> logger)
    {
        var path = context.Request.Path.Value;

        if (!string.IsNullOrEmpty(path) && path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            var originalResponseBody = context.Response.Body;
            await using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            try
            {
                await next(context);

                // Content type can only be read after the rest of the middleware has run (serve static files etc.).
                if (context.Response.ContentType?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true)
                {
                    // Capture the response content as a string
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    using var reader = new StreamReader(memoryStream);
                    var htmlContent = await reader.ReadToEndAsync();

                    // Normalize the tag for matching
                    var tagStart = "<mws:loginLinks";
                    var tagEnd = "/>";
                    var tagIndex = htmlContent.IndexOf(tagStart, StringComparison.OrdinalIgnoreCase);

                    while (tagIndex != -1)
                    {
                        // Find the closing tag and replace the entire tag
                        var closingIndex = htmlContent.IndexOf(tagEnd, tagIndex, StringComparison.OrdinalIgnoreCase);
                        if (closingIndex != -1)
                        {
                            closingIndex += tagEnd.Length; // Include the closing characters

                            var loginStateText = context.User.Identity?.IsAuthenticated == true
                                ? "<a href='/auth/logout'>Logout</a>"
                                : "<a href='/auth/login'>Login</a>";

                            htmlContent = htmlContent.Substring(0, tagIndex) + loginStateText + htmlContent.Substring(closingIndex);
                        }

                        // Search for the next occurrence
                        tagIndex = htmlContent.IndexOf(tagStart, tagIndex + 1, StringComparison.OrdinalIgnoreCase);
                    }

                    // The pages that this middleware is used on could be cached in the client browser.
                    // If we change the login state, we need to invalidate the client cache.
                    // To do this we need to set the Vary header to "Cookie" to ensure caching is handled based on the login (and all other) cookies.
                    context.Response.Headers.Vary = "Cookie";

                    // Set cache headers to ensure fresh content for dynamically modified pages
                    context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
                    context.Response.Headers.Pragma = "no-cache";
                    context.Response.Headers.Expires = "0";

                    // Reset response stream and write modified content
                    context.Response.Body = originalResponseBody;
                    context.Response.Headers.Remove("Content-Length");
                    await context.Response.WriteAsync(htmlContent);
                }
                else
                {
                    // Restore original response body if no modifications
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    await memoryStream.CopyToAsync(originalResponseBody);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing middleware");
                throw;
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
