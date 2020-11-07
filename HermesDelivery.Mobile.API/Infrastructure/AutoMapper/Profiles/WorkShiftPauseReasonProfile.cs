using AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Models.DTO.WorkShifts;

namespace CourierAPI.Infrastructure.AutoMapper.Profiles
{
    public class WorkShiftPauseReasonProfile : Profile
    {
        public WorkShiftPauseReasonProfile()
        {
            // WorkShiftPauseReason.
            CreateMap<WorkShiftPauseReason, WorkShiftPauseReasonDto>();
        }
    }
}