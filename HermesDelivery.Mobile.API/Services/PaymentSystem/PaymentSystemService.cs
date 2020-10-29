using AutoMapper;
using HermesDMobAPI.Infrastructure;
using HermesDMobAPI.Infrastructure.Database;
using HermesDMobAPI.Infrastructure.Exceptions;
using HermesDMobAPI.Models.DTO.PaymentSystems;
using HermesDMobAPI.Services.Account;
using HermesDMobAPI.Services.Sms;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HermesDMobAPI.Services.PaymentSystem
{
    [Authorize]
    public class PaymentSystemService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public PaymentSystemService(ILogger logger, 
            IMapper mapper, 
            UserService userService,
            MessageService messageService)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PaymentSystemDto>> List()
        {
            var items= 
                _dbContext.PaymentSystems
                .Where(x => x.IsActive == true)
                .OrderBy(x=>x.OrderId)
                .ToList();

            return _mapper.Map<IEnumerable<PaymentSystemDto>>(items);
        }
    }
}