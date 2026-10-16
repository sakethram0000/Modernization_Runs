using Microsoft.AspNetCore.Mvc;
using VidlyCustomers.Models;
using VidlyCustomers.Services;
using VidlyCustomers.ViewModels;

namespace VidlyCustomers.Controllers;

public class CustomersController : Controller
{
    private readonly CustomerService _customers;

    public CustomersController(CustomerService customers)
    {
        _customers = customers;
    }

    public async Task<IActionResult> Index()
    {
        return View();
    }

    public async Task<IActionResult> New()
    {
        var viewModel = new CustomerFormViewModel
        {
            Customer = new Customer(),
            MembershipTypes = await _customers.TypesAsync()
        };
        return View("CustomerForm", viewModel);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        var viewModel = new CustomerFormViewModel
        {
            Customer = customer,
            MembershipTypes = await _customers.TypesAsync()
        };
        return View("CustomerForm", viewModel);
    }

    public async Task<IActionResult> Details(int id)
    {
        var customer = await _customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        return View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Customer customer)
    {
        if (!ModelState.IsValid)
        {
            var viewModel = new CustomerFormViewModel
            {
                Customer = customer,
                MembershipTypes = await _customers.TypesAsync()
            };
            return View("CustomerForm", viewModel);
        }

        if (customer.Id == 0)
        {
            await _customers.AddAsync(customer);
        }
        else
        {
            await _customers.UpdateAsync(customer);
        }

        return RedirectToAction(nameof(Index), "Customers");
    }
}
