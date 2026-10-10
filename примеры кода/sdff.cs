public class FeedBack  
{
	private Customer user;
}

public class Product  
{
	public FeedBack[] reviews;
	public int serialNUm;
	public readonly string producer;
	public readonly int guarantee;
	public readonly double price;
	public FeedBack getFeedBack() {}
	public void display() {}
}

public class Order  
{
	private Product[] lines;
	public double totalSum;
	public string dateTime;
	public readonly string paid;
	public int accureBonuses() {}
	public void setDelivery() {}
	public void applyPromoCode(string code) {}
	public void change(Product prod) {}
}

public class Customer  
{
	private Order[] orderHistory;
	public readonly string phoneNumer;
	public readonly string nickName;
	protected int bonuses;
	protected void userPromo() {}
	public uint payTheBill(Order o) {}
}

public class VIPCustomer : Customer 
{
	private int personalDiscount;
}

