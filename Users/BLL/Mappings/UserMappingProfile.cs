using AutoMapper;
using Reservation.UsersService.BLL.DTOs;
using Reservation.UsersService.DAL.Entities;

namespace Reservation.UsersService.BLL.Mappings {
    public class UserMappingProfile : Profile {
        public UserMappingProfile() {
            CreateMap<User, UserResponse>();

            CreateMap<User, AuthenticationResponse>()
                .ForMember(destination => destination.Success, options => options.MapFrom(_ => false))
                .ForMember(destination => destination.Token, options => options.MapFrom(_ => string.Empty))
                .ForMember(destination => destination.UserId, options => options.MapFrom(source => source.Id));
        }   
    }
}