﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dolstagis.Web.Http
{
    public interface IApplicationInfo
    {
        string Root { get; }

        string PhysicalRoot { get; }
    }
}