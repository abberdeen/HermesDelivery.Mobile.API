using AutoMapper;
using HermesDMobAPI.Infrastructure;
using HermesDMobAPI.Infrastructure.Database;
using HermesDMobAPI.Infrastructure.Exceptions;
using HermesDMobAPI.Models.DTO.PaymentSystems;
using HermesDMobAPI.Services.Account;
using HermesDMobAPI.Services.Sms;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HermesDMobAPI.Services.PaymentSystem
{
    [Authorize]
    public class PaymentSystemService
    {
        private readonly DatabaseContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public PaymentSystemService(ILogger logger, 
            IMapper mapper, 
            UserService userService,
            MessageService messageService)
        {
            _dbContext = new DatabaseContext();
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PaymentSystemDto>> List()
        {
            throw new AppException(AppMessage.InvalidPassword);
            return new List<PaymentSystemDto>();
        }
    }
}