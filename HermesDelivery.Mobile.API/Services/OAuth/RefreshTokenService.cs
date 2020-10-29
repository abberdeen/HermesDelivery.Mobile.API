using AutoMapper;
using HermesDMobAPI.Infrastructure.Database;
using HermesDMobAPI.Models.DTO.OAuth;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace HermesDMobAPI.Services.OAuth
{
    public class RefreshTokenService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public RefreshTokenService(ILogger logger, IMapper mapper)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<AspNetUser> GetByUserByIdAsync(string id)
        {
            return await _dbContext.AspNetUsers.Where(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async Task SetAsync(RefreshTokenDto model, string userId)
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