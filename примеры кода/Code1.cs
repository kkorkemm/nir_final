public abstract class Document : IPrintable {
	public string fileName;
	public int size; // коммент1
}

// комментарий между классами

public class TextDocument : Document { // еще комментарий
	private string[] text;

	// комментарий внутри класса
	public override void save() {}
	public void spellCheck() {}
}

public abstract class Document {
	// этот класс встречается второй раз!
	public void save() {}
	public void saveAs(string filename) {}
	protected bool isModified() {}
}

interface IPrintable {
	public void print() {}
	public void setup(PrintOptions opt) {}
}

public class ExcelDocument : Document, TextDocument {
	public string name;
}

public class Documentation {
	public Document[] documents; // другой комментарий
}

public class PrintOptions {
	public string name;
}