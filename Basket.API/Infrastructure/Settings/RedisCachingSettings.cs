using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.API.Infrastructure.Settings
{
    public class RedisCachingSettings
    {
        public bool IsEnabled { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }
    }
}
