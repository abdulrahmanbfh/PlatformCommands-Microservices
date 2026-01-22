using AutoMapper;
using PlatformService.Dtos;
using PlatformService.Models;

namespace PlatformService.Profiles;

public class PlatformsProfile : Profile
{
    public PlatformsProfile()
    {
        CreateMap<Platform, PlatformReadDto>();
        CreateMap<PlatformCreateDto, Platform>();
        CreateMap<PlatformReadDto, PlatformPublishedDto>();
        CreateMap<Platform, GrpcPlatformModel>()
        .ForMember(dist => dist.PlatformId, 
            opt => opt.MapFrom(src => src.Id))
        .ForMember(dist => dist.Name, 
            opt => opt.MapFrom(src => src.Name))
        .ForMember(dist => dist.Publisher,
            opt => opt.MapFrom(src => src.Publisher));
    }
}