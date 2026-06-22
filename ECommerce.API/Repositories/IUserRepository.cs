using ECommerce.API.Entities;

namespace ECommerce.API.Repositories
{

    public interface IUserRepository
    {
        Task<User> GetByEmail(string email);

        Task Add(User user);

        Task Save();
    }
}
