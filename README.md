# MembershipSite

A membership management website for securing access to web pages and documents to members only. The /secure folder is secured by default, all other pages are public.

Useful for clubs, societies, and other organisations that need to restrict access to certain documents or areas of their website.

If you have a need for a simple membership system, this project may be a good starting point.

If you think it's missing some functionality, either raise an issue or contact me (ryan at ryanoneill.com) and I may be able to help.

## Hosting
* Requires .Net 8, can run in Linux plans. I've tested this on Microsoft Azure / Linux
* Requires Microsoft SQL Server
* If you don't want to host your own instance and are a society / club, I may be able to host it for a reasonable fee (email ryan at ryanoneill.com). This would help me offset the costs of further development.

## Getting started
* Clone the repository
* Set up a github action workflow to publish on commit to your preferred hosting provider
* Override app settings for email and society name via environment variables
* Upload your members only pages / documents to the secure folder
* Upload public content (plain HTML pages or whatever you use) to the wwwroot folder
 
## Handles
* Authorisation of members
* Registration of new members
* Login of members and administrators
* Password reset for members
* Administration section for managing users
* Sending emails for auth processes

## Possible improvements
* Add unit tests to placeholder project
* Add PlayWright integration tests
* Payment integration
* Alternative data stores (SQL Lite, Postgresql, Mongo, raw JSON)
* Email campaign integration (MailChimp etc.)
* A better name

## Tech
* Written in C# using ASP.NET 8.
* Uses Microsoft SQL Server and Entity Framework (may be overkill, other options can be added in future)
