namespace MembershipSite.ViewModels;

using System;

public class MemberSummaryRow
{
    public string Email { get; set; }
    public string MemberNumber { get; set; }
    public string Name { get; set; }
    public bool IsApproved { get; set; }
    public DateTimeOffset DateRegistered { get; set; }
}
