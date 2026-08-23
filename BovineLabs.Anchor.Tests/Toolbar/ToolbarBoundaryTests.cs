// <copyright file="ToolbarBoundaryTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Toolbar
{
    using System;
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using NUnit.Framework;
    using Unity.Collections;
    using ToolbarService = BovineLabs.Anchor.Debug.Toolbar.Toolbar;

    public class ToolbarBoundaryTests
    {
        [Test]
        public void ActiveTab_AtUtf8Capacity_IsVisibleToEcs()
        {
            var tabName = new string('a', 27) + "é";
            var storage = new TestLocalStorageService();
            var viewModel = new ToolbarViewModel(storage);
            using var toolbar = new ToolbarService(new EmptyServiceProvider(), viewModel, storage, Array.Empty<Type>());

            toolbar.SetActiveTab(tabName);

            Assert.AreEqual(tabName, toolbar.ActiveTabName);
            Assert.AreEqual(tabName, ToolbarViewData.ActiveTab.Data.ToString());
        }

        [Test]
        public void ActiveTab_BeyondUtf8Capacity_IsNotTruncatedForEcs()
        {
            var tabName = new string('a', 28) + "é";
            var storage = new TestLocalStorageService();
            var viewModel = new ToolbarViewModel(storage);
            using var toolbar = new ToolbarService(new EmptyServiceProvider(), viewModel, storage, Array.Empty<Type>());

            toolbar.SetActiveTab(tabName);

            Assert.AreEqual(tabName, toolbar.ActiveTabName);
            Assert.AreEqual(tabName, storage.LastSetStringValue);
            Assert.AreEqual(default(FixedString32Bytes), ToolbarViewData.ActiveTab.Data);
        }

        private sealed class EmptyServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                return null;
            }
        }
    }
}
