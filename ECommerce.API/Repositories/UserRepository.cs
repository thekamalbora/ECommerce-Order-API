using ECommerce.API.Data;
using ECommerce.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;

        public UserRepository(
            ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<User> GetByEmail(
            string email)
        {
            return await _db.Users
                .FirstOrDefaultAsync(
                    x =>
                    x.Email == email);
        }

        public async Task Add(
            User user)
        {
            await _db.Users.AddAsync(user);
        }

        public async Task Save()
        {
            await _db.SaveChangesAsync();
        }
    }
}
