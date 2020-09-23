using AutoMapper;
using HermesDelivery.Mobile.API.Infrastructure;
using HermesDelivery.Mobile.API.Models.DTO.OAuth;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace HermesDelivery.Mobile.API.Services.OAuth
{
    public class RefreshTokenService
    {
        private HDEntities _dbContext = new HDEntities();
        private readonly IMapper _mapper;

        public RefreshTokenService(IMapper mapper)
        {
            _mapper = mapper;
            _dbContext = new HDEntities();
        }

        public RefreshTokenService()
        {
        }

        public async Task<AspNetUser> GetByUserByIdAsync(string id)
        {
            return await _dbContext.AspNetUsers.Where(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async Task SetAsync(RefreshTokenDTO model, string userId)
        {
            var user = await _dbContext.AspNetUsers.FindAsync(userId);

            if (user != null)
            {
                user.RefreshToken = model.Token;
                user.RefreshTokenIp = model.RemoteIp;
                user.RefreshTokenExpires = model.Expires;
                user.RefreshTokenIsActive = model.IsActive;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task ClearAsync(string userId)
        {
            var user = await _dbContext.AspNetUsers.FindAsync(userId);

            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenIp = null;
                user.RefreshTokenExpires = null;
                user.RefreshTokenIsActive = null;
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}