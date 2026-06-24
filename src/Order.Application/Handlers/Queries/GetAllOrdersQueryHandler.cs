using MediatR;
using Order.Application.DTOs;
using Order.Application.Mappers;
using Order.Application.Queries;
using Order.Domain.Repositories;

namespace Order.Application.Handlers.Queries;

public class GetAllOrdersQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetAllOrdersQuery, IEnumerable<OrderDto>>
{
    public async Task<IEnumerable<OrderDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetAllAsync(cancellationToken);
        return orders.Select(OrderMapper.ToDto);
    }
}
