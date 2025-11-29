using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Services
{
	public class CustomerService : ICustomerService
	{
		private readonly AppDBContext _context;
		private readonly UserManager<User> _userManager;

		public CustomerService(AppDBContext context, UserManager<User> userManager)
		{
			_userManager = userManager;
			_context = context;
		}

		public async Task<IEnumerable<User>> GetAllAsync()
		{
			return await _userManager.Users.ToListAsync();
		}

		public async Task<Customer> GetByIdAsync(int id)
		{
			return await _context.Customers.FindAsync(id);
		}

		public async Task<IEnumerable<Customer>> GetByRoleAsync(string role)
		{
			return await _context.Customers.Where(c => c.Role == role).ToListAsync();
		}

		public async Task<Customer> CreateAsync(Customer customer)
		{
			_context.Customers.Add(customer);
			await _context.SaveChangesAsync();
			return customer;
		}

		public async Task<bool> UpdateAsync(Customer customer)
		{
			var existing = await _context.Customers.FindAsync(customer.CustomerId);
			if (existing == null) return false;

			existing.FullName = customer.FullName;
			existing.Role = customer.Role;
			existing.Email = customer.Email;
			existing.Password = customer.Password;
			existing.PhoneNumber = customer.PhoneNumber;
			existing.Address = customer.Address;
			existing.BirthDate = customer.BirthDate;
			existing.LoyaltyPoints = customer.LoyaltyPoints;

			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> DeleteAsync(int id)
		{
			var customer = await _context.Customers.FindAsync(id);
			if (customer == null) return false;

			_context.Customers.Remove(customer);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> BanAccountAsync(int id)
		{
			var customer = await _context.Customers.FindAsync(id);
			if (customer == null) return false;

			customer.IsBanned = true;
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> UnbanAccountAsync(int id)
		{
			var customer = await _context.Customers.FindAsync(id);
			if (customer == null) return false;

			customer.IsBanned = false;
			await _context.SaveChangesAsync();
			return true;
		}


	}
}