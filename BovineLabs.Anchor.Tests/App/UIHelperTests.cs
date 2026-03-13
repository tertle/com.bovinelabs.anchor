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
            BurstUIInterop.Changed.Clear();
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
            Assert.IsTrue(BurstUIInterop.Changed.ContainsKey(key));

            helper.Unbind();

            Assert.AreEqual(1, viewModel.DisposeCount);
            Assert.IsFalse(BurstUIInterop.Changed.ContainsKey(key));
            Assert.IsNull(viewModelService.Get<TestViewModel>());
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

        private sealed class TestViewModel : ObservableObject, IBindingObjectNotify<TestData>, ILoadable
        {
            private TestData data;

            public int InitializeCount { get; private set; }

            public int DisposeCount { get; private set; }

            public ref TestData Value => ref this.data;

            public void Load()
            {
                this.InitializeCount++;
            }

            public void Unload()
            {
                this.DisposeCount++;
            }

            public void OnPropertyChanging(in FixedString64Bytes property)
            {
            }

            public void OnPropertyChanged(in FixedString64Bytes property)
            {
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
