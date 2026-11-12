using HyperInsurance.Data;
using HyperInsurance.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HyperInsurance.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CustomersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerResponse>> CreateCustomer(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            YearsLicensed = request.YearsLicensed,
            PreviousAccidents = request.PreviousAccidents,
            TrafficViolations = request.TrafficViolations
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetCustomerById),
            new { id = customer.Id },
            ToResponse(customer));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerResponse>>> GetCustomers(
        CancellationToken cancellationToken)
    {
        var customers = await _context.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.Id)
            .Select(customer => new CustomerResponse(
                customer.Id,
                customer.FirstName,
                customer.LastName,
                customer.Email,
                customer.YearsLicensed,
                customer.PreviousAccidents,
                customer.TrafficViolations))
            .ToListAsync(cancellationToken);

        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerResponse>> GetCustomerById(
        int id,
        CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (customer is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Customer not found",
                detail: "No customer exists with the requested ID.");
        }

        return Ok(ToResponse(customer));
    }

    private static CustomerResponse ToResponse(Customer customer)
    {
        return new CustomerResponse(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.YearsLicensed,
            customer.PreviousAccidents,
            customer.TrafficViolations);
    }
}
