using System;
using N.M;

namespace N.M
{
    [A(0, arg: 1, P = 2)]
    class C : B
    {
        C c1;
        N.M.C c2;
        C<int> c3;
        int i;
        C? n;
        ref int M()
        {
            _ = 0;
            _ = true;
            _ = false;
            _ = 'c';
            _ = "s";
            _ = $"";
            _ = (1, 2);
            await null;
            _ = A.B;
            _ = this;
            M(42);
            _ = (0);
            _ = checked(unchecked(0));
            _ = true ? 1 : 0;
            _ = base.F;
            _ = this.F;
            i = default;
            _ = default(int);
            _ = typeof(int);
            _ = sizeof(int);
            _ = (int)0;
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
            _ = c1[0];
            _ = c1?[0];
            _ = new int[10];
            _ = stackalloc int[10];
            _ = stackalloc int[]{42};
            _ = stackalloc[]{42};
            _ = () => 0;
            _ = c1 ?? throw new Exception();
            ;
            break;
            continue;
            yield return 42;
            yield break;
            l:
                goto l;
            try
            {
                throw new Exception();
            }
            catch (Exception e)
            {
            }

            checked
            {
                unchecked
                {
                }
            }

            if (true)
            {
            }

            foreach (int i in new int[0])
            {
            }

            foreach (_ in new int[0])
            {
            }

            for (int i = 0; i < 10; i++)
            {
            }

            for (;;)
            {
            }

            while (true)
            {
            }

            using (null)
            {
            }

            using (object o = null)
            {
            }

            unsafe
            {
            }

            lock (o)
            {
            }

            return ref this.i;
        }

        public static C operator !(C c)
        {
        }

        public static implicit operator S(C c)
        {
        }

        ~C()
        {
        }
    }

    struct S
    {
    }

    interface I
    {
    }

    enum E
    {
        M
    }

    delegate void D(in int i);
}