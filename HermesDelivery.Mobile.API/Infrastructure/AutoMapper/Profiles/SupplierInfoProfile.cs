using AutoMapper;
using CourierAPI.DTO.Supplier;

namespace CourierAPI.Infrastructure.AutoMapper.Profiles
{
    public class SupplierInfoProfile : Profile
    {
        public SupplierInfoProfile()
        {
            // SupplierInfo.
            CreateMap<SupplierInfoDto, IncomingOrderInfoSupplierDto>();
            CreateMap<SupplierInfoDto, IncomingOrderDetailsSupplierDto>();
        }
    }
}