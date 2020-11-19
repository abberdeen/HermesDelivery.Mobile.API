using AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Models.DTO.Shift;

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