[A(), B]
class C
{
    C[][,] a;
    C* p;
    (int i, string) t;
    public int i, j;
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

        int i, j = 42;
    }

    int P1 { get; set; }

    int I.P2 { get; set; }

    C() : this(42)
    {
    }

    C(int i) : base()
    {
    }
}