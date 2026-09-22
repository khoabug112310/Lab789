using System;
using System.Collections.Generic;

namespace Lab789SalesWebApi.Core.Entities;

public partial class Order
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public DateTime OrderDate { get; set; }

    public int Quantity { get; set; }

    public virtual Product Product { get; set; } = null!;
}
