using MediatR;
using Order.Api.Shared;

namespace Order.Api.Features.GetAllOrders;

public record GetAllOrdersQuery : IRequest<IEnumerable<OrderDto>>;
