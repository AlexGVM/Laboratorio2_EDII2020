using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace L2.Helpers
{
    interface IFixedSizeText
    {
        int FixedSizeText { get; }
        string ToFixedLengthString();
    }
}
