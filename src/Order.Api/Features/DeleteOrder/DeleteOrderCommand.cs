using MediatR;

namespace Order.Api.Features.DeleteOrder;

public record DeleteOrderCommand(string OrderId) : IRequest<DeleteOrderResult>;

public record DeleteOrderResult(
    bool Success,
    string? Message
);
