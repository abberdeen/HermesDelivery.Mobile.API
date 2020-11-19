using AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Models.DTO.Shift;

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