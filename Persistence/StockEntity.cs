using System.ComponentModel.DataAnnotations;

namespace OrderStateMachineDemo.Persistence;

/// <summary>Entity for stock levels.</summary>
public class StockEntity
{
    [Key]
    public int StockId { get; set; }

    public int Quantity { get; set; }
}

