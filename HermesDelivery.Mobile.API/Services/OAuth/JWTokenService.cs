using AutoMapper;
using CourierAPI.Infrastructure.Database;
using Serilog;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace CourierAPI.Services.OAuth
{
    public class JwTokenService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public JwTokenService(ILogger logger, IMapper mapper)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<string> GetTokenAsync(int courierId)
        {
            var courier = await _dbContext.CourierOAuthDatas.Where(e => e.CourierId == courierId).FirstOrDefaultAsync();

            return courier?.JWToken;
        }

        public async Task<int?> GetCourierIdAsync(string jwToken)
        {
            var courier = await _dbContext.CourierOAuthDatas.Where(e => e.JWToken == jwToken).FirstOrDefaultAsync();

            return courier?.CourierId;
        }

        public async Task SetAsync(int courierId, string jwToken)
        {
            var courierOAuthData = await _dbContext.CourierOAuthDatas.FirstOrDefaultAsync(x => x.CourierId == courierId);

            if (courierOAuthData != null)
            {
                courierOAuthData.JWToken = jwToken;
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                _dbContext.CourierOAuthDatas.Add(new CourierOAuthData
                {
                    CourierId = courierId,
                    JWToken = jwToken
                });
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