// <copyright file="MvvmRuntimeTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.App
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using BovineLabs.Anchor.MVVM;
    using NUnit.Framework;
    using Unity.Properties;
    using UnityEngine.UIElements;

    public partial class MvvmRuntimeTests
    {
        [Test]
        public void ObservableObject_SetProperty_RaisesChangingChangedAndBindableEvents()
        {
            var viewModel = new TestViewModel();
            var events = new List<string>();

            viewModel.PropertyChanging += (_, args) => events.Add($"changing:{args.PropertyName}");
            viewModel.PropertyChanged += (_, args) => events.Add($"changed:{args.PropertyName}");
            ((INotifyBindablePropertyChanged)viewModel).propertyChanged += (_, _) => events.Add("bindable");

            viewModel.Value = 42;

            CollectionAssert.AreEqual(
                new[]
                {
                    "changing:Value",
                    "changed:Value",
                    "bindable",
                },
                events);
        }

        [Test]
        public void ObservableObject_SetProperty_WithSameValue_DoesNotRaiseEvents()
        {
            var viewModel = new TestViewModel();
            var eventCount = 0;
            viewModel.SetBackingValue(5);

            viewModel.PropertyChanging += (_, _) => eventCount++;
            viewModel.PropertyChanged += (_, _) => eventCount++;
            ((INotifyBindablePropertyChanged)viewModel).propertyChanged += (_, _) => eventCount++;

            viewModel.Value = 5;

            Assert.AreEqual(0, eventCount);
        }

        [Test]
        public void RelayCommand_RespectsCanExecute()
        {
            var executeCount = 0;
            var command = new RelayCommand(() => executeCount++, () => false);

            Assert.IsFalse(command.CanExecute());
            command.Execute();

            Assert.AreEqual(0, executeCount);
        }

        [Test]
        public void RelayCommand_NotifyCanExecuteChanged_RaisesEvent()
        {
            var command = new RelayCommand(() => { });
            var eventCount = 0;
            command.CanExecuteChanged += (_, _) => eventCount++;

            command.NotifyCanExecuteChanged();

            Assert.AreEqual(1, eventCount);
        }

        [Test]
        public void RelayCommandOfT_InvalidObjectParameter_Throws()
        {
            var command = new RelayCommand<int>(_ => { });

            Assert.IsFalse(command.CanExecute("invalid"));
            Assert.Throws<InvalidOperationException>(() => command.Execute("invalid"));
        }

        [Test]
        public void RelayCommandOfT_ValidParameter_ExecutesWhenAllowed()
        {
            var received = 0;
            var command = new RelayCommand<int>(value => received = value, value => value > 0);

            command.Execute(4);
            command.Execute(-1);

            Assert.AreEqual(4, received);
        }

        [Test]
        public void GeneratedCommand_RespectsCanExecuteMethod()
        {
            var viewModel = new GeneratedCanExecuteViewModel();
            var command = viewModel.IncrementCommand;

            Assert.IsFalse(command.CanExecute(null));
            command.Execute(null);
            Assert.AreEqual(0, viewModel.ExecuteCount);

            viewModel.SetAllowExecute(true);

            Assert.IsTrue(command.CanExecute(null));
            command.Execute(null);
            Assert.AreEqual(1, viewModel.ExecuteCount);
        }

        [Test]
        public void GeneratedCommandWithParameter_RespectsCanExecuteMethod()
        {
            var viewModel = new GeneratedCanExecuteWithParameterViewModel();
            var command = viewModel.SetValueCommand;

            Assert.IsFalse(command.CanExecute(0));
            command.Execute(0);
            Assert.AreEqual(0, viewModel.Value);

            Assert.IsTrue(command.CanExecute(7));
            command.Execute(7);
            Assert.AreEqual(7, viewModel.Value);
        }

        [Test]
        public void GeneratedCommand_WithoutCanExecuteMethod_ExecutesByDefault()
        {
            var viewModel = new GeneratedDefaultCanExecuteViewModel();
            var command = viewModel.IncrementCommand;

            Assert.IsTrue(command.CanExecute(null));
            command.Execute(null);

            Assert.AreEqual(1, viewModel.ExecuteCount);
        }

        [Test]
        public void Generator_IgnoresForeignICommandAttribute()
        {
            var generatedProperty = typeof(ForeignICommandViewModel).GetProperty("RunCommand");

            Assert.IsNull(generatedProperty);
        }

        [Test]
        public void GeneratedObservableProperty_HasExpectedAttributesAndAccessors()
        {
            var generatedProperty = typeof(GeneratedObservablePropertyViewModel).GetProperty(nameof(GeneratedObservablePropertyViewModel.Test));

            Assert.IsNotNull(generatedProperty);
            Assert.IsTrue(Attribute.IsDefined(generatedProperty, typeof(CompilerGeneratedAttribute), false));
            Assert.IsTrue(Attribute.IsDefined(generatedProperty, typeof(CreatePropertyAttribute), false));

            var viewModel = new GeneratedObservablePropertyViewModel();
            viewModel.Test = 7;

            Assert.AreEqual(7, viewModel.Test);
        }

        [Test]
        public void GeneratedObservableProperty_AlsoNotifyChangeFor_RaisesDependentNotificationOnChangeOnly()
        {
            var viewModel = new GeneratedAlsoNotifyViewModel();
            var changedProperties = new List<string>();
            viewModel.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

            viewModel.Test = 4;
            viewModel.Test = 4;

            CollectionAssert.AreEqual(new[] { nameof(GeneratedAlsoNotifyViewModel.Test), nameof(GeneratedAlsoNotifyViewModel.Test2) }, changedProperties);
        }

        [Test]
        public void GeneratedObservableProperty_AlsoExecute_RunsOnChangeOnly()
        {
            var viewModel = new GeneratedAlsoExecuteViewModel();

            viewModel.Test = 2;
            viewModel.Test = 2;

            Assert.AreEqual(1, viewModel.ExecuteCount);
        }

        [Test]
        public void GeneratedObservableProperty_AlsoNotifyAndExecute_RunInOrder()
        {
            var viewModel = new GeneratedNotifyAndExecuteViewModel();
            viewModel.PropertyChanged += (_, args) => viewModel.Log.Add($"changed:{args.PropertyName}");

            viewModel.Test = 9;

            CollectionAssert.AreEqual(
                new[]
                {
                    $"changed:{nameof(GeneratedNotifyAndExecuteViewModel.Test)}",
                    "execute",
                    $"changed:{nameof(GeneratedNotifyAndExecuteViewModel.Test2)}",
                },
                viewModel.Log);
        }

        private sealed class TestViewModel : ObservableObject
        {
            private int value;

            public int Value
            {
                get => this.value;
                set => this.SetProperty(ref this.value, value);
            }

            public void SetBackingValue(int value)
            {
                this.value = value;
            }
        }

        public partial class GeneratedCanExecuteViewModel
        {
            private bool allowExecute;

            [ICommand(CanExecuteMethod = nameof(CanExecuteIncrement))]
            private void Increment()
            {
                this.ExecuteCount++;
            }

            public int ExecuteCount { get; private set; }

            public void SetAllowExecute(bool allowExecute)
            {
                this.allowExecute = allowExecute;
            }

            private bool CanExecuteIncrement()
            {
                return this.allowExecute;
            }
        }

        public partial class GeneratedCanExecuteWithParameterViewModel
        {
            [ICommand(CanExecuteMethod = nameof(CanSetValue))]
            private void SetValue(int value)
            {
                this.Value = value;
            }

            public int Value { get; private set; }

            private bool CanSetValue(int value)
            {
                return value > 0;
            }
        }

        public partial class GeneratedDefaultCanExecuteViewModel
        {
            [ICommand]
            private void Increment()
            {
                this.ExecuteCount++;
            }

            public int ExecuteCount { get; private set; }
        }

        public partial class ForeignICommandViewModel
        {
            [global::BovineLabs.Anchor.Tests.App.FakeMvvm.ICommand(CanExecuteMethod = nameof(CanExecuteRun))]
            private void Run()
            {
            }

            private bool CanExecuteRun()
            {
                return true;
            }
        }

        public partial class GeneratedObservablePropertyViewModel : ObservableObject
        {
            [ObservableProperty]
            private int test;
        }

        public partial class GeneratedAlsoNotifyViewModel : ObservableObject
        {
            [ObservableProperty]
            [AlsoNotifyChangeFor(nameof(Test2))]
            private int test;

            public int Test2 => this.Test * 2;
        }

        public partial class GeneratedAlsoExecuteViewModel : ObservableObject
        {
            [ObservableProperty]
            [AlsoExecute(nameof(OnTestChanged))]
            private int test;

            public int ExecuteCount { get; private set; }

            private void OnTestChanged()
            {
                this.ExecuteCount++;
            }
        }

        public partial class GeneratedNotifyAndExecuteViewModel : ObservableObject
        {
            [ObservableProperty]
            [AlsoNotifyChangeFor(nameof(Test2))]
            [AlsoExecute(nameof(OnTestChanged))]
            private int test;

            public List<string> Log { get; } = new List<string>();

            public int Test2 => this.Test + 1;

            private void OnTestChanged()
            {
                this.Log.Add("execute");
            }
        }
    }
}

namespace BovineLabs.Anchor.Tests.App.FakeMvvm
{
    using System;

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class ICommandAttribute : Attribute
    {
        public string CanExecuteMethod { get; set; }
    }
}
