﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcMessages.Events
{
    public static class EventSources
    {
        public const string ScriptFiles = "ScriptFiles";
        public const string Rpc = "Rpc";
        public const string Worker = "Worker";
        public const string WorkerProcess = "WorkerProcess";
    }
}
