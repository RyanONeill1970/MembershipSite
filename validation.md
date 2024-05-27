MVC does not output proper HTML5 validation and does not allow customisation to work with Bootstrap 5 classes.

See;
	https://andrewlock.net/adding-client-side-validation-to-aspnet-core-without-jquery-or-unobtrusive-validation/
	https://github.com/dotnet/aspnetcore/issues/8573
	https://github.com/dotnet/aspnetcore/issues/17412

We use https://github.com/haacked/aspnet-client-validation to replace jQuery, jQuery validation and unobtrusive validation.
We then customise it to use the Bootstrap 5 styles.
