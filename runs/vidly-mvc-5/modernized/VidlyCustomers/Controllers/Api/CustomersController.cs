using Microsoft.AspNetCore.Mvc;
using VidlyCustomers.Dtos;
using VidlyCustomers.Models;
using VidlyCustomers.Services;

namespace VidlyCustomers.Controllers.Api;

[Route("api/customers")]
[ApiController]
public class CustomersController : ControllerBase
{
    private readonly CustomerService _customers;
    private readonly CustomerStore _store;

    public CustomersController(CustomerService customers, CustomerStore store)
    {
        _customers = customers;
        _store = store;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomers(string? query = null)
    {
        var customers = await _customers.ListAsync(query);
        var dtos = customers.Select(customer => _store.ToDto(customer, includeMembership: true));
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCustomer(int id)
    {
        var customer = await _customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        return Ok(_store.ToDto(customer, includeMembership: false));
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer(CustomerDto customerDto)
    {
        var customer = new Customer
        {
            Name = customerDto.Name,
            IsSubscribedToNewsletter = customerDto.IsSubscribedToNewsletter,
            MembershipTypeId = customerDto.MembershipTypeId,
            Birthdate = customerDto.Birthdate
        };
        customerDto.Id = await _customers.AddAsync(customer);
        return Created($"/api/customers/{customerDto.Id}", customerDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCustomer(int id, CustomerDto customerDto)
    {
        var customer = await _customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        customer.Name = customerDto.Name;
        customer.Birthdate = customerDto.Birthdate;
        customer.MembershipTypeId = customerDto.MembershipTypeId;
        customer.IsSubscribedToNewsletter = customerDto.IsSubscribedToNewsletter;
        await _customers.UpdateAsync(customer);
        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var deleted = await _customers.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return Ok();
    }
}
