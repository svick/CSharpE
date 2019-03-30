using N.M;

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
            _ = (0);
            _ = checked(unchecked(0));

            _ = +0;
            _ = -0;
            _ = ~0;
            _ = !true;
            --i;
            ++i;
            _ = &i;
            _ = *p;
            i--;
            i++;

            checked { unchecked { } }

            return ref this.i;
        }
    }
}
