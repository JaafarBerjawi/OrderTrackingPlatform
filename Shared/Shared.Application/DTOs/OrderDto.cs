namespace Shared.Application.DTOs
{
	public class OrderDto
	{
		public Guid? Id { get; set; }
		public Guid ProductId { get; set; }
		public int Quantity { get; set; }
		public decimal Total { get; set; }
		public Guid ClientId { get; set; }
		public DateTime OrderDate { get; set; }
		public Guid LoggedInEmployeeId { get; set; }
	}
}
