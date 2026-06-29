using MediatR;
using Order.Api.Application.Dtos;

namespace Order.Api.Features.GetOrdersByCustomer;

public record GetOrdersByCustomerQuery(string CustomerId) : IRequest<IEnumerable<OrderDto>>;
