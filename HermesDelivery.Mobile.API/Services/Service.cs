using AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Services.Sms;
using Serilog;

namespace CourierAPI.Services
{
    public class xService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public xService(ILogger logger, IMapper mapper)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper;
        }
    }
}