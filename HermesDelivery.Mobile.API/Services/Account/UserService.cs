using AutoMapper;
using HermesDMobAPI.Infrastructure;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace HermesDMobAPI.Services.Account
{
    public class UserService
    {
        private HDEntities _dbContext = new HDEntities();
        private readonly IMapper _mapper;

        public UserService(IMapper mapper)
        {
            _mapper = mapper;
        } 

        public async Task<AspNetUser> GetUserByNameAsync(string userName)
        {
            return await _dbContext.AspNetUsers.Where(e => e.UserName == userName).FirstOrDefaultAsync();
        }

        public async Task<AspNetUser> GetUserByIdAsync(string id)
        {
            return await _dbContext.AspNetUsers.Where(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<string>> GetUserRolesByIdAsync(string id)
        {
            return await _dbContext.AspNetUserRoles.Where(e => e.UserId == id).Select(e => e.AspNetRole.Name).ToListAsync();
        }
    }
}