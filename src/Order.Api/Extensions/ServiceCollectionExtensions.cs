using Amazon.DynamoDBv2;
using Amazon.SimpleNotificationService;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Commands;
using Order.Application.DTOs;
using Order.Application.Handlers.Commands;
using Order.Application.Handlers.Queries;
using Order.Application.Interfaces;
using Order.Application.Queries;
using Order.Application.Validators;
using Order.Domain.Repositories;
using Order.Infrastructure.EventBus;
using Order.Infrastructure.Repositories;

namespace Order.Api.Extensions;

public static class ServiceCollectionExtensions
{
    private static IServiceCollection AddOrderInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
        services.AddSingleton<IOrderRepository, DynamoDbOrderRepository>();
        return services;
    }

    private static IServiceCollection AddOrderEventBus(this IServiceCollection services)
    {
        services.AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();
        var useLocalEventBus = Environment.GetEnvironmentVariable("USE_LOCAL_EVENT_BUS") == "true";
        if (useLocalEventBus)
            services.AddSingleton<IEventBus, InMemoryEventBus>();
        else
            services.AddSingleton<IEventBus, SnsEventBus>();
        return services;
    }

    public static IServiceCollection AddCreateOrderServices(this IServiceCollection services)
    {
        services.AddOrderInfrastructure().AddOrderEventBus();
        services.AddMediatR(cfg => { });
        services.AddTransient<IRequestHandler<CreateOrderCommand, CreateOrderResult>, CreateOrderCommandHandler>();
        services.AddTransient<IValidator<CreateOrderCommand>, CreateOrderCommandValidator>();
        return services;
    }

    public static IServiceCollection AddAddOrderItemServices(this IServiceCollection services)
    {
        services.AddOrderInfrastructure().AddOrderEventBus();
        services.AddMediatR(cfg => { });
        services.AddTransient<IRequestHandler<AddOrderItemCommand, AddOrderItemResult>, AddOrderItemCommandHandler>();
        services.AddTransient<IValidator<AddOrderItemCommand>, AddOrderItemCommandValidator>();
        return services;
    }

    public static IServiceCollection AddCancelOrderServices(this IServiceCollection services)
    {
        services.AddOrderInfrastructure().AddOrderEventBus();
        services.AddMediatR(cfg => { });
        services.AddTransient<IRequestHandler<CancelOrderCommand, CancelOrderResult>, CancelOrderCommandHandler>();
        return services;
    }

    public static IServiceCollection AddDeleteOrderServices(this IServiceCollection services)
    {
        services.AddOrderInfrastructure();
        services.AddMediatR(cfg => { });
        services.AddTransient<IRequestHandler<DeleteOrderCommand, DeleteOrderResult>, DeleteOrderCommandHandler>();
        return services;
    }

    public static IServiceCollection AddUpdateOrderStatusServices(this IServiceCollection services)
    {
        services.AddOrderInfrastructure().AddOrderEventBus();
        services.AddMediatR(cfg => { });
        services.AddTransient<IRequestHandler<UpdateOrderStatusCommand, UpdateOrderStatusResult>, UpdateOrderStatusCommandHandler>();
        return services;
    }

    public static IServiceCollection AddGetOrderServices(this IServiceCollection services)
    {
        services.AddOrderInfrastructure();
        services.AddMediatR(cfg => { });
        services.AddTransient<IRequestHandler<GetOrderQuery, OrderDto?>, GetOrderQueryHandler>();
        return services;
    }

    public static IServiceCollection AddGetAllOrdersServices(this IServiceCollection services)
    {
        services.AddOrderInfrastructure();
        services.AddMediatR(cfg => { });
        services.AddTransient<IRequestHandler<GetAllOrdersQuery, IEnumerable<OrderDto>>, GetAllOrdersQueryHandler>();
        return services;
    }

    public static IServiceCollection AddGetOrdersByCustomerServices(this IServiceCollection services)
    {
        services.AddOrderInfrastructure();
        services.AddMediatR(cfg => { });
        services.AddTransient<IRequestHandler<GetOrdersByCustomerQuery, IEnumerable<OrderDto>>, GetOrdersByCustomerQueryHandler>();
        return services;
    }
}
