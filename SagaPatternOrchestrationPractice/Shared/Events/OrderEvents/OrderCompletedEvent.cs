﻿using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events.OrderEvents
{
    public class OrderCompletedEvent
    {
        public int OrderId { get; set; }    
    }
}
