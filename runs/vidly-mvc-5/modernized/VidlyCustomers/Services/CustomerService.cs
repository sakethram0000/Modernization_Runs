using VidlyCustomers.Models;

namespace VidlyCustomers.Services;

public class CustomerService
{
    private readonly CustomerStore _store;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(CustomerStore store, ILogger<CustomerService> logger)
    {
        _store = store;
        _logger = logger;
    }

    public Task<IReadOnlyList<MembershipType>> TypesAsync()
    {
        return Task.FromResult(_store.Types());
    }

    public Task<IReadOnlyList<Customer>> ListAsync(string? query)
    {
        return Task.FromResult(_store.List(query));
    }

    public Task<Customer?> FindAsync(int id)
    {
        return Task.FromResult(_store.Find(id));
    }

    public Task<int> AddAsync(Customer customer)
    {
        var id = _store.Add(customer);
        _logger.LogInformation("Added customer {CustomerId}", id);
        return Task.FromResult(id);
    }

    public Task UpdateAsync(Customer customer)
    {
        try
        {
            _store.Update(customer);
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogError(exception, "Save failed for customer {CustomerId}", customer.Id);
            throw;
        }

        _logger.LogInformation("Updated customer {CustomerId}", customer.Id);
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(int id)
    {
        var deleted = _store.Delete(id);
        _logger.LogInformation("Delete customer {CustomerId} succeeded: {Deleted}", id, deleted);
        return Task.FromResult(deleted);
    }
}
