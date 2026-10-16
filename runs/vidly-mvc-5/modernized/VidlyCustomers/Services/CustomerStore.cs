using Microsoft.Extensions.Options;
using VidlyCustomers.Dtos;
using VidlyCustomers.Models;

namespace VidlyCustomers.Services;

public class CustomerStore
{
    private readonly object _gate = new();
    private readonly List<MembershipType> _types;
    private readonly List<Customer> _customers = [];

    public CustomerStore(IOptions<MembershipCatalog> catalog)
    {
        _types = catalog.Value.Types.Select(CopyType).ToList();
        Reset();
    }

    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _customers.Count;
            }
        }
    }

    public void Reset()
    {
        lock (_gate)
        {
            _customers.Clear();
            // Data source: preview data is fake. These two people are not in the legacy database.
            _customers.Add(new Customer
            {
                Id = 1,
                Name = "John Smith",
                MembershipTypeId = 2,
                Birthdate = new DateTime(1980, 1, 1),
                IsSubscribedToNewsletter = true
            });
            _customers.Add(new Customer
            {
                Id = 2,
                Name = "Mary Williams",
                MembershipTypeId = 1,
                Birthdate = null,
                IsSubscribedToNewsletter = false
            });
        }
    }

    public IReadOnlyList<MembershipType> Types()
    {
        lock (_gate)
        {
            return _types.Select(CopyType).ToList();
        }
    }

    public IReadOnlyList<Customer> List(string? query)
    {
        lock (_gate)
        {
            IEnumerable<Customer> rows = _customers;
            if (!string.IsNullOrWhiteSpace(query))
            {
                rows = rows.Where(customer => customer.Name.Contains(query));
            }

            return rows.Select(Copy).ToList();
        }
    }

    public Customer? Find(int id)
    {
        lock (_gate)
        {
            var customer = _customers.SingleOrDefault(item => item.Id == id);
            return customer == null ? null : Copy(customer);
        }
    }

    public int Add(Customer customer)
    {
        lock (_gate)
        {
            customer.Id = _customers.Count == 0 ? 1 : _customers.Max(item => item.Id) + 1;
            _customers.Add(Copy(customer));
            return customer.Id;
        }
    }

    public void Update(Customer posted)
    {
        lock (_gate)
        {
            var stored = _customers.Single(item => item.Id == posted.Id);
            stored.Name = posted.Name;
            stored.Birthdate = posted.Birthdate;
            stored.MembershipTypeId = posted.MembershipTypeId;
            stored.IsSubscribedToNewsletter = posted.IsSubscribedToNewsletter;
        }
    }

    public bool Delete(int id)
    {
        lock (_gate)
        {
            var stored = _customers.SingleOrDefault(item => item.Id == id);
            if (stored == null)
            {
                return false;
            }

            _customers.Remove(stored);
            return true;
        }
    }

    public CustomerDto ToDto(Customer customer, bool includeMembership)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            IsSubscribedToNewsletter = customer.IsSubscribedToNewsletter,
            MembershipTypeId = customer.MembershipTypeId,
            Birthdate = customer.Birthdate,
            MembershipType = includeMembership && customer.MembershipType != null
                ? new MembershipTypeDto
                {
                    Id = customer.MembershipType.Id,
                    Name = customer.MembershipType.Name
                }
                : null
        };
    }

    private Customer Copy(Customer customer)
    {
        var type = _types.SingleOrDefault(item => item.Id == customer.MembershipTypeId);
        return new Customer
        {
            Id = customer.Id,
            Name = customer.Name,
            IsSubscribedToNewsletter = customer.IsSubscribedToNewsletter,
            MembershipTypeId = customer.MembershipTypeId,
            Birthdate = customer.Birthdate,
            MembershipType = type == null ? null : CopyType(type)
        };
    }

    private static MembershipType CopyType(MembershipType type)
    {
        return new MembershipType
        {
            Id = type.Id,
            Name = type.Name,
            SignUpFee = type.SignUpFee,
            DurationInMonths = type.DurationInMonths,
            DiscountRate = type.DiscountRate
        };
    }
}
