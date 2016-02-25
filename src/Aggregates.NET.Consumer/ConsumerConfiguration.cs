﻿using NServiceBus;
using NServiceBus.Configuration.AdvanceExtensibility;
using NServiceBus.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregates
{

    public static class ConsumerConfiguration
    {
        public static void SetEventStoreMaxDegreeOfParallelism( this ExposeSettings settings, Int32 Parallelism)
        {
            settings.GetSettings().Set("SetEventStoreMaxDegreeOfParallelism", Parallelism);
        }
        public static void SetEventStoreCapacity(this ExposeSettings settings, Int32 Capacity)
        {
            settings.GetSettings().Set("SetEventStoreCapacity", Capacity);
        }
    }
}