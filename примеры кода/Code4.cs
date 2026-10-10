public class Category  
{
	public Product[] products;
}

public class Catalog  
{
	public Category[] categorys;
}

public class Store  
{
	public Catalog[] catalogs;
	public Client[] clients;
}

public class Product  
{
	public Review[] reviews;
	public int number;
	public string manufacturer;
	public double price;
	public int guarantee;
}

public class Order  
{
	public double сумма = 10000;
	public string дата_время;
	public readonly string статус_оплаты;
	public string доставка;
}

public class Client  
{
	public Order[] orders;
	public string Имя;
	public string Фамилия;
}

public class Review  
{
	public Client client;
}

public class VIPClient : Client 
{
	public string Имя;
	public string Фамилия;
	private int скидка;
}

