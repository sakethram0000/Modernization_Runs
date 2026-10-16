using System.ComponentModel.DataAnnotations;

namespace VidlyCustomers.Models;

public class Customer
{
    public int Id { get; set; }

    [Required(ErrorMessage = "The Name field is required.")]
    [StringLength(255, ErrorMessage = "The field Name must be a string or array type with a maximum length of '255'.")]
    public string Name { get; set; } = "";

    public bool IsSubscribedToNewsletter { get; set; }

    public MembershipType? MembershipType { get; set; }

    [Display(Name = "Membership Type")]
    public byte MembershipTypeId { get; set; }

    [Display(Name = "Date of Birth")]
    [Min18YearsIfAMember]
    public DateTime? Birthdate { get; set; }
}
