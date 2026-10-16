using VidlyCustomers.Models;

namespace VidlyCustomers.ViewModels;

public class CustomerFormViewModel
{
    public IEnumerable<MembershipType> MembershipTypes { get; set; } = [];

    public Customer Customer { get; set; } = new();
}
