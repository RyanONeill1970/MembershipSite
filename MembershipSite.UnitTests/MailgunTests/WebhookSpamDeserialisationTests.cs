namespace MembershipSite.UnitTests.MailgunTests;

using MembershipSite.Logic.Mail.Mailgun;
using MembershipSite.Logic.Mail.Mailgun.Models;
using MembershipSite.ViewModels;

public class WebhookSpamDeserialisationTests
{
    private readonly EmailConfig emailConfig;
    private readonly MailgunWebhookHandler mailgunWebhookHandler;

    public WebhookSpamDeserialisationTests()
    {
        emailConfig = new EmailConfig
        {
            WebhookSigningKey = "a",
        };
        mailgunWebhookHandler = new MailgunWebhookHandler(emailConfig, null);
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

        var request = mailgunWebhookHandler.Deserialise<MailgunRequest>(json, "")!;

        Assert.Equal("id-goes-here", request.EventData.Id);
        Assert.True(request.EventData.Message.Headers.ContainsValue("Test complained webhook"));
    }
}