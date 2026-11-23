using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces.Services.Courts;
using Application.Interfaces.Services.Reservations;
using Application.Interfaces.Services.Users;
using Application.Interfaces.Services.Venues;
using Application.Interfaces.Services.Auth;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IRegisterUserService, Services.Auth.RegisterUserService>();
        services.AddScoped<ILoginService, Services.Auth.LoginService>();
        services.AddScoped<IChangeUserEmailService, Services.Users.ChangeUserEmailService>();

        services.AddScoped<IGetUserByIdService, Services.Users.GetUserByIdService>();

        services.AddScoped<ICreateVenueService, Services.Venues.CreateVenueService>();
        services.AddScoped<IUpdateVenueService, Services.Venues.UpdateVenueService>();
        services.AddScoped<IChangeVenueStatusService, Services.Venues.ChangeVenueStatusService>();
        services.AddScoped<IGetVenueByIdService, Services.Venues.GetVenueByIdService>();
        services.AddScoped<IListVenuesByOwnerService, Services.Venues.ListVenuesByOwnerService>();

        services.AddScoped<ICreateCourtService, Services.Courts.CreateCourtService>();
        services.AddScoped<IUpdateCourtService, Services.Courts.UpdateCourtService>();
        services.AddScoped<IChangeCourtStatusService, Services.Courts.ChangeCourtStatusService>();
        services.AddScoped<IGetCourtByIdService, Services.Courts.GetCourtByIdService>();
        services.AddScoped<ISearchNearbyCourtsService, Services.Courts.SearchNearbyCourtsService>();

        services.AddScoped<IGetCourtAvailabilityService, Services.Reservations.GetCourtAvailabilityService>();
        services.AddScoped<ICreateReservationService, Services.Reservations.CreateReservationService>();
        services.AddScoped<IConfirmReservationService, Services.Reservations.ConfirmReservationService>();
        services.AddScoped<ICompleteReservationService, Services.Reservations.CompleteReservationService>();
        services.AddScoped<IMarkReservationAsNoShowService, Services.Reservations.MarkReservationAsNoShowService>();
        services.AddScoped<ICancelReservationService, Services.Reservations.CancelReservationService>();
        services.AddScoped<IListUserReservationsService, Services.Reservations.ListUserReservationsService>();
        services.AddScoped<IListCourtReservationsService, Services.Reservations.ListCourtReservationsService>();

        return services;
    }
}
