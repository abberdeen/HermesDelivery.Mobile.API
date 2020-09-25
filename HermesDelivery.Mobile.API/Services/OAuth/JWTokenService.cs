using AutoMapper;
using HermesDMobAPI.Infrastructure;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace HermesDMobAPI.Services.OAuth
{
    public class JwTokenService
    {
        private HDEntities _dbContext = new HDEntities();
        private readonly IMapper _mapper;

        public JwTokenService(IMapper mapper)
        {
            _mapper = mapper;
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