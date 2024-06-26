﻿## TODO

* 403 page - currently redirecting to login, ideally needs to show 'unauthorised'.
* 404 page - currently redirecting to login, needs to show 'page not found'.
* 500 page - detail mode for dev. Currently not being served.
* Member admin;
	* Approve user with auto ticked option to send welcome email.
	* Delete user.
	* Ability to create a new user, like registration but choosing everything except password for them. Have a link sent to them.
* Azure has put a tracker on the site. Delete it. Shows up in member list in admin section.
* For the forgot password use email instead of member number. People can't remember their member numbers!
* When published, /secure/ is showing a 404 instead of using the default of index.html
* site.css is junk I think. Is it used anywhere?
* Role management;
	* Allow for upload of members via CSV.
	* Prime first users via register and add to db via UserRole table.
* Add a logout button.
* Can we avoid a redirect on /favicon.ico?
* How long is the login cookie at present? Can this be put into the config?
* Bundling - Look at using Bun everywhere and get rid of libman. Only after a version is working. See documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
for details on configuring this project to bundle and minify static web assets.

## Society content / runtime;
* Need favicon generating for all versions.
* Offer to create a better favicon to fit more formats. Maybe we could have one master and output the variations?
* Mailgun has been setup. Issue here is that I own the account for this, which is not a problem but could get confusing.

## Final checks
* Enumerate all file types and check all can be downloaded. Static file options has a setting for this (DOC, PDF, ZIP, video, MP3).
* Redirect loop when trying to access /secure and /auth/login did not exist. Why? What else might this happen with? That should have been a 404, so might happen with some missing files or content stored in auth. Seems to be that the auth directory is secured so anything inside is getting redirected.

## Future
* Password complexity test. Ensure at least 6 chars but also, could use Troy Hunts thing.

## Design
* Following does not work. UI still jumps around on register / login.
```
	span.field-validation-valid {
	    /* Prevent the validation messages from causing the UI to jump around when they appear. */
		display: block;
		width:100%;
		margin-top: .25rem;
		font-size: .875em;
	}
```
