using System;
using System.Collections.Generic;

namespace AnthStat.Statistics
{
    internal sealed class LookupComparer: IComparer<Lookup>
    {
        public int Compare(Lookup x, Lookup y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            else if (x == null && y != null)
            {
                return -1;
            }
            else if (x != null && y == null)
            {
                return 1;
            }
            else
            {
                return x.CompareTo(y);
            }
        }
    }
}