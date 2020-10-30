using AutoMapper;
using CourierAPI.Infrastructure.Database;
using Serilog;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace CourierAPI.Services.Account
{
    public class UserService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public UserService(ILogger logger, IMapper mapper)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
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