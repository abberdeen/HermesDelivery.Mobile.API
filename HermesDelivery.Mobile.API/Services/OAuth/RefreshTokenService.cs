using AutoMapper;
using CourierAPI.Infrastructure.Database;
using Serilog;
using System.Data.Entity;
using System.Threading.Tasks;
using CourierAPI.DTO.OAuth;

namespace CourierAPI.Services.OAuth
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

        public async Task<CourierOAuthData> GetByCourierAuthDataByIdAsync(int courierId)
        {
            return await _dbContext.CourierOAuthDatas.FirstOrDefaultAsync(e => e.CourierId == courierId);
        }

        public async Task SetAsync(RefreshTokenDto model, int courierId)
        {
            var courierOAuthData = await _dbContext.CourierOAuthDatas.FirstOrDefaultAsync(x => x.CourierId == courierId);

            if (courierOAuthData != null)
            {
                courierOAuthData.RefreshToken = model.Token;
                courierOAuthData.RefreshTokenIp = model.RemoteIp;
                courierOAuthData.RefreshTokenExpires = model.Expires;
                courierOAuthData.RefreshTokenIsActive = model.IsActive;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task ClearAsync(int courierId)
        {
            var courierOAuthData = await _dbContext.CourierOAuthDatas.FirstOrDefaultAsync(x => x.CourierId == courierId);

            if (courierOAuthData != null)
            {
                _dbContext.CourierOAuthDatas.Remove(courierOAuthData);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}