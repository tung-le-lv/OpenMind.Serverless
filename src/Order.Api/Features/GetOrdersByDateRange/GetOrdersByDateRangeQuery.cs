using MediatR;
using Order.Api.Application.Dtos;

namespace Order.Api.Features.GetOrdersByDateRange;

public record GetOrdersByDateRangeQuery(DateOnly Date) : IRequest<IEnumerable<OrderDto>>;
