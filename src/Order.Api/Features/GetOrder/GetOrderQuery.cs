using MediatR;
using Order.Api.Shared;

namespace Order.Api.Features.GetOrder;

public record GetOrderQuery(string OrderId) : IRequest<OrderDto?>;
