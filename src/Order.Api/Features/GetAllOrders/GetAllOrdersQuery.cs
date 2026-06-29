using MediatR;
using Order.Api.Application.Dtos;

namespace Order.Api.Features.GetAllOrders;

public record GetAllOrdersQuery : IRequest<IEnumerable<OrderDto>>;
