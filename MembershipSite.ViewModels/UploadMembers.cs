namespace MembershipSite.ViewModels;

public class UploadMembers
{
    [Required(ErrorMessage = "Please upload a CSV file")]
    [Display(Name = "Upload File")]
    public IFormFile? File { get; set; }
}
