using System;
using static System.Console;
using Foo = System.Console;

namespace N.M
{
    [A(0, arg: 1, P = 2)]
    class C<in T1, out T2, T3> : B where T1 : class, object, new()
        where T2 : struct
    {
        C c1;
        A.B.C c2;
        C<int> c3;
        int i;
        C? n;
        (int i, string) t;
        ref int M<T>()
            where T : object
        {
            WriteLine();
            Foo.WriteLine();
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
            M(42, foo: 13);
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
            i ??= 1;
            _ = ..;
            _ = 1..;
            _ = ..2;
            _ = 1..2;
            _ = ^0;
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
            _ =
                from i in null
                from i2 in null
                let ii = i * i
                where true
                orderby i, i2 descending
                join j in null on i equals j
                join j2 in null on i equals j2 into g
                group i by i2;
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

            switch (i)
            {
                case 0:
                    goto default;
                case string s:
                    break;
                case var x:
                    break;
                case 1:
                default:
                    goto case 0;
            }

            async void F<T>(int i)
                where T : new()
            {
                return;
            }

            return ref this.i;
        }

        void I.F()
        {
        }

        int P1
        {
            get
            {
            }

            set
            {
            }
        }

        public int this[int i]
        {
            get
            {
            }
        }

        int I.this[int i]
        {
            get
            {
            }

            set
            {
            }
        }

        event EventHandler E1
        {
            add
            {
            }

            remove
            {
            }
        }

        event I.EventHandler E2
        {
            add
            {
            }

            remove
            {
            }
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

    delegate void D<T>(in int i)
        where T : class;
}