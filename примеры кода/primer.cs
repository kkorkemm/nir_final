public class Университет  
{
	public Ректор[] ректорs;
	public string Название;
}

public class Ректор  
{
	public string Отчество;
	public string Имя;
	private int ГодРождения;
	public string Фамилия;
}

public class Студент  
{
	public Студент староста;
	private Экзамен[] сданныеЭкзамены;
	public Кафедра НИР;
	public Куратор наставник;
	public string Фамилия;
	public string Имя;
	public string Отчество;
	public readonly int ГодРождения = 2004;
	public string Предметы;
	public void податьЗаявление(string название) {}
	private bool пойтиНаПары() {}
	private void проснуться(double время) {}
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

