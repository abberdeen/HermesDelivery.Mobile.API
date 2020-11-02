using AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Services.Account;
using CourierAPI.Services.Sms;
using Serilog;
using System.Web.Mvc;

namespace CourierAPI.Services
{ 
    public class xService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public xService(ILogger logger, IMapper mapper, UserService userService, MessageService messageService)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper;
        }
    }
}