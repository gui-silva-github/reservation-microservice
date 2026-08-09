using AutoMapper;
using Reservation.SpacesService.BLL.DTO;
using Reservation.SpacesService.DAL.Entities;

namespace Reservation.SpacesService.BLL.Mappers
{
    public class SpaceToSpaceResponseMappingProfile : Profile
    {
        public SpaceToSpaceResponseMappingProfile()
        {
            CreateMap<Space, SpaceResponse>();
        }
    }
}