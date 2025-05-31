namespace OrderService.Domain.Entities
{
	public class Order
	{
		public Guid Id { get; private set; }
		public Guid ProductId { get; private set; }
		public int Quantity { get; private set; }
		public decimal Total { get; private set; }
		public Guid ClientId { get; private set; }
		public DateTime OrderDate { get; private set; }
		public Guid LoggedInEmployeeId { get; private set; }

		public Order()
		{
			
		}

		// Constructor for creating a new order
		public Order(Guid id, Guid productId, int quantity, decimal total, Guid clientId, DateTime orderDate, Guid employeeId)
		{
			Id = id;
			ProductId = productId;
			Quantity = quantity;
			Total = total;
			ClientId = clientId;
			OrderDate = orderDate;
			LoggedInEmployeeId = employeeId;
		}

		// Method to update order details
		public void Update(int quantity, decimal total, DateTime orderDate)
		{
			Quantity = quantity;
			Total = total;
			OrderDate = orderDate;
		}
	}


}