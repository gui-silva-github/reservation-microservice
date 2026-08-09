using AutoMapper;
using Reservation.SpacesService.BLL.DTO;
using Reservation.SpacesService.DAL.Entities;

namespace Reservation.SpacesService.BLL.Mappers
{
    public class CreateSpaceRequestToSpaceMappingProfile : Profile
    {
        public CreateSpaceRequestToSpaceMappingProfile()
        {
            CreateMap<CreateSpaceRequest, Space>()
                .ForMember(destination => destination.Id, options => options.Ignore())
                .ForMember(destination => destination.IsActive, options => options.MapFrom(_ => true));
        }
    }
}