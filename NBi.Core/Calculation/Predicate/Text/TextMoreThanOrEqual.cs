﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBi.Core.Calculation.Predicate.Text
{
    class TextMoreThanOrEqual : AbstractPredicateReference
    {
        public TextMoreThanOrEqual(object reference) : base(reference)
        { }
        public override bool Apply(object x)
        {
            var cpr = StringComparer.Create(CultureInfo.InvariantCulture, false);
            return cpr.Compare(x.ToString(), Reference.ToString()) >= 0;
        }

        public override string ToString()
        {
            return $"is alphabetically after '{Reference}' or equal to it";
        }
    }
}
