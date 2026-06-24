using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Extensions;
using Order.Application.DTOs;
using Order.Application.Queries;
using System.Text.Json;

namespace Order.Api.Functions;

public class GetOrder
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddOrderServices();
        return services.BuildServiceProvider();
    }

    private readonly IMediator _mediator;

    public GetOrder()
    {
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
    }

    public GetOrder(IMediator mediator)
    {
        _mediator = mediator;
    }

    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
    public async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var orderId = request.PathParameters?["id"];
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return CreateResponse(400, ApiResponse<OrderDto>.ErrorResponse("Order ID is required."));
            }

            context.Logger.LogInformation($"Getting order with ID: {orderId}");

            var query = new GetOrderQuery(orderId);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return CreateResponse(404, ApiResponse<OrderDto>.ErrorResponse($"Order with ID '{orderId}' not found."));
            }

            return CreateResponse(200, ApiResponse<OrderDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error getting order: {ex.Message}");
            return CreateResponse(500, ApiResponse<OrderDto>.ErrorResponse("Internal server error.", [ex.Message]));
        }
    }

    private static APIGatewayProxyResponse CreateResponse<T>(int statusCode, ApiResponse<T> body)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = statusCode,
            Body = JsonSerializer.Serialize(body, _jsonOptions),
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Headers", "Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token" },
                { "Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS" }
            }
        };
    }
}
