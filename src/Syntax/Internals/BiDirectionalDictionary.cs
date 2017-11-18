using System;
using System.Collections;
using System.Collections.Generic;

namespace CSharpE.Syntax.Internals
{
    // https://stackoverflow.com/a/8105057/41071
    public class BiDirectionalDictionary<TLeft, TRight> : IEnumerable<KeyValuePair<TLeft, TRight>>
    {
        private readonly Dictionary<TLeft, TRight> leftToRight = new Dictionary<TLeft, TRight>();
        private readonly Dictionary<TRight, TLeft> rightToLeft = new Dictionary<TRight, TLeft>();

        public void Add(TLeft leftSide, TRight rightSide)
        {
            if (leftToRight.ContainsKey(leftSide) ||
                rightToLeft.ContainsKey(rightSide))
                throw new ArgumentException();
            leftToRight.Add(leftSide, rightSide);
            rightToLeft.Add(rightSide, leftSide);
        }

        public TLeft this[TRight rightSideKey] => rightToLeft[rightSideKey];
        public TRight this[TLeft leftSideKey] => leftToRight[leftSideKey];

        public bool ContainsKey(TLeft leftSideKey) => leftToRight.ContainsKey(leftSideKey);
        public bool ContainsKey(TRight rightSideKey) => rightToLeft.ContainsKey(rightSideKey);


        public IEnumerator<KeyValuePair<TLeft, TRight>> GetEnumerator() => leftToRight.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}