using AutoMapper;
using HermesDMobAPI.Infrastructure.Database;
using HermesDMobAPI.Models.DTO.Sms;
using System;
using System.Threading.Tasks;
using Serilog;

namespace HermesDMobAPI.Services.Sms
{
    public class SmsSendLogService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public SmsSendLogService(ILogger logger, IMapper mapper)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper;
        }

        public async Task Create(SMSSendLogCreateDto model)
        {
            //Map and create log
            var log = _mapper.Map<SMSSendLog>(model);
            log.CreatedAt = DateTime.Now;
            log.CreatedBy = "42cde713-2368-41fa-b158-a452fc2cfda6";
            _dbContext.SMSSendLogs.Add(log);

            await _dbContext.SaveChangesAsync();
        }
    }
}