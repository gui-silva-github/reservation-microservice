using AutoMapper;
using Reservation.NotificationsService.BLL.DTOs;
using Reservation.NotificationsService.DAL.Entities;

namespace Reservation.NotificationsService.BLL.Mappings
{
    public class NotificationMappingProfile : Profile
    {
        public NotificationMappingProfile()
        {
            CreateMap<Notification, NotificationResponse>();
        }
    }
}
