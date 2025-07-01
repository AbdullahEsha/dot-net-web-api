using Microsoft.AspNetCore.Mvc;
using dot_net_web_api.DTOs;
using dot_net_web_api.Services.Interfaces;

namespace dot_net_web_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdOrder = await _orderService.CreateOrderAsync(createOrderDto);
                return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.Id }, createdOrder);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request to create order");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new { message = "An error occurred while creating the order." });
            }
        }

        /// <summary>
        /// Get an order by its ID
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid order ID." });
                }

                var order = await _orderService.GetOrderByIdAsync(id);
                return order == null
                    ? NotFound(new { message = "Order not found." })
                    : Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting order with ID {id}");
                return StatusCode(500, new { message = "An error occurred while retrieving the order." });
            }
        }

        /// <summary>
        /// Get all orders
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all orders");
                return StatusCode(500, new { message = "An error occurred while retrieving orders." });
            }
        }

        /// <summary>
        /// Get orders by user ID
        /// </summary>
        [HttpGet("user/{userId:int}")]
        [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrdersByUserId(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "Invalid user ID." });
                }

                var orders = await _orderService.GetOrdersByUserIdAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting orders for user {userId}");
                return StatusCode(500, new { message = "An error occurred while retrieving user orders." });
            }
        }

        /// <summary>
        /// Update order status
        /// </summary>
        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderDto updateDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid order ID." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _orderService.UpdateOrderStatusAsync(id, updateDto);
                return success
                    ? Ok(new { message = "Order status updated successfully." })
                    : NotFound(new { message = "Order not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status for order {id}");
                return StatusCode(500, new { message = "An error occurred while updating order status." });
            }
        }

        /// <summary>
        /// Cancel an order
        /// </summary>
        [HttpPatch("{id:int}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid order ID." });
                }

                var success = await _orderService.CancelOrderAsync(id);
                return success
                    ? Ok(new { message = "Order cancelled successfully." })
                    : NotFound(new { message = "Order not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling order {id}");
                return StatusCode(500, new { message = "An error occurred while cancelling the order." });
            }
        }

        /// <summary>
        /// Delete an order
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid order ID." });
                }

                var success = await _orderService.DeleteOrderAsync(id);
                return success
                    ? Ok(new { message = "Order deleted successfully." })
                    : NotFound(new { message = "Order not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting order {id}");
                return StatusCode(500, new { message = "An error occurred while deleting the order." });
            }
        }
    }
}