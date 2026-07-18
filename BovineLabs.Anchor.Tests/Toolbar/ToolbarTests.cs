// <copyright file="ToolbarTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_APPUI
namespace BovineLabs.Anchor.Tests.Toolbar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using BovineLabs.Anchor.Binding;
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using BovineLabs.Core.Utility;
    using NUnit.Framework;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine;
    using UnityEngine.UIElements;
    using ToolbarService = BovineLabs.Anchor.Debug.Toolbar.Toolbar;

    public unsafe class ToolbarTests
    {
        private const string ShowRibbonKey = "bl.show-ribbon";
        private const string LifecycleSaveKey = "bl.toolbar.Lifecycle.Model";

        [Test]
        public void CreateRootTwice_RetainsRegistrationModelPointerAndLifecycle()
        {
            PlayerPrefs.DeleteKey(LifecycleSaveKey);

            var seed = new TestBindingModel { PersistentValue = 17 };
            PlayerPrefs.SetString(LifecycleSaveKey, JsonUtility.ToJson(seed));

            var storage = CreateVisibleStorage();
            var viewModel = new ToolbarViewModel(storage);
            using var toolbar = new ToolbarService(new TestServiceProvider(), viewModel, storage, Array.Empty<Type>());

            try
            {
                var handle = toolbar.Register<TestBindingModel, TestBindingModel.Data>(
                    "Lifecycle",
                    "Model",
                    out var data);

                var firstRoot = (ToolbarView)toolbar.CreateRootVisualElement();
                var firstElement = firstRoot.Q<TestToolbarElement>();
                var model = (TestBindingModel)firstElement.dataSource;

                Assert.AreEqual(17, model.PersistentValue);
                Assert.AreEqual((IntPtr)data, GetAddress(model));
                Assert.AreEqual(1, model.PinCalls);
                Assert.AreEqual(1, model.LoadCalls);
                Assert.AreEqual(1, model.ElementCreateCalls);
                Assert.AreEqual(1, model.ActiveSubscriptions);

                model.PersistentValue = 29;

                var secondRoot = (ToolbarView)toolbar.CreateRootVisualElement();
                var secondElement = secondRoot.Q<TestToolbarElement>();

                Assert.AreNotSame(firstRoot, secondRoot);
                Assert.AreNotSame(firstElement, secondElement);
                Assert.IsNull(firstElement.dataSource);
                Assert.AreSame(model, secondElement.dataSource);
                Assert.AreEqual((IntPtr)data, GetAddress(model));
                Assert.AreEqual(29, model.PersistentValue);
                Assert.AreEqual(1, model.PinCalls);
                Assert.AreEqual(1, model.LoadCalls);
                Assert.AreEqual(2, model.ElementCreateCalls);
                Assert.AreEqual(1, model.ActiveSubscriptions);

                model.RaiseChanged();
                Assert.AreEqual(1, model.VisualCallbackCalls);

                Assert.IsTrue(toolbar.Remove(handle));
                Assert.IsFalse(toolbar.Remove(handle));
                Assert.AreEqual(1, model.UnloadCalls);
                Assert.AreEqual(1, model.UnpinCalls);
                Assert.AreEqual(0, model.ActiveSubscriptions);
                Assert.IsNull(secondElement.dataSource);

                model.RaiseChanged();
                Assert.AreEqual(1, model.VisualCallbackCalls);

                var restored = new TestBindingModel();
                JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(LifecycleSaveKey), restored);
                Assert.AreEqual(29, restored.PersistentValue);
            }
            finally
            {
                PlayerPrefs.DeleteKey(LifecycleSaveKey);
            }
        }

        [Test]
        public void DuplicateRegistrations_AreIndependent()
        {
            var storage = CreateVisibleStorage();
            var viewModel = new ToolbarViewModel(storage);
            using var toolbar = new ToolbarService(new TestServiceProvider(), viewModel, storage, Array.Empty<Type>());

            var firstHandle = toolbar.Register<TestBindingModel, TestBindingModel.Data>("World", "First", out var firstData);
            toolbar.Register<TestBindingModel, TestBindingModel.Data>("World", "Second", out var secondData);
            var root = (ToolbarView)toolbar.CreateRootVisualElement();
            var models = root.Query<TestToolbarElement>().ToList().Select(element => (TestBindingModel)element.dataSource).ToArray();
            var firstModel = models.Single(model => GetAddress(model) == (IntPtr)firstData);
            var secondModel = models.Single(model => GetAddress(model) == (IntPtr)secondData);

            Assert.AreNotSame(firstModel, secondModel);
            Assert.AreNotEqual((IntPtr)firstData, (IntPtr)secondData);

            Assert.IsTrue(toolbar.Remove(firstHandle));
            Assert.IsFalse(toolbar.Remove(firstHandle));
            Assert.AreEqual(1, firstModel.UnloadCalls);
            Assert.AreEqual(1, firstModel.UnpinCalls);
            Assert.AreEqual(0, secondModel.UnloadCalls);
            Assert.AreEqual(0, secondModel.UnpinCalls);
            Assert.AreSame(secondModel, root.Q<TestToolbarElement>().dataSource);

            toolbar.Dispose();
            Assert.AreEqual(1, secondModel.UnloadCalls);
            Assert.AreEqual(1, secondModel.UnpinCalls);
        }

        [Test]
        public void CreatingSecondToolbarWhileFirstIsActive_Throws()
        {
            var firstStorage = CreateVisibleStorage();
            var firstViewModel = new ToolbarViewModel(firstStorage);
            using var toolbar = new ToolbarService(new TestServiceProvider(), firstViewModel, firstStorage, Array.Empty<Type>());

            var secondStorage = CreateVisibleStorage();
            var secondViewModel = new ToolbarViewModel(secondStorage);

            Assert.Throws<InvalidOperationException>(
                () => new ToolbarService(new TestServiceProvider(), secondViewModel, secondStorage, Array.Empty<Type>()));
        }

        [Test]
        public void AutoToolbarDiscovery_RunsOnceAcrossVisualRecreation()
        {
            var storage = CreateVisibleStorage();
            var viewModel = new ToolbarViewModel(storage);
            var autoModel = new TestAutoToolbarModel<int> { State = 41 };
            var serviceProvider = new TestServiceProvider();
            serviceProvider.Add(autoModel);

            var toolbar = new ToolbarService(serviceProvider, viewModel, storage, new[] { typeof(TestAutoToolbarModel<int>) });

            try
            {
                Assert.AreEqual(1, serviceProvider.ResolveCalls);
                Assert.AreEqual(1, autoModel.LoadCalls);
                CollectionAssert.AreEqual(new[] { "Auto" }, viewModel.FilterItems);

                var firstRoot = (ToolbarView)toolbar.CreateRootVisualElement();
                var firstElement = firstRoot.Q<TestScheduledToolbarElement<int>>();
                var secondRoot = (ToolbarView)toolbar.CreateRootVisualElement();
                var secondElement = secondRoot.Q<TestScheduledToolbarElement<int>>();

                Assert.AreNotSame(firstRoot, secondRoot);
                Assert.AreNotSame(firstElement, secondElement);
                Assert.IsNull(firstElement.dataSource);
                Assert.AreSame(autoModel, secondElement.dataSource);
                Assert.AreEqual(41, autoModel.State);
                Assert.AreEqual(2, autoModel.ElementCreateCalls);
                Assert.AreEqual(2, autoModel.SchedulesCreated);
                Assert.AreEqual(1, autoModel.LoadCalls);
                Assert.AreEqual(1, serviceProvider.ResolveCalls);
                CollectionAssert.AreEqual(new[] { "Auto" }, viewModel.FilterItems);
            }
            finally
            {
                toolbar.Dispose();
            }

            Assert.AreEqual(1, autoModel.UnloadCalls);
            Assert.AreEqual(0, viewModel.FilterItems.Count);
        }

        [Test]
        public void ProductionAutoToolbarDiscovery_DoesNotIncludeTestFixture()
        {
            var discoveredTypes = ReflectionUtility.GetAllWithAttribute<AutoToolbarAttribute>();

            CollectionAssert.DoesNotContain(discoveredTypes, typeof(TestAutoToolbarModel<>));
        }

        private static TestLocalStorageService CreateVisibleStorage()
        {
            var storage = new TestLocalStorageService();
            storage.SetValue(ShowRibbonKey, true);
            return storage;
        }

        private static IntPtr GetAddress(TestBindingModel model)
        {
            return (IntPtr)UnsafeUtility.AddressOf(ref model.Value);
        }

        [Serializable]
        public sealed class TestBindingModel : IToolbarElement, IBindingObjectNotify<TestBindingModel.Data>, ILoadable
        {
            [SerializeField]
            private DataBox data = new();

            [NonSerialized]
            private GCHandle pin;

            public event Action Changed;

            public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

            public int PinCalls { get; private set; }

            public int UnpinCalls { get; private set; }

            public int LoadCalls { get; private set; }

            public int UnloadCalls { get; private set; }

            public int ElementCreateCalls { get; private set; }

            public int ActiveSubscriptions { get; set; }

            public int VisualCallbackCalls { get; set; }

            public int PersistentValue
            {
                get => this.data.Value.PersistentValue;
                set => this.data.Value.PersistentValue = value;
            }

            public ref Data Value => ref this.data.Value;

            public VisualElement CreateElement()
            {
                this.ElementCreateCalls++;
                return new TestToolbarElement(this);
            }

            public void Load()
            {
                this.LoadCalls++;
            }

            public void Unload()
            {
                this.UnloadCalls++;
            }

            public void RaiseChanged()
            {
                this.Changed?.Invoke();
            }

            public void OnPropertyChanging(in FixedString64Bytes property)
            {
            }

            public void OnPropertyChanged(in FixedString64Bytes property)
            {
                this.propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property.ToString()));
            }

            void IBindingObjectNotify<Data>.Pin()
            {
                this.pin = GCHandle.Alloc(this.data, GCHandleType.Pinned);
                this.PinCalls++;
            }

            void IBindingObjectNotify<Data>.Unpin()
            {
                this.pin.Free();
                this.pin = default;
                this.UnpinCalls++;
            }

            [Serializable]
            private sealed class DataBox
            {
                public Data Value;
            }

            [Serializable]
            public struct Data
            {
                public int PersistentValue;
            }
        }

        public sealed class TestToolbarElement : VisualElement, IDisposable
        {
            private TestBindingModel model;

            public TestToolbarElement(TestBindingModel model)
            {
                this.model = model;
                this.model.Changed += this.OnChanged;
                this.model.ActiveSubscriptions++;
            }

            public void Dispose()
            {
                if (this.model == null)
                {
                    return;
                }

                this.model.Changed -= this.OnChanged;
                this.model.ActiveSubscriptions--;
                this.model = null;
            }

            private void OnChanged()
            {
                this.model.VisualCallbackCalls++;
            }
        }

        [IsService]
        [AutoToolbar("Auto", "Managed")]
        public sealed class TestAutoToolbarModel<T> : IToolbarElement, ILoadable
        {
            public int State { get; set; }

            public int LoadCalls { get; private set; }

            public int UnloadCalls { get; private set; }

            public int ElementCreateCalls { get; private set; }

            public int SchedulesCreated { get; private set; }

            public VisualElement CreateElement()
            {
                this.ElementCreateCalls++;
                this.SchedulesCreated++;
                return new TestScheduledToolbarElement<T>();
            }

            public void Load()
            {
                this.LoadCalls++;
            }

            public void Unload()
            {
                this.UnloadCalls++;
            }
        }

        private sealed class TestScheduledToolbarElement<T> : VisualElement, IDisposable
        {
            private readonly IVisualElementScheduledItem scheduledItem;

            public TestScheduledToolbarElement()
            {
                this.scheduledItem = this.schedule.Execute(this.UpdateModel).Every(1);
            }

            public void Dispose()
            {
                this.scheduledItem.Pause();
            }

            private void UpdateModel()
            {
                _ = ((TestAutoToolbarModel<T>)this.dataSource).State;
            }
        }

        private sealed class TestServiceProvider : IServiceProvider
        {
            private readonly Dictionary<Type, object> services = new();

            public int ResolveCalls { get; private set; }

            public object GetService(Type serviceType)
            {
                this.ResolveCalls++;
                this.services.TryGetValue(serviceType, out var service);
                return service;
            }

            public void Add<T>(T service)
                where T : class
            {
                this.services.Add(typeof(T), service);
            }
        }
    }
}
#endif
