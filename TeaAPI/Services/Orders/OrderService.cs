using TeaAPI.Dtos.Orders;
using TeaAPI.Models.Orders.Enums;
using TeaAPI.Models.Orders;
using TeaAPI.Models.Responses;
using TeaAPI.Repositories.Orders.Interfaces;
using TeaAPI.Services.Orders.Interfaces;
using TeaAPI.Models.Requests.Orders;
using TeaAPI.Services.Products.Interfaces;
using FluentValidation;
using System.Transactions;

namespace TeaAPI.Services.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IValidator<EditOrderRequest> _validator;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemService _orderItemService;
        private readonly IProductService _productService;
        public OrderService( 
            IOrderRepository orderRepository,
            IOrderItemService orderItemService,
            IProductService productService,
            IValidator<EditOrderRequest> validator)
        {         
            _orderRepository = orderRepository;
            _orderItemService = orderItemService;
            _productService = productService;
            _validator = validator;
        }

        public async Task<ResponseBase> CreateAsync(EditOrderRequest request, string user)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return GenerateErrorResponse(-6, validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var (orderItems, totalAmount, validationErrors) = await ProcessOrderItems(request.Items);
            if (validationErrors.Any())
            {
                return GenerateErrorResponse(-1, validationErrors);
            }

            var newOrder = new OrderPO
            {
                TotalAmount = totalAmount,
                Phone = request.Phone,
                Title = request.Title,
                Address = request.Address,
                Remark = request.Remark,
                OrderStatus = OrderStatusEnum.Pending,
                OrderDate = request.OrderDate,
                CreateUser = user,
                ModifyUser = user,
                CreateAt = DateTime.UtcNow,
                ModifyAt = DateTime.UtcNow
            };

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                int orderId = await _orderRepository.CreateAsync(newOrder);
                if (orderId <= 0)
                {
                    return GenerateErrorResponse(-1, new List<string> { "create fail" });
                }

                foreach (var item in orderItems)
                {
                    await _orderItemService.CreateAsync(orderId, item, user);
                }

                transaction.Complete();
            }

            return new ResponseBase { ResultCode = 0 };
        }

        public async Task<ResponseBase> DeleteAsync(int id)
        {
            var existingOrder = await _orderRepository.GetByIdAsync(id);
            if (existingOrder == null)
            {
                return GenerateErrorResponse(-1, new List<string> { "order not exist" });
            }
            if (existingOrder.OrderStatus == OrderStatusEnum.Canceled || existingOrder.OrderStatus == OrderStatusEnum.Completed)
            {
                return GenerateErrorResponse(-2, new List<string> { "order cannot be deleted" });
            }

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _orderItemService.DeleteAsync(id);
                await _orderRepository.DeleteAsync(id);
                transaction.Complete();
            }

            return new ResponseBase { ResultCode = 0 };
        }

        public async Task<IEnumerable<OrderDTO>> GetAllAsync()
        {
            var orders = await _orderRepository.GetAllAsync();

            return orders.Select(o => new OrderDTO
            {
                Id = o.Id,
                Phone = o.Phone,
                Title = o.Title,
                Address = o.Address,
                OrderStatus = o.OrderStatus,
                OrderDate = o.OrderDate,
                CreateAt = o.CreateAt,
                Remark = o.Remark,
                TotalAmount = o.TotalAmount
            });
        }

        public async Task<OrderDTO> GetByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return null;

            return new OrderDTO
            {
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Remark = order.Remark,
                Id = order.Id,
                Phone = order.Phone,
                Title = order.Title,
                Address = order.Address,
                OrderStatus = order.OrderStatus,
                CreateAt = order.CreateAt,
                Items = await _orderItemService.GetByOrderIdAsync(id)
            };
        }

        public async Task<ResponseBase> UpdateAsync(UpdateOrderRequest request, string user)
        {
            var existingOrder = await _orderRepository.GetByIdAsync(request.Id);
            if (existingOrder == null)
            {
                return new ResponseBase { ResultCode = -1, Errors = new List<string> { "order not exist" } };
            }
            if(existingOrder.OrderStatus == OrderStatusEnum.Canceled || existingOrder.OrderStatus == OrderStatusEnum.Completed)
            {
                return new ResponseBase { ResultCode = -2, Errors = new List<string> { "order can not modify" } };
            }

            var orderItems = new List<OrderItemDTO>();
            decimal totalAmount = 0;
            foreach (var item in request.Items)
            {
                decimal itemPrice = 0;
                var product = await _productService.GetByIdAsync(item.ProductId, true);
                if (product == null)
                {
                    return new ResponseBase()
                    {
                        ResultCode = -3,
                        Errors = new List<string>() { $"product:{item.ProductId} not exist" }
                    };
                }
                var size = product.ProductSizes.FirstOrDefault(x => x.Id == item.SelectedSize);
                if (size == null)
                {
                    return new ResponseBase()
                    {
                        ResultCode = -4,
                        Errors = new List<string>() { "size error" }
                    };
                }
                itemPrice += size.Price;
                var optionsDict = new Dictionary<int, OrderItemOptionDTO>();
                foreach (var val in item.SelectedValues)
                {
                    var type = product.Options.FirstOrDefault(x => x.Id == val.TypeId);
                    if (type == null)
                    {
                        return new ResponseBase()
                        {
                            ResultCode = -1,
                            Errors = new List<string>() { "type error" }
                        };
                    }
                    var option = type.VariantValues.FirstOrDefault(x => x.Id == val.ValueId);
                    if (option == null)
                    {
                        return new ResponseBase()
                        {
                            ResultCode = -1,
                            Errors = new List<string>() { "option error" }
                        };
                    }
                    if (optionsDict.ContainsKey(type.Id))
                    {
                        return new ResponseBase()
                        {
                            ResultCode = -1,
                            Errors = new List<string>() { "duplicate answer" }
                        };
                    }
                    itemPrice += option.ExtraPrice;
                    optionsDict[type.Id] = new OrderItemOptionDTO()
                    {
                        ExtraValue = option.ExtraPrice,
                        VariantType = type.TypeName,
                        VariantTypeId = type.Id,
                        VariantValue = option.Value,
                        VariantValueId = option.Id
                    };
                }
                orderItems.Add(new OrderItemDTO()
                {
                    Count = item.Count,
                    ProductSizeName = size.Size,
                    ProductSizeId = size.Id,
                    Price = size.Price,
                    ProductId = product.Id,
                    Remark = item.Remark,
                    ProductName = product.Name,
                    Options = optionsDict.Values
                });
                totalAmount += itemPrice * item.Count;
            }

            existingOrder.TotalAmount = totalAmount;
            existingOrder.Phone = request.Phone;
            existingOrder.Title = request.Title;
            existingOrder.Address = request.Address;
            existingOrder.OrderStatus = (OrderStatusEnum)request.OrderStatus;
            existingOrder.OrderDate = request.OrderDate;
            existingOrder.ModifyUser = user;
            existingOrder.ModifyAt = DateTime.UtcNow;

            await _orderRepository.ModifyAsync(existingOrder);

            await _orderItemService.DeleteAsync(existingOrder.Id);
            foreach (var item in orderItems)
            {
                await _orderItemService.CreateAsync(existingOrder.Id, item, user);
            }

            return new ResponseBase { ResultCode = 0 };
        }

        private async Task<(List<OrderItemDTO>, decimal, List<string>)> ProcessOrderItems(List<OrderItemRequest> items)
        {
            var orderItems = new List<OrderItemDTO>();
            decimal totalAmount = 0;
            var errors = new List<string>();

            foreach (var item in items)
            {
                var (orderItem, itemTotalPrice, error) = await ValidateAndBuildOrderItem(item);
                if (!string.IsNullOrEmpty(error))
                {
                    errors.Add(error);
                    continue;
                }

                orderItems.Add(orderItem);
                totalAmount += itemTotalPrice * item.Count;
            }

            return (orderItems, totalAmount, errors);
        }

        private async Task<(OrderItemDTO, decimal, string)> ValidateAndBuildOrderItem(OrderItemRequest item)
        {
            var product = await _productService.GetByIdAsync(item.ProductId);
            if (product == null) return (null, 0, $"product:{item.ProductId} not exist");

            var size = product.ProductSizes.FirstOrDefault(x => x.Id == item.SelectedSize);
            if (size == null) return (null, 0, "size error");

            decimal itemPrice = size.Price;
            var optionsDict = new Dictionary<int, OrderItemOptionDTO>();

            foreach (var val in item.SelectedValues)
            {
                var type = product.Options.FirstOrDefault(x => x.Id == val.TypeId);
                if (type == null) return (null, 0, "type error");

                var option = type.VariantValues.FirstOrDefault(x => x.Id == val.ValueId);
                if (option == null) return (null, 0, "option error");

                if (optionsDict.ContainsKey(type.Id))
                    return (null, 0, "duplicate answer");

                itemPrice += option.ExtraPrice;
                optionsDict[type.Id] = new OrderItemOptionDTO
                {
                    ExtraValue = option.ExtraPrice,
                    VariantType = type.TypeName,
                    VariantTypeId = type.Id,
                    VariantValue = option.Value,
                    VariantValueId = option.Id
                };
            }

            var orderItemDTO = new OrderItemDTO
            {
                Count = item.Count,
                ProductSizeName = size.Size,
                ProductSizeId = size.Id,
                Price = size.Price,
                ProductId = product.Id,
                Remark = item.Remark,
                ProductName = product.Name,
                Options = optionsDict.Values
            };

            return (orderItemDTO, itemPrice, null);
        }

        private ResponseBase GenerateErrorResponse(int resultCode, List<string> errors)
        {
            return new ResponseBase
            {
                ResultCode = resultCode,
                Errors = errors
            };
        }

    }
}
