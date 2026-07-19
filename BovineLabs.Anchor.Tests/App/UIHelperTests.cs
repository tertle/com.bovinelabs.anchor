// <copyright file="UIHelperTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.App
{
    using System;
    using BovineLabs.Anchor.Binding;
    using BovineLabs.Anchor.Services;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using BovineLabs.Testing;
    using NUnit.Framework;
    using BovineLabs.Anchor.MVVM;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;

    public unsafe class UIHelperTests : ECSTestsFixture
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            BurstObjectNotify.Changed.Clear();
            TestRequireUpdateSystem.UpdateCount = 0;
        }

        [Test]
        public void BindAndUnbind_ManagesViewModelLifecycleAndBurstRegistration()
        {
            using var scope = new TestAnchorAppScope(static services =>
            {
                services.AddSingleton(typeof(IViewModelService), typeof(ViewModelService));
                services.AddSingleton(typeof(TestViewModel));
            });

            var helper = default(UIHelper<TestViewModel, TestData>);
            helper.Bind();

            var viewModelService = scope.ServiceProvider.GetRequiredService<IViewModelService>();
            var viewModel = viewModelService.Get<TestViewModel>();
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(1, viewModel.InitializeCount);

            helper.Binding.Counter = 19;
            Assert.AreEqual(19, viewModel.Value.Counter);

            var key = (IntPtr)UnsafeUtility.AddressOf(ref viewModel.Value);
            Assert.IsTrue(BurstObjectNotify.Changed.ContainsKey(key));

            helper.Unbind();

            Assert.AreEqual(1, viewModel.DisposeCount);
            Assert.IsFalse(BurstObjectNotify.Changed.ContainsKey(key));
            Assert.IsNull(viewModelService.Get<TestViewModel>());
        }

        [Test]
        public void Unbind_AfterAppReplacementReleasesOriginalViewModelOnly()
        {
            var firstScope = new TestAnchorAppScope(RegisterServices);
            var firstHelper = default(UIHelper<TestViewModel, TestData>);
            var secondHelper = default(UIHelper<TestViewModel, TestData>);
            TestAnchorAppScope secondScope = null;
            var firstBound = false;
            var secondBound = false;

            try
            {
                firstHelper.Bind();
                firstBound = true;
                var firstService = firstScope.ServiceProvider.GetRequiredService<IViewModelService>();
                var firstViewModel = firstService.Get<TestViewModel>();
                var firstKey = (IntPtr)UnsafeUtility.AddressOf(ref firstViewModel.Value);

                firstScope.Dispose();

                secondScope = new TestAnchorAppScope(RegisterServices);
                secondHelper.Bind();
                secondBound = true;
                var secondService = secondScope.ServiceProvider.GetRequiredService<IViewModelService>();
                var secondViewModel = secondService.Get<TestViewModel>();
                var secondKey = (IntPtr)UnsafeUtility.AddressOf(ref secondViewModel.Value);

                firstHelper.Unbind();
                firstBound = false;

                Assert.AreEqual(1, firstViewModel.DisposeCount);
                Assert.AreEqual(0, secondViewModel.DisposeCount);
                Assert.IsFalse(BurstObjectNotify.Changed.ContainsKey(firstKey));
                Assert.IsTrue(BurstObjectNotify.Changed.ContainsKey(secondKey));
                Assert.IsNull(firstService.Get<TestViewModel>());
                Assert.AreSame(secondViewModel, secondService.Get<TestViewModel>());

                Assert.DoesNotThrow(() => firstHelper.Unbind());
                Assert.AreEqual(1, firstViewModel.DisposeCount);
            }
            finally
            {
                if (firstBound)
                {
                    firstHelper.Unbind();
                }

                if (secondBound)
                {
                    secondHelper.Unbind();
                }

                secondScope?.Dispose();
                firstScope.Dispose();
            }

            static void RegisterServices(AnchorServiceCollection services)
            {
                services.AddSingleton(typeof(IViewModelService), typeof(ViewModelService));
                services.AddSingleton(typeof(TestViewModel));
            }
        }

        [Test]
        public void ComponentRequirementConstructor_RequiresMatchingComponentBeforeUpdate()
        {
            var system = this.World.CreateSystem<TestRequireUpdateSystem>();

            system.Update(this.WorldUnmanaged);
            Assert.AreEqual(0, TestRequireUpdateSystem.UpdateCount);

            this.Manager.CreateEntity(typeof(TestRequiredComponent));
            system.Update(this.WorldUnmanaged);

            Assert.AreEqual(1, TestRequireUpdateSystem.UpdateCount);
        }

        private sealed class TestViewModel : SystemObservableObject<TestData>, ILoadable
        {
            public int InitializeCount { get; private set; }

            public int DisposeCount { get; private set; }

            public void Load()
            {
                this.InitializeCount++;
            }

            public void Unload()
            {
                this.DisposeCount++;
            }
        }

        private struct TestData
        {
            public int Counter;
        }

        private struct TestRequiredComponent : IComponentData
        {
        }

        private partial struct TestRequireUpdateSystem : ISystem
        {
            public static int UpdateCount;

            public void OnCreate(ref SystemState state)
            {
                _ = new UIHelper<TestViewModel, TestData>(ref state, ComponentType.ReadOnly<TestRequiredComponent>());
            }

            public void OnUpdate(ref SystemState state)
            {
                UpdateCount++;
            }
        }
    }
}
