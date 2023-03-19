namespace RandomAPIApp.DTOs;

public class CustomerDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public required List<OrderDTO> Orders { get; set; }
}
