using AutoMapper;
using HermesDMobAPI.Infrastructure.Database;
using HermesDMobAPI.Services.Account;
using HermesDMobAPI.Services.Sms;
using Serilog;
using System.Web.Mvc;

namespace HermesDMobAPI.Services
{
    [Authorize]
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