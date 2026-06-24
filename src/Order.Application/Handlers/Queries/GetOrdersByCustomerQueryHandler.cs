using MediatR;
using Order.Application.DTOs;
using Order.Application.Mappers;
using Order.Application.Queries;
using Order.Domain.Repositories;

namespace Order.Application.Handlers.Queries;

public class GetOrdersByCustomerQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetOrdersByCustomerQuery, IEnumerable<OrderDto>>
{
    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        return orders.Select(OrderMapper.ToDto);
    }
}
