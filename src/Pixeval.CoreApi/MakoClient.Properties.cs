#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/MakoClient.Properties.cs
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using Autofac;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Preference;

namespace Pixeval.CoreApi
{
    public partial class MakoClient
    {
        private readonly List<IEngineHandleSource> _runningInstances = new();

        /// <summary>
        ///     The globally unique ID of current <see cref="MakoClient" />
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        public Session Session { get; private set; }

        public MakoClientConfiguration Configuration { get; set; }

        internal ISessionUpdate SessionUpdater { get; }

        /// <summary>
        ///     The IoC container
        /// </summary>
        internal IContainer MakoServices { get; init; }

        /// <summary>
        ///     The <see cref="CancellationTokenSource" /> that is used to cancel ths <see cref="MakoClient" />\
        ///     and all of its running engines
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; set; }
    }
}