[typ: A(), B]
class C
{
    C[][,] a;
    C* p;
    public int i, j;
    event EventHandler e1, e2;
    global::System.Int32 k;
    void M()
    {
        _ = $@"x{0,1+1:X}";
        _ = @"\";
        new object();
        _ = typeof(Dictionary<,>);
        new C { P = 42, [0] = { } };
        new C { 42, { 42, 0 } };
        _ = new { i, x = 42 };
        _ = new int[,][] { { } };
        _ = new[,] { { 0 } };
        var (_, x, (y, z)) = t;
        _ = x => { };
        _ = async (ref int x) => { };
        _ = async delegate { };
        _ = delegate (int i) { };
        _ = 1 is 1;
        _ = 1 is int i;
        _ = null!;
        _ =
            from i in null
            orderby i ascending
            group i by i into g
            select i into i
            select i;
        _ = 42 switch { _ => 0 };
        foreach ((int i, _) in new (int, int)[0])
        {
        }

        foreach (var (i, j) in new (int, int)[0])
        {
        }

        for (int i = 0, j; i < 10; i++)
        {
        }

        for (i = 0, M(); i < 10; i++)
        {
        }

        do
        {
        } while (false);

        fixed (int* p = &new int[1])
        {
        }

        switch (i)
        {
            case 1 when false:
                break;
            case C(arg1: 0, 1, _) { Prop: { } } c:
                break;
        }

        int i, j = 42;
    }

    int P1 { get; private set; }

    int I.P2 { get; set; }

    int P3 => 42;

    int P4 { get; } = 42;

    C() : this(42)
    {
    }

    C(int i) : base()
    {
    }
}

enum E
{
    [A, B]
    M
}