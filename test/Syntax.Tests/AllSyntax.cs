﻿using N.M;

namespace N.M
{
    class C
    {
        C c1;
        N.M.C c2;
        C<int> c3;
        int i;
        C[][,] a;
        C* p;
        C? n;
        (int i, string) t;

        ref int M()
        {
            _ = (1, 2);
            await null;
            _ = A.B;
            _ = this;
            M(42);
            new object();
        }
    }
}