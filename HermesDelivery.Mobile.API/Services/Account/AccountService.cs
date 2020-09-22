using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HermesDelivery.Mobile.API.Infrastructure;

namespace HermesDelivery.Mobile.API.Services.OAuth
{
    public class AccountService
    {
        private HDEntities _dbContext;
        private readonly IMapper _mapper;

        public AccountService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public AccountService()
        {
        }
         
        public async Task<AspNetUser> GetUserByNameAsync(string userName)
        {
            return await _dbContext.AspNetUsers.Where(e => e.UserName == userName).FirstOrDefaultAsync(); 
        }

        public async Task<AspNetUser> GetUserByIdAsync(string id)
        {
            return await _dbContext.AspNetUsers.Where(e => e.Id == id).FirstOrDefaultAsync();
        }

    }
}