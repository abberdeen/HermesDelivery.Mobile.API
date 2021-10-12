using AutoMapper;
using CourierAPI.DTO.Orders;
using CourierAPI.DTO.Supplier;
using CourierAPI.Infrastructure.Database;

namespace CourierAPI.Infrastructure.AutoMapper.Profiles
{
    public class OrderInfoProfile : Profile
    {
        public OrderInfoProfile()
        {
            // OrderInfo.
            CreateMap<OrderSupplierItemDto, ProductDto>(); 
        }
    }
}