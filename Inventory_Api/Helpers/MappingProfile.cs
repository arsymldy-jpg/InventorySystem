using AutoMapper;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Models.Entities;

namespace Inventory_Api.Helpers // یا نام فضای نام دلخواه شما
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // مپ‌های خود را اینجا اضافه کنید
            // مثال:
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<Brand, BrandDto>().ReverseMap();
            CreateMap<Warehouse, WarehouseDto>().ReverseMap();
            CreateMap<Stock, StockDto>().ReverseMap();
            CreateMap<CostCenter, CostCenterDto>().ReverseMap();
            CreateMap<CreateCostCenterDto, CostCenter>().ReverseMap();
            CreateMap<UpdateCostCenterDto, CostCenter>().ReverseMap();
            CreateMap<InventoryTransaction, InventoryTransactionDto>().ReverseMap();
            CreateMap<CreateInventoryTransactionDto, InventoryTransaction>().ReverseMap();
            // سایر مپ‌ها...
        }
    }
}