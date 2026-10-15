using Microsoft.Extensions.DependencyInjection;
using Partners.Application.UseCases.Partners;

namespace Partners.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreatePartnerUseCase>();
        services.AddScoped<GetPartnerByIdUseCase>();
        services.AddScoped<GetPartnersUseCase>();
        services.AddScoped<UpdatePartnerUseCase>();

        return services;
    }
}