using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using RandomAPIApp.DTOs;

namespace RandomAPIApp.Controllers;

public class CustomersController : ODataController
{
    private static Random random = new Random();
    private static List<CustomerDTO> customers = new List<CustomerDTO>(
        Enumerable.Range(1, 3).Select(idx => new CustomerDTO
        {
            Id = idx,
            Name = $"Customer {idx}",
            Orders = new List<OrderDTO>(
                Enumerable.Range(1, 2).Select(dx => new OrderDTO
                {
                    Id = (idx - 1) * 2 + dx,
                    Amount = random.Next(1, 9) * 10
                }))
        }));

    [EnableQuery]
    public ActionResult Get()
    {
        return Ok(customers);
    }

    [EnableQuery]
    public ActionResult Get([FromRoute] int key)
    {
        var item = customers.SingleOrDefault(d => d.Id.Equals(key));

        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }
}