using Microsoft.EntityFrameworkCore;
using NorthwindRAG.Infrastructure.Entities;

namespace NorthwindRAG.Infrastructure.Data;

public class NorthwindRepository
{
    private readonly NorthwindDbContext _context;

    public NorthwindRepository(NorthwindDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllProductsAsync() =>
        await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .ToListAsync();

    public async Task<List<Customer>> GetAllCustomersAsync() =>
        await _context.Customers
            .AsNoTracking()
            .ToListAsync();

    public async Task<List<Order>> GetAllOrdersAsync() =>
        await _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .ToListAsync();

    public async Task<List<Employee>> GetAllEmployeesAsync() =>
        await _context.Employees
            .AsNoTracking()
            .ToListAsync();

    public async Task<List<Supplier>> GetAllSuppliersAsync() =>
        await _context.Suppliers
            .AsNoTracking()
            .ToListAsync();
}
