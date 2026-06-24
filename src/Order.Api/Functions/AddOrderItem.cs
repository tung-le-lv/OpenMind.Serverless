using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Extensions;
using Order.Application.Commands;
using Order.Application.DTOs;
using System.Text.Json;

namespace Order.Api.Functions;

public class AddOrderItem
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

    public AddOrderItem()
    {
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
    }

    public AddOrderItem(IMediator mediator)
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
                return CreateResponse(400, ApiResponse<string>.ErrorResponse("Order ID is required."));
            }

            if (string.IsNullOrWhiteSpace(request.Body))
            {
                return CreateResponse(400, ApiResponse<string>.ErrorResponse("Request body is required."));
            }

            var itemRequest = JsonSerializer.Deserialize<AddItemRequest>(request.Body, _jsonOptions);
            if (itemRequest == null)
            {
                return CreateResponse(400, ApiResponse<string>.ErrorResponse("Invalid request body."));
            }

            context.Logger.LogInformation($"Adding item to order: {orderId}");

            var command = new AddOrderItemCommand(
                orderId,
                itemRequest.ProductId,
                itemRequest.ProductName,
                itemRequest.Quantity,
                itemRequest.UnitPrice
            );

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                var statusCode = result.Message?.Contains("not found") == true ? 404 : 400;
                return CreateResponse(statusCode, ApiResponse<string>.ErrorResponse(result.Message ?? "Failed to add item.", result.Errors));
            }

            return CreateResponse(200, ApiResponse<string>.SuccessResponse("OK", result.Message));
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error adding item to order: {ex.Message}");
            return CreateResponse(500, ApiResponse<string>.ErrorResponse("Internal server error.", [ex.Message]));
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

    private record AddItemRequest(
        string ProductId,
        string ProductName,
        int Quantity,
        decimal UnitPrice
    );
}
