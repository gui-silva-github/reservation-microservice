using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Reservation.UsersService.BLL.DTOs;
using Reservation.UsersService.BLL.Exceptions;
using Reservation.UsersService.BLL.Interfaces;
using Reservation.UsersService.DAL.Entities;
using Reservation.UsersService.DAL.Enums;
using Reservation.UsersService.DAL.Repositories;

namespace Reservation.UsersService.BLL.Services {
    internal sealed class AuthService : IAuthService {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IMapper _mapper;
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly IValidator<LoginRequest> _loginValidator;

        public AuthService(
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IMapper mapper,
            IValidator<RegisterRequest> registerValidator,
            IValidator<LoginRequest> loginValidator
        ) {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _mapper = mapper;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
        }

        public async Task<AuthenticationResponse> RegisterAsync(RegisterRequest request) {
            await ValidateAsync(_registerValidator, request);

            User? existingUser = await _userRepository.GetByEmailAsync(request.Email);

            if (existingUser is not null) {
                throw new ConflictException("Email já foi registrado.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.Customer,
                CreatedAt = DateTime.UtcNow
            };

            User createdUser = await _userRepository.CreateAsync(user);
            string token = _jwtTokenService.GenerateToken(createdUser);

            return BuildAuthenticationResponse(createdUser, token);
        }

        public async Task<AuthenticationResponse> LoginAsync(LoginRequest request) {
            await ValidateAsync(_loginValidator, request);

            User? user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());

            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) {
                throw new InvalidCredentialsException();
            }

            string token = _jwtTokenService.GenerateToken(user);

            return BuildAuthenticationResponse(user, token);
        }

        private AuthenticationResponse BuildAuthenticationResponse(User user, string token) {
            AuthenticationResponse response = _mapper.Map<AuthenticationResponse>(user);
            return response with { Success = true, Token = token };
        }

        private static async Task ValidateAsync<T>(IValidator<T> validator, T instance) {
            ValidationResult validationResult = await validator.ValidateAsync(instance);

            if (!validationResult.IsValid) {
                throw new ValidationException(validationResult.Errors);
            }
        }
    }
}