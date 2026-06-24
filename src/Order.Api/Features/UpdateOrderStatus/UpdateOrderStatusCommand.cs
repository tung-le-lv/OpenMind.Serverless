using MediatR;
using Order.Api.Domain.Enums;

namespace Order.Api.Features.UpdateOrderStatus;

public record UpdateOrderStatusCommand(
    string OrderId,
    OrderStatus NewStatus
) : IRequest<UpdateOrderStatusResult>;

public record UpdateOrderStatusResult(
    bool Success,
    string? Message,
    List<string>? Errors
);
