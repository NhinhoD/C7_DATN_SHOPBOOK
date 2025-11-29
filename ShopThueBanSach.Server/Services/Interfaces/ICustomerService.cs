using ShopThueBanSach.Server.Entities;

namespace ShopThueBanSach.Server.Services.Interfaces
{
	public interface ICustomerService
	{
		Task<IEnumerable<User>> GetAllAsync();
		Task<Customer> GetByIdAsync(int id);
		Task<IEnumerable<Customer>> GetByRoleAsync(string role);
		Task<Customer> CreateAsync(Customer customer);
		Task<bool> UpdateAsync(Customer customer);
		Task<bool> DeleteAsync(int id);
		Task<bool> BanAccountAsync(int id);
		Task<bool> UnbanAccountAsync(int id);

	}

}