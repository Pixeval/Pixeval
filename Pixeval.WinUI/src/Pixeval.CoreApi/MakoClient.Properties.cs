using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading;
using Autofac;
using Pixeval.CoreApi.Engines;
using Pixeval.CoreApi.Preference;

namespace Pixeval.CoreApi
{
    public partial class MakoClient
    {
        public Guid Id { get; } = Guid.NewGuid();

        public Session Session { get; private set; }

        public MakoClientConfiguration Configuration { get; set; }

        /// <summary>
        /// 正在执行的所有实例
        /// </summary>
        private readonly List<IEngineHandleSource> _runningInstances = new();

        public CancellationTokenSource CancellationTokenSource { get; set; }
        
        internal ISessionUpdate SessionUpdater { get; }
        
        internal IContainer MakoServices { get; init; }

        internal static MemoryCache? MemoryCache { get; }
    }
}