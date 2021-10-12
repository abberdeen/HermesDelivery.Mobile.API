using AutoMapper;
using CourierAPI.DTO.Shift;
using CourierAPI.Infrastructure.Database;

namespace CourierAPI.Infrastructure.AutoMapper.Profiles
{
    public class ShiftPauseReasonProfile : Profile
    {
        public ShiftPauseReasonProfile()
        {
            // WorkShiftPauseReason.
            CreateMap<ShiftPauseReason, ShiftPauseReasonDto>();
        }
    }
}