// <copyright file="NavigationStateSystemTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.App
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using BovineLabs.Core;
    using BovineLabs.Testing;
    using NUnit.Framework;
    using Unity.Entities;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class NavigationStateSystemTests : ECSTestsFixture
    {
        private readonly List<ScriptableObject> createdAssets = new();

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            foreach (var asset in this.createdAssets)
            {
                if (asset != null)
                {
                    Object.DestroyImmediate(asset);
                }
            }

            this.createdAssets.Clear();
        }

        [Test]
        public void OnCreate_BuildsStateMap_FromUISystemTypes()
        {
            this.InitializeUiSystemTypes(("A", StableHash<TestStateA>()));

            using var navHarness = new TestAnchorNavHostHarness();
            navHarness.RegisterScreen("A");
            navHarness.Host.Navigate("A");

            var system = this.World.CreateSystem<NavigationStateSystem>();
            system.Update(this.WorldUnmanaged);

            Assert.IsTrue(this.Manager.HasComponent<TestStateA>(system));
        }

        [Test]
        public void OnUpdate_DestinationUnchanged_IsStable()
        {
            this.InitializeUiSystemTypes(("A", StableHash<TestStateA>()));

            using var navHarness = new TestAnchorNavHostHarness();
            navHarness.RegisterScreen("A");
            navHarness.Host.Navigate("A");

            var system = this.World.CreateSystem<NavigationStateSystem>();
            system.Update(this.WorldUnmanaged);
            system.Update(this.WorldUnmanaged);

            Assert.IsTrue(this.Manager.HasComponent<TestStateA>(system));
        }

        [Test]
        public void OnUpdate_DestinationTransition_RemovesPreviousAndAddsCurrent()
        {
            this.InitializeUiSystemTypes(("A", StableHash<TestStateA>()), ("B", StableHash<TestStateB>()));

            using var navHarness = new TestAnchorNavHostHarness();
            navHarness.RegisterScreen("A");
            navHarness.RegisterScreen("B");
            navHarness.Host.Navigate("A");

            var system = this.World.CreateSystem<NavigationStateSystem>();
            system.Update(this.WorldUnmanaged);

            Assert.IsTrue(this.Manager.HasComponent<TestStateA>(system));
            Assert.IsFalse(this.Manager.HasComponent<TestStateB>(system));

            navHarness.Host.Navigate("B");
            system.Update(this.WorldUnmanaged);

            Assert.IsFalse(this.Manager.HasComponent<TestStateA>(system));
            Assert.IsTrue(this.Manager.HasComponent<TestStateB>(system));
        }

        [Test]
        public void OnUpdate_UnmappedDestination_DoesNotAddComponents()
        {
            this.InitializeUiSystemTypes(("A", StableHash<TestStateA>()));

            using var navHarness = new TestAnchorNavHostHarness();
            navHarness.RegisterScreen("Unmapped");
            navHarness.Host.Navigate("Unmapped");

            var system = this.World.CreateSystem<NavigationStateSystem>();
            system.Update(this.WorldUnmanaged);

            Assert.IsFalse(this.Manager.HasComponent<TestStateA>(system));
        }

        [Test]
        public void OnDestroy_DisposesStateMapWithoutException()
        {
            this.InitializeUiSystemTypes(("A", StableHash<TestStateA>()));

            using var navHarness = new TestAnchorNavHostHarness();
            navHarness.RegisterScreen("A");
            navHarness.Host.Navigate("A");

            var system = this.World.CreateSystem<NavigationStateSystem>();

            Assert.DoesNotThrow(() => this.World.DestroySystem(system));
        }

        private void InitializeUiSystemTypes(params (string State, ulong StableHash)[] entries)
        {
            var settings = ScriptableObject.CreateInstance<UISystemTypes>();
            this.createdAssets.Add(settings);

            var navigationComponents = new UISystemTypes.NavigationComponent[entries.Length];
            for (var i = 0; i < entries.Length; i++)
            {
                var componentAsset = ScriptableObject.CreateInstance<ComponentAsset>();
                this.createdAssets.Add(componentAsset);

                SetField(componentAsset, "component", entries[i].StableHash);

                navigationComponents[i] = new UISystemTypes.NavigationComponent
                {
                    States = new[] { entries[i].State },
                    Component = componentAsset,
                };
            }

            SetField(settings, "types", navigationComponents);
            InvokeInitialize(settings);
        }

        private static void InvokeInitialize(UISystemTypes settings)
        {
            var method = typeof(UISystemTypes).GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new MissingMethodException(typeof(UISystemTypes).FullName, "Initialize");
            }

            method.Invoke(settings, null);
        }

        private static void SetField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(instance.GetType().FullName, fieldName);
            }

            field.SetValue(instance, value);
        }

        private static ulong StableHash<T>()
            where T : unmanaged, IComponentData
        {
            return TypeManager.GetTypeInfo<T>().StableTypeHash;
        }

        private struct TestStateA : IComponentData
        {
        }

        private struct TestStateB : IComponentData
        {
        }
    }
}
