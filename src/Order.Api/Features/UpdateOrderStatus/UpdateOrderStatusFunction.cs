using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Shared;
using System.Text.Json;
using Order.Api.Domain.Enums;
using Order.Api.Domain.Interfaces;
using Order.Api.Domain.Repositories;
using Order.Api.Infrastructure.EventBus;
using Order.Api.Infrastructure.Repositories;
using Order.Api.Shared.Helpers;

namespace Order.Api.Features.UpdateOrderStatus;

public class UpdateOrderStatusFunction(IMediator mediator)
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
        services.AddSingleton<IOrderRepository, DynamoDbOrderRepository>();
        if (Environment.GetEnvironmentVariable("USE_LOCAL_EVENT_BUS") == "true")
        {
            services.AddSingleton<IEventBus, InMemoryEventBus>();
        }
        else
        {
            services.AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();
            services.AddSingleton<IEventBus, SnsEventBus>();
        }
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DynamoDbOrderRepository).Assembly));
        services.AddTransient<IRequestHandler<UpdateOrderStatusCommand, UpdateOrderStatusResult>, UpdateOrderStatusHandler>();
        return services.BuildServiceProvider();
    }

    public UpdateOrderStatusFunction() : this(_serviceProvider.GetRequiredService<IMediator>()) { }

    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
    [Logging(LogEvent = false, CorrelationIdPath = CorrelationIdPaths.ApiGatewayRest)]
    [Tracing]
    [Metrics(Namespace = "OrderService", CaptureColdStart = true)]
    public async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var orderId = request.PathParameters?["id"];
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return ApiResponseHelper.CreateResponse(400, ApiResponse<string>.ErrorResponse("Order ID is required."));
            }

            if (string.IsNullOrWhiteSpace(request.Body))
            {
                return ApiResponseHelper.CreateResponse(400, ApiResponse<string>.ErrorResponse("Request body is required."));
            }

            var updateRequest = JsonSerializer.Deserialize<UpdateStatusRequest>(request.Body, ApiResponseHelper.JsonOptions);
            if (updateRequest == null)
            {
                return ApiResponseHelper.CreateResponse(400, ApiResponse<string>.ErrorResponse("Invalid request body."));
            }

            Logger.LogInformation("Updating status for order {OrderId} to {Status}", orderId, updateRequest.Status);

            var result = await mediator.Send(new UpdateOrderStatusCommand(orderId, updateRequest.Status));

            if (!result.Success)
            {
                var statusCode = result.Message?.Contains("not found") == true ? 404 : 400;
                return ApiResponseHelper.CreateResponse(statusCode, ApiResponse<string>.ErrorResponse(result.Message ?? "Failed to update status.", result.Errors));
            }

            Metrics.AddMetric("OrderStatusUpdated", 1, MetricUnit.Count);
            return ApiResponseHelper.CreateResponse(200, ApiResponse<string>.SuccessResponse("OK", result.Message));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating status for order {OrderId}", request.PathParameters?["id"]);
            return ApiResponseHelper.CreateResponse(500, ApiResponse<string>.ErrorResponse("Internal server error.", [ex.Message]));
        }
    }

    private record UpdateStatusRequest(OrderStatus Status);
}
