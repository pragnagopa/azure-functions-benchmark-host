﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;

namespace GrpcMessages.Events
{
    public interface IScriptEventManager : IObservable<ScriptEvent>
    {
        void Publish(ScriptEvent scriptEvent);
    }
}
