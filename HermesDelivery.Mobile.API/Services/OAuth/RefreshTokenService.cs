using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HermesDelivery.Mobile.API.Infrastructure;

namespace HermesDelivery.Mobile.API.Services.OAuth
{
    public class RefreshTokenService
    {
        private HDEntities _dbContext;
        private readonly IMapper _mapper;

        public RefreshTokenService(  IMapper mapper )
        {
            _mapper = mapper;
            _dbContext = new HDEntities();
        }

        public RefreshTokenService()
        {
        }

        public async Task<AspNetUser> GetByUserIdAsync(string id)
        {
            return await _dbContext.AspNetUsers.Where(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async Task SetAsync(string token, DateTime tokenExpires, string userId)
        {
            var user = await _dbContext.AspNetUsers.FindAsync(userId);

            if (user != null)
            {
                user.RefreshToken = token;
                user.TokenExpires = tokenExpires;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task ClearAsync(string userId)
        {
            var user = await _dbContext.AspNetUsers.FindAsync(userId);

            if (user != null)
            {
                user.RefreshToken = null;
                user.TokenExpires = null;
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
