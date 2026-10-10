public class Category  
{
	private Product[] products;
}

public class Catalog  
{
	private Category[] categorys;
}

public class Store  
{
	public Catalog[] catalogs;
	private Client[] clients;
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
	private Order[] orders;
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

