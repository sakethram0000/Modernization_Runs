using System.ComponentModel.DataAnnotations;

namespace VidlyCustomers.Dtos;

public class MembershipTypeDto
{
    public byte Id { get; set; }

    public string Name { get; set; } = "";
}

public class CustomerDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "The Name field is required.")]
    [StringLength(255, ErrorMessage = "The field Name must be a string or array type with a maximum length of '255'.")]
    public string Name { get; set; } = "";

    public bool IsSubscribedToNewsletter { get; set; }

    public byte MembershipTypeId { get; set; }

    public MembershipTypeDto? MembershipType { get; set; }

    public DateTime? Birthdate { get; set; }
}
