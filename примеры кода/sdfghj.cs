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
	private Catalog[] catalogs;
	public Client[] clients;
}

public class Order  
{
	public double сумма;
	public string датавремя;
	public readonly string статусоплаты;
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

public class Product  
{
	private Review[] reviews;
	public int number;
	public string manufacturer;
	public double price;
	public int guarantee;
}

public class VIPClient : Client 
{
	public string Имя;
	public string Фамилия;
	private int скидка;
}

