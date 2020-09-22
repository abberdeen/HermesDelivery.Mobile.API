using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HermesDelivery.Mobile.API.Infrastructure;

namespace HermesDelivery.Mobile.API.Services.OAuth
{
    public class JWTokenService
    {
        private HDEntities _dbContext;
        private readonly IMapper _mapper;

        public JWTokenService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public JWTokenService()
        {
        }

        public async Task<string> GetTokenAsync(string userId)
        {
            var user = await _dbContext.AspNetUsers.Where(e => e.Id == userId).FirstOrDefaultAsync();

            return user?.JWToken;
        }

        public async Task<string> GetUserIdAsync(string jwToken)
        {
            var user = await _dbContext.AspNetUsers.Where(e => e.JWToken == jwToken).FirstOrDefaultAsync();

            return user?.Id;
        }

        public async Task SetAsync(string userId, string jwToken)
        {
            var user = await _dbContext.AspNetUsers.FindAsync(userId);

            if (user != null)
            {
                user.JWToken = jwToken;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task ClearAsync(string userId)
        {
            var user = await _dbContext.AspNetUsers.FindAsync(userId);

            if (user != null)
            {
                user.JWToken = null;
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}