using AutoMapper;
using Reservation.UsersService.BLL.DTOs;
using Reservation.UsersService.DAL.Entities;

namespace Reservation.UsersService.BLL.Mappings {
    public class UserMappingProfile : Profile {
        public UserMappingProfile() {
            CreateMap<User, UserResponse>()
                .ConstructUsing(source => new UserResponse(
                    source.Id,
                    source.Name,
                    source.Email,
                    source.Role,
                    source.CreatedAt
                ));
        }   
    }
}