﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that have an <see cref="INavigationBase"/>.
    /// </summary>
    public abstract class NavigationBaseEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="navigationBase"> The navigation. </param>
        protected NavigationBaseEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] INavigationBase navigationBase)
            : base(eventDefinition, messageGenerator)
        {
        }

        /// <summary>
        ///     The navigation.
        /// </summary>
        public abstract INavigationBase NavigationBase { get; }
    }
}
