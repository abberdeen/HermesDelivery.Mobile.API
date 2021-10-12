using AutoMapper;
using CourierAPI.DTO.Shift;
using CourierAPI.Infrastructure.Database;

namespace CourierAPI.Infrastructure.AutoMapper.Profiles
{
    public class CourierShiftHistoryProfile : Profile
    {
        public CourierShiftHistoryProfile()
        {
            // CourierWorkShiftsItem.
            CreateMap<CourierShiftHistory, CourierShiftHistoryDto>();
        }
    }
}