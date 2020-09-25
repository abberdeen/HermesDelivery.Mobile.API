using AutoMapper;
using HermesDMobAPI.Infrastructure;
using HermesDMobAPI.Models.DTO.Sms;
using System;
using System.Threading.Tasks;

namespace HermesDMobAPI.Services.Sms
{
    public class SmsSendLogService
    {
        private readonly HDEntities _dbContext = new HDEntities();
        private readonly IMapper _mapper;

        public SmsSendLogService(IMapper mapper)
        {
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