[A]
[B]
class C
{
    C[][, ] a;
    C*p;
    public int i;
    public int j;
    event EventHandler e1;
    event EventHandler e2;
    int k;
    void M()
    {
        _ = $@"x{0, 1 + 1:X}";
        _ = "\\";
        new object ();
        _ = typeof(Dictionary<, >);
        new C()
        {P = 42, [0] = {}};
        new C()
        {42, {42, 0}};
        _ = new
        {
        i, x = 42
        }

        ;
        _ = new int[, ][]{{}};
        _ = new[, ]{{0}};
        var(_, x, (y, z)) = t;
        _ = x =>
        {
        }

        ;
        _ = async (ref int x) =>
        {
        }

        ;
        _ = async delegate
        {
        }

        ;
        _ = delegate (int i)
        {
        }

        ;
        _ = 1is 1;
        _ = 1is int i;
        _ =
            from i in null
            orderby i
            group i by i into g
                select i into i
                    select i;
        foreach ((int i, _)in new (int, int)[0])
        {
        }

        foreach (var(i, j)in new (int, int)[0])
        {
        }

        int i = 0;
        int j;
        for (; i < 10; i++)
        {
        }

        i = 0;
        M();
        for (; i < 10; i++)
        {
        }

        do
        {
        }
        while (false);
        fixed (int *p = &new int[1])
        {
        }

        switch (i)
        {
            case 1when false:
                break;
        }

        int i;
        int j = 42;
    }

    int P1
    {
        get;
        private set;
    }

    int I.P2
    {
        get;
        set;
    }

    C(): this(42)
    {
    }

    C(int i): base()
    {
    }
}