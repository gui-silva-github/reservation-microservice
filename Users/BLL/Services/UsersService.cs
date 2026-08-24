using AutoMapper;
using Reservation.UsersService.BLL.DTOs;
using Reservation.UsersService.BLL.Exceptions;
using Reservation.UsersService.BLL.Interfaces;
using Reservation.UsersService.DAL.Entities;
using Reservation.UsersService.DAL.Repositories;

namespace Reservation.UsersService.BLL.Services {
    internal sealed class UserService : IUserService {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper) {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserResponse> GetByIdAsync(Guid id) {
            User? user = await _userRepository.GetByIdAsync(id);

            if (user is null) {
                throw new NotFoundException($"Usuário com id '${id}' não foi encontrado.");
            }

            return _mapper.Map<UserResponse>(user);
        }
    }
}