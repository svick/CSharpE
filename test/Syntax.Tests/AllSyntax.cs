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
            _ = (0);
            _ = checked(unchecked(0));

            _ = +-~0;
            _ = !true;
            --i;
            ++i;
            _ = *&i;
            i--;
            i++;

            _ = 1 + 1 - 1 * 1 / 1 % 1 << 1 >> 1 ^ 1;
            _ = true || true && true | true & true;
            _ = true == true != true < true <= true > true >= true;
            _ = 1 is int;
            _ = 1 as int;
            _ = null ?? 0;

            i = 1;
            i += 1;
            i -= 1;
            i *= 1;
            i /= 1;
            i %= 1;
            i &= 1;
            i ^= 1;
            i |= 1;
            i <<= 1;
            i >>= 1;

            _ = c1.M;
            _ = c1->M;
            _ = c1?.M;

            new C { P = 42, [0] = { } };
            new C { 42, { 42, 0 } };

            checked { unchecked { } }

            return ref this.i;
        }
    }
}
