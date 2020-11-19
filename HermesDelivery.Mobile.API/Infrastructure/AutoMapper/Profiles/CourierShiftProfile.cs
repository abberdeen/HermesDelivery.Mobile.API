using AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Models.DTO.Shift;

namespace CourierAPI.Infrastructure.AutoMapper.Profiles
{
    public class CourierShiftProfile : Profile
    {
        public CourierShiftProfile()
        {
            // CourierWorkShiftsSchedule.
            CreateMap<CourierShift, CurrentActiveCourierShiftDto>();
        }
    }
}