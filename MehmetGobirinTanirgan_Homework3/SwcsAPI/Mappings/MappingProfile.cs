using AutoMapper;
using Data.DataModels;
using SwcsAPI.Dtos;

namespace SwcsAPI.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ContainerCreateDto, Container>();
            CreateMap<ContainerUpdateDto, Container>();
            CreateMap<Container, ContainerDefaultResponseDto>();

            CreateMap<VehicleCreateDto, Vehicle>();
            CreateMap<VehicleUpdateDto, Vehicle>();
            CreateMap<Vehicle, VehicleDefaultResponseDto>();
        }
    }
}
