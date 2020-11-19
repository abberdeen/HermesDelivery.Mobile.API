using AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Services.Sms;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourierAPI.DTO.PaymentSystem;

namespace CourierAPI.Services.PaymentSystem
{
    public class PaymentSystemService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public PaymentSystemService(
            ILogger logger,
            IMapper mapper)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PaymentSystemDto>> List()
        {
            var items =
                _dbContext.PaymentSystems
                .Where(x => x.IsActive == true)
                .OrderBy(x => x.OrderId)
                .ToList();

            return _mapper.Map<IEnumerable<PaymentSystemDto>>(items);
        }
    }
}