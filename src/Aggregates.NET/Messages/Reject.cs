﻿using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregates.Messages
{
    public class Reject : IMessage
    {
        public String Message { get; set; }
    }
}
