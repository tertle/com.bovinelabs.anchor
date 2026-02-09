// <copyright file="TestVisualElementFactory.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.TestDoubles
{
    using System;
    using System.Collections.Generic;
    using UnityEngine.UIElements;

    internal sealed class TestVisualElementFactory
    {
        private readonly Dictionary<string, Func<VisualElement>> factories = new(StringComparer.Ordinal);

        public void Register(string destination, Func<VisualElement> factory)
        {
            this.factories[destination] = factory;
        }

        public VisualElement Create(string destination)
        {
            if (this.factories.TryGetValue(destination, out var factory))
            {
                return factory.Invoke();
            }

            var container = new VisualElement { name = destination };
            return container;
        }
    }
}
