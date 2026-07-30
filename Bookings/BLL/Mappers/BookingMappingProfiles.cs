using AutoMapper;
using Reservation.BookingsService.BLL.DTO;
using Reservation.BookingsService.DAL.Entities;
using Reservation.BookingsService.DAL.Enums;

namespace Reservation.BookingsService.BLL.Mappers;

public class BookingToBookingResponseMappingProfile : Profile
{
    public BookingToBookingResponseMappingProfile()
    {
        CreateMap<Booking, BookingResponse>()
            .ForMember(
                destination => destination.Status,
                options => options.MapFrom(source => source.Status.ToString()));
    }
}

public class CreateBookingRequestToBookingMappingProfile : Profile
{
    public CreateBookingRequestToBookingMappingProfile()
    {
        CreateMap<CreateBookingRequest, Booking>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.Status, options => options.MapFrom(_ => BookingStatus.Confirmed))
            .ForMember(destination => destination.CreatedAt, options => options.Ignore());
    }
}

public class UpdateBookingRequestToBookingMappingProfile : Profile
{
    public UpdateBookingRequestToBookingMappingProfile()
    {
        CreateMap<UpdateBookingRequest, Booking>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore());
    }
}
