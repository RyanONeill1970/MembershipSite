namespace MembershipSite.UnitTests.MailgunTests;

using MembershipSite.Logic.Mail.Mailgun;
using MembershipSite.Logic.Mail.Mailgun.Models;
using MembershipSite.ViewModels;

public class WebhookDeserialisationTests
{
    private readonly EmailConfig emailConfig;
    private readonly MailgunWebhookHandler mailgunWebhookHandler;

    public WebhookDeserialisationTests()
    {
        emailConfig = new EmailConfig
        {
            WebhookSigningKey = "a",
        };
        mailgunWebhookHandler = new MailgunWebhookHandler(null, null, emailConfig, null, null, null);
    }

    [Fact]
    public void Spam_report_can_be_deserialised()
    {
        var json = """
            {
                "signature":
                {
                    "token":"abcdef",
                    "timestamp":"1738538545",
                    "signature":"634966f04128459dd6c84d579c130258f1347d3a821042208015b199efb5dbbb"
                },
                "event-data":
                {
            	    "id": "id-goes-here",
            	    "timestamp": 1521233123.501324,
            	    "log-level": "warn",
            	    "event": "complained",
            	    "envelope":
                    {
            		    "sending-ip": "173.193.210.33"
            	    },
            	    "flags":
                    {
            		    "is-test-mode": false
            	    },
            	    "message":
                    {
            		    "headers":
                        {
            			    "to": "Alice <alice@example.com>",
            			    "message-id": "20110215055645.25246.63817@example.co.uk",
            			    "from": "Bob <bob@example.com>",
            			    "subject": "Test complained webhook"
            		    },
            		    "attachments": [],
            		    "size": 111
            	    },
            	    "recipient": "alice@example.com",
            	    "campaigns": [],
            	    "tags": ["my_tag_1", "my_tag_2"],
            	    "user-variables":
                    {
            		    "my_var_1": "Value 1",
            		    "my-var-2": "Value 2"
            	    }
                }
            }
        """;

        var request = mailgunWebhookHandler.Deserialise<MailgunSpamReport>(json, "")!;

        Assert.Equal("id-goes-here", request.EventData.Id);
        Assert.True(request.EventData.Message.Headers.ContainsValue("Test complained webhook"));
    }


    [Fact]
    public void Accept_report_can_be_deserialised()
    {
        var json = """
            {
                "signature":
                {
                    "token":"d151bb0196795c8f3a15d1f61aa02123cb3cfd309bc5d7b62e",
                    "timestamp":"1742224637",
                    "signature":"9fef2c10a148dbb3357cc5bc3c8dfb1e53fa3c96acedd54b1a57e38af327872c"
                },
                "event-data":
                {
        	        "event": "accepted",
        	        "id": "id-goes-here",
        	        "timestamp": 1521472262.908181,
        	        "api-key-id": "api-key-here",
        	        "flags":
                    {
        		        "is-authenticated": true,
        		        "is-test-mode": false
        	        },
        	        "envelope":
                    {
        		        "transport": "smtp",
        		        "sender": "bob@mg.example.com",
        		        "targets": "alice@example.com"
        	        },
        	        "message":
                    {
        		        "headers":
                        {
        			        "to": "Alice <alice@example.com>",
        			        "message-id": "20130503182626.18666.16540@example.com",
        			        "from": "Bob <bob@mg.example.com>",
        			        "subject": "Test accepted webhook"
        		        },
        		        "attachments": [],
        		        "size": 256
        	        },
        	        "storage":
                    {
        		        "url": "https://xyz.com",
        		        "key": "message_key"
        	        },
        	        "recipient": "alice@example.com",
        	        "recipient-domain": "example.com",
        	        "method": "HTTP",
        	        "log-level": "info",
        	        "tags": ["my_tag_1", "my_tag_2"],
        	        "user-variables":
                    {
        		        "my_var_1": "Mailgun Variable #1",
        		        "my-var-2": "awesome"
        	        }
                }
            }
        """;

        var request = mailgunWebhookHandler.Deserialise<MailgunAcceptReport>(json, "")!;

        Assert.Equal("id-goes-here", request.EventData.Id);
        Assert.True(request.EventData.Message.Headers.ContainsValue("Test accepted webhook"));
    }
}
