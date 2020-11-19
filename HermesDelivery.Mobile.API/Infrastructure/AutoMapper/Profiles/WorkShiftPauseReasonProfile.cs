using AutoMapper;
using CourierAPI.DTO.Shift;
using CourierAPI.Infrastructure.Database;

namespace CourierAPI.Infrastructure.AutoMapper.Profiles
{
    public class WorkShiftPauseReasonProfile : Profile
    {
        public WorkShiftPauseReasonProfile()
        {
            // WorkShiftPauseReason.
            CreateMap<ShiftPauseReason, ShiftPauseReasonDto>();
        }
    }
}