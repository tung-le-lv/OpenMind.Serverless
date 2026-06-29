using MediatR;
using Order.Api.Application.Dtos;

namespace Order.Api.Features.GetOrder;

public record GetOrderQuery(string OrderId) : IRequest<OrderDto?>;
