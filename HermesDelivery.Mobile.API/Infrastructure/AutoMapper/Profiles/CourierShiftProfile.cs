using AutoMapper;
using CourierAPI.DTO.Shift;
using CourierAPI.Infrastructure.Database;

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