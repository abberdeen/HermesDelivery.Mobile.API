using AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Models.DTO.WorkShifts;

namespace CourierAPI.Infrastructure.AutoMapper.Profiles
{
    public class CourierWorkShiftsScheduleProfile : Profile
    {
        public CourierWorkShiftsScheduleProfile()
        {
            // CourierWorkShiftsSchedule.
            CreateMap<CourierWorkShiftsSchedule, CourierWorkShiftsCurrentActiveScheduleDto>();
        }
    }
}