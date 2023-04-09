using Microsoft.AspNetCore.Mvc;
using RandomAPIApp.DTOs;

namespace RandomAPIApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private IEnumerable<OrderDTO> _orders = new List<OrderDTO>
    {
        new OrderDTO { Id = 1, Amount = 10 },
        new OrderDTO { Id = 2, Amount = 20 },
        new OrderDTO { Id = 3, Amount = 30 },
        new OrderDTO { Id = 4, Amount = 40 },
        new OrderDTO { Id = 5, Amount = 50 },
        new OrderDTO { Id = 6, Amount = 60 },
        new OrderDTO { Id = 7, Amount = 70 },
        new OrderDTO { Id = 8, Amount = 80 },
        new OrderDTO { Id = 9, Amount = 90 },
        new OrderDTO { Id = 10, Amount = 100 },
    };
    
    public OrdersController()
    {
        
    }
    
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_orders);
    }
}
