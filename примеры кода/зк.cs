public class Университет  
{
	public Ректор[] ректорs;
	public string Название;
}

public class Ректор  
{
	public string Фамилия;
	public string Отчество;
	public string Имя;
	private int ГодРождения;
}

public class Студент  
{
	public Кафедра НИР;
	public Студент староста;
	private Экзамен[] сданныеЭкзамены;
	public Куратор наставник;
	public string Фамилия;
	public string Имя;
	public string Отчество;
	private readonly int ГодРождения = 2004;
	public string Предметы;
	public void податьЗаявление(string название) {}
	private void проснуться(double время) {}
	private bool пойтиНаПары() {}
}

public static class Кафедра  
{
	// Зависимость
	public void ConnectionMethod()
	{
		Экзамен экзамен = new Экзамен();
	}
	public string Название;
}

public class Экзамен  
{
	public string Название;
}

public class Куратор : Ректор 
{
	public Студент ученик;
}

