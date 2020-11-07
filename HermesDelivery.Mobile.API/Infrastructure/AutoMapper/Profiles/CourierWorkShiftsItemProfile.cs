using AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Models.DTO.WorkShifts;

namespace CourierAPI.Infrastructure.AutoMapper.Profiles
{
    public class CourierWorkShiftsItemProfile : Profile
    {
        public CourierWorkShiftsItemProfile()
        {
            // CourierWorkShiftsItem.
            CreateMap<CourierWorkShiftsItem, CourierWorkShiftsItemDto>();
        }
    }
}