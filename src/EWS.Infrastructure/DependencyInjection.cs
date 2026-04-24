using EWS.Application.Common.Interfaces;
using EWS.Infrastructure.Persistence;
using EWS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EWS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IApproverResolver, ApproverResolver>();
        services.AddScoped<IWorkflowEngine, WorkflowEngine>();
        services.AddScoped<IDocumentNumberService, DocumentNumberService>();
        services.AddSingleton<IDateTimeService, DateTimeService>();

        return services;
    }
}
