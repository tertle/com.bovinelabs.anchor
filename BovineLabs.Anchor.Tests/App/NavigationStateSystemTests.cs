// <copyright file="NavigationStateSystemTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.App
{
    using System;
    using System.Reflection;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using BovineLabs.Core;
    using BovineLabs.Testing;
    using NUnit.Framework;
    using Unity.Entities;
    using UnityEngine;

    public class NavigationStateSystemTests : ECSTestsFixture
    {
        private ComponentAsset componentAsset;
        private UISystemTypes settings;

        [TearDown]
        public override void TearDown()
        {
            try
            {
                base.TearDown();
            }
            finally
            {
                var emptySettings = ScriptableObject.CreateInstance<UISystemTypes>();
                InitializeSettings(emptySettings, Array.Empty<UISystemTypes.NavigationComponent>());

                UnityEngine.Object.DestroyImmediate(emptySettings);
                UnityEngine.Object.DestroyImmediate(this.settings);
                UnityEngine.Object.DestroyImmediate(this.componentAsset);
            }
        }

        [Test]
        public void DestinationChanges_GateConsumerWithoutPerFrameStructuralChanges()
        {
            this.componentAsset = ScriptableObject.CreateInstance<ComponentAsset>();
            SetField(this.componentAsset, "component", TypeManager.GetTypeInfo<TestNavigationState>().StableTypeHash);

            this.settings = ScriptableObject.CreateInstance<UISystemTypes>();
            InitializeSettings(this.settings, new[]
            {
                new UISystemTypes.NavigationComponent
                {
                    States = new[] { "game", "game-secondary" },
                    Component = this.componentAsset,
                },
            });

            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("game");
            harness.RegisterScreen("game-secondary");

            var system = this.World.CreateSystem<NavigationStateSystem>();
            var consumer = this.World.CreateSystem<TestNavigationConsumerSystem>();
            var componentType = ComponentType.ReadOnly<TestNavigationState>();

            Assert.IsFalse(this.Manager.HasComponent(system, componentType));
            consumer.Update(this.WorldUnmanaged);
            AssertConsumerState(consumer, starts: 0, updates: 0, stops: 0);

            harness.Host.Navigate("game");
            system.Update(this.WorldUnmanaged);
            consumer.Update(this.WorldUnmanaged);

            Assert.IsTrue(this.Manager.HasComponent(system, componentType));
            AssertConsumerState(consumer, starts: 1, updates: 1, stops: 0);
            var activeOrderVersion = this.Manager.EntityOrderVersion;

            system.Update(this.WorldUnmanaged);
            consumer.Update(this.WorldUnmanaged);

            Assert.AreEqual(activeOrderVersion, this.Manager.EntityOrderVersion);
            AssertConsumerState(consumer, starts: 1, updates: 2, stops: 0);

            harness.Host.Navigate("game-secondary");
            system.Update(this.WorldUnmanaged);
            consumer.Update(this.WorldUnmanaged);

            Assert.IsTrue(this.Manager.HasComponent(system, componentType));
            Assert.AreEqual(activeOrderVersion, this.Manager.EntityOrderVersion);
            AssertConsumerState(consumer, starts: 1, updates: 3, stops: 0);

            harness.Host.ClearNavigation();
            system.Update(this.WorldUnmanaged);
            consumer.Update(this.WorldUnmanaged);

            Assert.IsFalse(this.Manager.HasComponent(system, componentType));
            Assert.Greater(this.Manager.EntityOrderVersion, activeOrderVersion);
            AssertConsumerState(consumer, starts: 1, updates: 3, stops: 1);
        }

        private void AssertConsumerState(SystemHandle consumer, int starts, int updates, int stops)
        {
            var consumerState = this.Manager.GetComponentData<TestNavigationConsumerState>(consumer);
            Assert.AreEqual(starts, consumerState.Starts);
            Assert.AreEqual(updates, consumerState.Updates);
            Assert.AreEqual(stops, consumerState.Stops);
        }

        private static void InitializeSettings(UISystemTypes target, UISystemTypes.NavigationComponent[] navigationComponents)
        {
            SetField(target, "types", navigationComponents);

            var initialize = typeof(UISystemTypes).GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(initialize);
            initialize.Invoke(target, null);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            field.SetValue(target, value);
        }

        private struct TestNavigationState : IComponentData
        {
        }

        private struct TestNavigationConsumerState : IComponentData
        {
            public int Starts;
            public int Updates;
            public int Stops;
        }

        private partial struct TestNavigationConsumerSystem : ISystem, ISystemStartStop
        {
            public void OnCreate(ref SystemState state)
            {
                state.EntityManager.AddComponentData(state.SystemHandle, default(TestNavigationConsumerState));
                state.RequireForUpdate<TestNavigationState>();
            }

            public void OnUpdate(ref SystemState state)
            {
                var consumerState = state.EntityManager.GetComponentData<TestNavigationConsumerState>(state.SystemHandle);
                consumerState.Updates++;
                state.EntityManager.SetComponentData(state.SystemHandle, consumerState);
            }

            public void OnStartRunning(ref SystemState state)
            {
                var consumerState = state.EntityManager.GetComponentData<TestNavigationConsumerState>(state.SystemHandle);
                consumerState.Starts++;
                state.EntityManager.SetComponentData(state.SystemHandle, consumerState);
            }

            public void OnStopRunning(ref SystemState state)
            {
                var consumerState = state.EntityManager.GetComponentData<TestNavigationConsumerState>(state.SystemHandle);
                consumerState.Stops++;
                state.EntityManager.SetComponentData(state.SystemHandle, consumerState);
            }
        }
    }
}
