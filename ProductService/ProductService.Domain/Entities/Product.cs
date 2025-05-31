namespace ProductService.Domain.Entities;

public class Product
{
	public Guid Id { get; private set; }
	public string Name { get; private set; }
	public decimal Price { get; private set; }

	// Constructor for creating a new product
	public Product(Guid id, string name, decimal price)
	{
		Id = id;
		Name = name;
		Price = price;
	}

	// Method to update product details
	public void Update(string name, decimal price)
	{
		Name = name;
		Price = price;
	}
}
