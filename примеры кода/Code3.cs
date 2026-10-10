public class ComponentExtender : Component, IExtenderProvider {
    public int index;

    public bool CanExtend(string extendee) {}

    public int GetExtenderIndex(IComponent component) {}

    public void SetExtenderIndex(IComponent component, int index) {}
}

public class LetBlock :AbstractBlock {
    public OperatorLet oper;

    protected override void CalcSize() {}
}

public class Student{}

interface IExtenderProvider {}