using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Reservation.SpacesService.BLL.DTO;
using Reservation.SpacesService.BLL.Exceptions;
using Reservation.SpacesService.BLL.HttpClients;
using Reservation.SpacesService.DAL.Entities;
using Reservation.SpacesService.DAL.Repositories;

namespace Reservation.SpacesService.BLL.Services
{
    public class SpaceService : ISpaceService
    {
        private const string ConfirmedBookingStatus = "Confirmed";

        private readonly ISpaceRepository _spaceRepository;
        private readonly IBookingsHttpClient _bookingsHttpClient;
        private readonly IValidator<CreateSpaceRequest> _createValidator;
        private readonly IValidator<UpdateSpaceRequest> _updateValidator;
        private readonly IMapper _mapper;
        private readonly ILogger<SpaceService> _logger;

        public SpaceService(
            ISpaceRepository spaceRepository,
            IBookingsHttpClient bookingsHttpClient,
            IValidator<CreateSpaceRequest> createValidator,
            IValidator<UpdateSpaceRequest> updateValidator,
            IMapper mapper,
            ILogger<SpaceService> logger
        )
        {
            _spaceRepository = spaceRepository;
            _bookingsHttpClient = bookingsHttpClient;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IReadOnlyList<SpaceResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Space> spaces = await _spaceRepository.GetAllActiveAsync(cancellationToken);
            return _mapper.Map<IReadOnlyList<SpaceResponse>>(spaces);
        }

        public async Task<SpaceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Space? space = await _spaceRepository.GetByIdAsync(id, cancellationToken);

            if (space is null)
            {
                throw new NotFoundException($"Espaço com ID '{id}' não foi encontrado.");
            }

            return _mapper.Map<SpaceResponse>(space);
        }

        public async Task<IReadOnlyList<SpaceResponse>> SearchAsync(string term, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                throw new BusinessException("O termo de busca é obrigatório.");
            }

            IReadOnlyList<Space> spaces = await _spaceRepository.SearchActiveAsync(term, cancellationToken);
            return _mapper.Map<IReadOnlyList<SpaceResponse>>(spaces);
        }

        public async Task<AvailabilityResponse> CheckAvailabilityAsync(
            Guid id,
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default
        )
        {
            ValidateAvailabilityWindow(from, to);

            Space? space = await _spaceRepository.GetByIdAsync(id, cancellationToken);

            if (space is null)
            {
                throw new NotFoundException($"Espaço com ID '{id}' não foi encontrado.");
            }

            if (!space.IsActive)
            {
                throw new BusinessException("O espaço informado não está ativo.");
            }

            DateTime fromUtc = NormalizeToUtc(from);
            DateTime toUtc = NormalizeToUtc(to);

            _logger.LogInformation(
                "Verificando disponibilidade do espaço {SpaceId} entre {From} e {To}.",
                id,
                fromUtc,
                toUtc
            );

            IReadOnlyList<BookingResponse> bookings =
                await _bookingsHttpClient.GetBookingsBySpaceIdAsync(id, cancellationToken);

            List<BookingConflictResponse> conflicts = bookings
                .Where(booking => isConfirmedBooking(booking.Status))
                .Where(booking => HasDateOverlap(booking.StartDate, booking.EndDate, fromUtc, toUtc))
                .Select(booking => new BookingConflictResponse(
                    booking.Id,
                    booking.StartDate,
                    booking.EndDate,
                    booking.Status
                )).ToList();

            return new AvailabilityResponse(
                space.Id,
                space.Name,
                fromUtc,
                toUtc,
                conflicts.Count == 0,
                conflicts
            );
        }

        public async Task<SpaceResponse> CreateAsync(CreateSpaceRequest request, CancellationToken cancellationToken = default)
        {
            await ValidateAsync(_createValidator, request);

            Space space = _mapper.Map<Space>(request);
            space.Id = Guid.NewGuid();

            Space createdSpace = await _spaceRepository.AddAsync(space, cancellationToken);

            _logger.LogInformation("Espaço {SpaceId} criado com sucesso.", createdSpace.Id);

            return _mapper.Map<SpaceResponse>(createdSpace);
        }

        public async Task<SpaceResponse> UpdateAsync(
            Guid id,
            UpdateSpaceRequest request,
            CancellationToken cancellationToken = default
        )
        {
            await ValidateAsync(_updateValidator, request);

            Space? existingSpace = await _spaceRepository.GetByIdAsync(id, cancellationToken);

            if (existingSpace is null)
            {
                throw new NotFoundException($"Espaço com ID '{id}' não foi encontrado.");
            }

            Space spaceToUpdate = _mapper.Map<Space>(request);
            spaceToUpdate.Id = id;
            spaceToUpdate.IsActive = existingSpace.IsActive;

            Space? updatedSpace = await _spaceRepository.UpdateAsync(spaceToUpdate, cancellationToken);

            if (updatedSpace is null)
            {
                throw new NotFoundException($"Espaço com ID '{id}' não foi encontrado.");
            }

            _logger.LogInformation("Espaço {SpaceId} atualizado com sucesso.", id);

            return _mapper.Map<SpaceResponse>(updatedSpace);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            bool deleted = await _spaceRepository.SoftDeleteAsync(id, cancellationToken);

            if (!deleted)
            {
                throw new NotFoundException($"Espaço com ID '{id}' não foi encontrado.");
            }

            _logger.LogInformation("Espaço {SpaceId} removido logicamente (soft delete).", id);
        }

        private static void ValidateAvailabilityWindow(DateTime from, DateTime to)
        {
            if (to <= from)
            {
                throw new BusinessException("A data final deve ser posterior à data inicial.");
            }
        }

        private static DateTime NormalizeToUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }

        private static bool isConfirmedBooking(string status)
        {
            return string.Equals(status, ConfirmedBookingStatus, StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasDateOverlap(
            DateTime bookingStart,
            DateTime bookingEnd,
            DateTime requestedFrom,
            DateTime requestedTo
        )
        {
            return bookingStart < requestedTo && bookingEnd > requestedFrom;
        }

        private static async Task ValidateAsync<T>(IValidator<T> validator, T instance)
        {
            FluentValidation.Results.ValidationResult validationResult = await validator.ValidateAsync(instance);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }
    }
}