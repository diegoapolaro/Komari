using FluentValidation;
using Komari.Application.Bills.Interfaces;
using Komari.Application.Bills.Services;
using Komari.Application.Categories.Interfaces;
using Komari.Application.Categories.Services;
using Komari.Application.Orders.Interfaces;
using Komari.Application.Orders.Services;
using Komari.Application.Products.Interfaces;
using Komari.Application.Products.Services;
using Komari.Application.Tables.Interfaces;
using Komari.Application.Tables.Services;
using Komari.Application.CashSessions.Interfaces;
using Komari.Application.CashSessions.Services;
using Komari.Application.Customers.Interfaces;
using Komari.Application.Customers.Services;
using Komari.Application.Payments.Interfaces;
using Komari.Application.Payments.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Komari.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<IBillService, BillService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICashSessionService, CashSessionService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ICustomerService, CustomerService>();

        return services;
    }
}
