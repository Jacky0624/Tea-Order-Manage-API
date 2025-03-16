using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeaAPI.Models.Requests.Orders;
using TeaAPI.Models.Responses;
using TeaAPI.Services.Orders.Interfaces;

namespace TeaAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "CanManageOrders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(
            IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("CreateOrder")]
        public async Task<IActionResult> CreateOrderAsync(EditOrderRequest request)
        {
            string user = GetUser();
            var res = await _orderService.CreateAsync(request, user);
            return Ok(res);
        }
        [AllowAnonymous]
        [HttpPost("GetOrderById")]
        public async Task<IActionResult> GetOrderByIdAsync(GetOrderRequest request)
        {
            string user = GetUser();
            var res = await _orderService.GetByIdAsync(request.Id);
            return Ok(res);
        }

        [AllowAnonymous]
        [HttpGet("GetAllOrders")]
        public async Task<IActionResult> GetAllOrdersAsync()
        {
            string user = GetUser();
            var res = await _orderService.GetAllAsync();
            return Ok(res);
        }


        [HttpPost("DeleteOrder")]
        public async Task<IActionResult> DeleteOrderAsync(DeleteOrderRequest request)
        {
            string user = GetUser();
            var res = await _orderService.DeleteAsync(request.Id);
            return Ok(res);
        }

        [HttpPost("UdateOrder")]
        public async Task<IActionResult> UdateOrderAsync(UpdateOrderRequest request)
        {
            string user = GetUser();
            var res = await _orderService.UpdateAsync(request, user);
            return Ok(res);
        }

        private string GetUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userIdClaim ?? "System";
        }
    }
}
