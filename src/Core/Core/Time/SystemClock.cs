﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dolstagis.Core.Time
{
    public class SystemClock : IClock
    {
        public DateTime Now()
        {
            return DateTime.Now;
        }

        public DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}
