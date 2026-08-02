using AutoMapper;
using FluentValidation;
using Reservation.NotificationsService.BLL.DTOs;
using Reservation.NotificationsService.BLL.Exceptions;
using Reservation.NotificationsService.BLL.Interfaces;
using Reservation.NotificationsService.DAL.Entities;
using Reservation.NotificationsService.DAL.Enums;
using Reservation.NotificationsService.DAL.Repositories;

namespace Reservation.NotificationsService.BLL.Services
{
    internal sealed class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<SendNotificationRequest> _sendNotificationValidator;

        public NotificationService(
            INotificationRepository notificationRepository,
            IMapper mapper,
            IValidator<SendNotificationRequest> sendNotificationValidator)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
            _sendNotificationValidator = sendNotificationValidator;
        }

        public async Task<NotificationResponse> SendAsync(SendNotificationRequest request)
        {
            await _sendNotificationValidator.ValidateAndThrowAsync(request);

            Notification notification = BuildNotification(request.UserId, request.Message, request.Type);
            await _notificationRepository.CreateAsync(notification);

            return _mapper.Map<NotificationResponse>(notification);
        }

        public async Task<IReadOnlyList<NotificationResponse>> GetByUserIdAsync(
            Guid userId,
            Guid? authenticatedUserId,
            string? role)
        {
            EnsureUserCanAccessNotifications(userId, authenticatedUserId, role);

            IReadOnlyList<Notification> notifications = await _notificationRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IReadOnlyList<NotificationResponse>>(notifications);
        }

        public async Task ProcessBookingCreatedEventAsync(BookingCreatedEvent bookingEvent)
        {
            string message =
                $"Sua reserva {bookingEvent.BookingId} para o espaço {bookingEvent.SpaceId} " +
                $"de {bookingEvent.StartDate:u} até {bookingEvent.EndDate:u} foi criada com sucesso.";

            Notification notification = BuildNotification(
                bookingEvent.UserId,
                message,
                NotificationType.BookingCreated);

            await _notificationRepository.CreateAsync(notification);
        }

        private static Notification BuildNotification(Guid userId, string message, NotificationType type)
        {
            return new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Message = message,
                Type = type,
                SentAt = DateTime.UtcNow,
                Read = false
            };
        }

        private static void EnsureUserCanAccessNotifications(Guid userId, Guid? authenticatedUserId, string? role)
        {
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (authenticatedUserId is null || authenticatedUserId.Value != userId)
            {
                throw new ForbiddenException("Você não tem permissão para acessar as notificações deste usuário.");
            }
        }
    }
}
