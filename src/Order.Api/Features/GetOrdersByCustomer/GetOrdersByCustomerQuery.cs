using MediatR;
using Order.Api.Shared;

namespace Order.Api.Features.GetOrdersByCustomer;

public record GetOrdersByCustomerQuery(string CustomerId) : IRequest<IEnumerable<OrderDto>>;
