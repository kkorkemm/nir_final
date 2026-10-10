public abstract class Университет  
{
	public Ректор ректор;
	public string Название;
}

public class Студент {
	public Кафедра НИР; //// ..?(*?(8)).ывджаб ком
	private Студент староста;
	public Экзамен[] сданныеЭкзамены;
	public Куратор наставник;
	public readonly string Фамилия;
	public abstract string Имя;
	private static string Отчество;
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
	private int ГодРождения;
	public string Предметы;
	public virtual void податьЗаявление(string название, int[] number) {}
	private void проснуться(double время) {}
	private override bool пойтиНаПары() {}
}

public static class Кафедра  
{
	// кдлтлытв
	public string Название;
}

// коммент

public class Экзамен  
{
	public const string Название;
}

interface Куратор  
{
	public required Студент ученик;
}

