using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Reservation.BookingsService.BLL.Constants;
using Reservation.BookingsService.BLL.DTO;
using Reservation.BookingsService.BLL.Exceptions;
using Reservation.BookingsService.BLL.HttpClients;
using Reservation.BookingsService.BLL.Messaging;
using Reservation.BookingsService.DAL.Entities;
using Reservation.BookingsService.DAL.Enums;
using Reservation.BookingsService.DAL.Repositories;

namespace Reservation.BookingsService.BLL.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUsersHttpClient _usersHttpClient;
    private readonly ISpacesHttpClient _spacesHttpClient;
    private readonly IEventPublisher _eventPublisher;
    private readonly IValidator<CreateBookingRequest> _createValidator;
    private readonly IValidator<UpdateBookingRequest> _updateValidator;
    private readonly IMapper _mapper;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        IBookingRepository bookingRepository,
        IUsersHttpClient usersHttpClient,
        ISpacesHttpClient spacesHttpClient,
        IEventPublisher eventPublisher,
        IValidator<CreateBookingRequest> createValidator,
        IValidator<UpdateBookingRequest> updateValidator,
        IMapper mapper,
        ILogger<BookingService> logger)
    {
        _bookingRepository = bookingRepository;
        _usersHttpClient = usersHttpClient;
        _spacesHttpClient = spacesHttpClient;
        _eventPublisher = eventPublisher;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<BookingResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Booking> bookings = await _bookingRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<BookingResponse>>(bookings);
    }

    public async Task<BookingResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Booking? booking = await _bookingRepository.GetByIdAsync(id, cancellationToken);

        if (booking is null)
        {
            throw new NotFoundException($"Reserva com ID '{id}' não foi encontrada.");
        }

        return _mapper.Map<BookingResponse>(booking);
    }

    public async Task<IReadOnlyList<BookingResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Booking> bookings = await _bookingRepository.GetByUserIdAsync(userId, cancellationToken);
        return _mapper.Map<IReadOnlyList<BookingResponse>>(bookings);
    }

    public async Task<IReadOnlyList<BookingResponse>> GetBySpaceIdAsync(Guid spaceId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Booking> bookings = await _bookingRepository.GetBySpaceIdAsync(spaceId, cancellationToken);
        return _mapper.Map<IReadOnlyList<BookingResponse>>(bookings);
    }

    public async Task<IReadOnlyList<BookingResponse>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        DateTime normalizedDate = NormalizeToUtc(date);
        IReadOnlyList<Booking> bookings = await _bookingRepository.GetByDateAsync(normalizedDate, cancellationToken);
        return _mapper.Map<IReadOnlyList<BookingResponse>>(bookings);
    }

    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_createValidator, request);

        DateTime startDateUtc = NormalizeToUtc(request.StartDate);
        DateTime endDateUtc = NormalizeToUtc(request.EndDate);

        _logger.LogInformation(
            "Iniciando criação de reserva para usuário {UserId} no espaço {SpaceId}.",
            request.UserId,
            request.SpaceId);

        await ValidateReferencesAsync(request.UserId, request.SpaceId, cancellationToken);
        await ValidateDateOverlapAsync(request.SpaceId, startDateUtc, endDateUtc, cancellationToken: cancellationToken);

        Booking booking = _mapper.Map<Booking>(request);
        booking.Id = Guid.NewGuid();
        booking.StartDate = startDateUtc;
        booking.EndDate = endDateUtc;
        booking.Status = BookingStatus.Confirmed;
        booking.CreatedAt = DateTime.UtcNow;

        Booking createdBooking = await _bookingRepository.AddAsync(booking, cancellationToken);

        _logger.LogInformation("Reserva {BookingId} criada com sucesso.", createdBooking.Id);

        PublishBookingCreatedFireAndForget(createdBooking);

        return _mapper.Map<BookingResponse>(createdBooking);
    }

    public async Task<BookingResponse> UpdateAsync(
        Guid id,
        UpdateBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_updateValidator, request);

        Booking? existingBooking = await _bookingRepository.GetByIdAsync(id, cancellationToken);

        if (existingBooking is null)
        {
            throw new NotFoundException($"Reserva com ID '{id}' não foi encontrada.");
        }

        if (existingBooking.Status == BookingStatus.Cancelled)
        {
            throw new BusinessException("Não é possível atualizar uma reserva cancelada.");
        }

        DateTime startDateUtc = NormalizeToUtc(request.StartDate);
        DateTime endDateUtc = NormalizeToUtc(request.EndDate);

        await ValidateReferencesAsync(request.UserId, request.SpaceId, cancellationToken);

        if (request.Status == BookingStatus.Confirmed)
        {
            await ValidateDateOverlapAsync(
                request.SpaceId,
                startDateUtc,
                endDateUtc,
                excludeBookingId: id,
                cancellationToken: cancellationToken);
        }

        Booking bookingToUpdate = _mapper.Map<Booking>(request);
        bookingToUpdate.Id = id;
        bookingToUpdate.StartDate = startDateUtc;
        bookingToUpdate.EndDate = endDateUtc;
        bookingToUpdate.CreatedAt = existingBooking.CreatedAt;

        Booking? updatedBooking = await _bookingRepository.UpdateAsync(bookingToUpdate, cancellationToken);

        if (updatedBooking is null)
        {
            throw new NotFoundException($"Reserva com ID '{id}' não foi encontrada.");
        }

        _logger.LogInformation("Reserva {BookingId} atualizada com sucesso.", id);

        return _mapper.Map<BookingResponse>(updatedBooking);
    }

    public async Task CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Booking? existingBooking = await _bookingRepository.GetByIdAsync(id, cancellationToken);

        if (existingBooking is null)
        {
            throw new NotFoundException($"Reserva com ID '{id}' não foi encontrada.");
        }

        if (existingBooking.Status == BookingStatus.Cancelled)
        {
            throw new BusinessException("A reserva já está cancelada.");
        }

        bool cancelled = await _bookingRepository.CancelAsync(id, cancellationToken);

        if (!cancelled)
        {
            throw new NotFoundException($"Reserva com ID '{id}' não foi encontrada.");
        }

        _logger.LogInformation("Reserva {BookingId} cancelada com sucesso.", id);
    }

    private async Task ValidateReferencesAsync(Guid userId, Guid spaceId, CancellationToken cancellationToken)
    {
        UserReferenceResponse? user = await _usersHttpClient.GetUserByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new InvalidReferenceException($"O usuário com ID '{userId}' não foi encontrado.");
        }

        SpaceReferenceResponse? space = await _spacesHttpClient.GetSpaceByIdAsync(spaceId, cancellationToken);

        if (space is null)
        {
            throw new InvalidReferenceException($"O espaço com ID '{spaceId}' não foi encontrado.");
        }

        if (!space.IsActive)
        {
            throw new InvalidReferenceException($"O espaço com ID '{spaceId}' não está ativo.");
        }
    }

    private async Task ValidateDateOverlapAsync(
        Guid spaceId,
        DateTime startDateUtc,
        DateTime endDateUtc,
        Guid? excludeBookingId = null,
        CancellationToken cancellationToken = default)
    {
        bool hasConflict = await _bookingRepository.HasConflictBookingAsync(
            spaceId,
            startDateUtc,
            endDateUtc,
            excludeBookingId,
            cancellationToken);

        if (hasConflict)
        {
            throw new BookingConflictException(
                $"Já existe uma reserva confirmada para o espaço '{spaceId}' no intervalo informado.");
        }
    }

    private void PublishBookingCreatedFireAndForget(Booking booking)
    {
        BookingCreatedEvent bookingCreatedEvent = new(
            RedisQueueConstants.BookingCreatedEventType,
            booking.Id,
            booking.UserId,
            booking.SpaceId,
            booking.StartDate,
            booking.EndDate);

        _ = Task.Run(async () =>
        {
            try
            {
                await _eventPublisher.PublishBookingCreatedAsync(bookingCreatedEvent);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Falha ao publicar evento BookingCreated para a reserva {BookingId}.",
                    booking.Id);
            }
        });
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

    private static async Task ValidateAsync<T>(IValidator<T> validator, T instance)
    {
        FluentValidation.Results.ValidationResult validationResult = await validator.ValidateAsync(instance);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }
}
