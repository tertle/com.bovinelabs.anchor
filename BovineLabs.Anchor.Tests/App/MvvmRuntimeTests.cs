namespace BovineLabs.Anchor.Tests.App
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.MVVM;
    using NUnit.Framework;
    using Unity.Properties;
    using UnityEngine.UIElements;
    using BoundSystemProperty = BovineLabs.Anchor.SystemPropertyAttribute;

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
        public void RelayCommand_PropertyChangedSource_RaisesCanExecuteChangedForObservedProperty()
        {
            var viewModel = new TestViewModel();
            var command = new RelayCommand(() => { }, () => true, viewModel, nameof(TestViewModel.Value));
            var eventCount = 0;
            command.CanExecuteChanged += (_, _) => eventCount++;

            viewModel.OtherValue = 3;
            viewModel.Value = 4;

            Assert.AreEqual(1, eventCount);
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
            Assert.IsFalse(command.CanExecute("invalid"));
            Assert.Throws<InvalidOperationException>(() => command.Execute("invalid"));
        }

        [Test]
        public void GeneratedCommand_RespectsCanExecuteProperty()
        {
            var viewModel = new GeneratedCanExecutePropertyViewModel();
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

        [Test]
        public void PartialObservableProperty_BindingNotifiesAndRunsCallbacksOnlyWhenChanged()
        {
            var viewModel = new GeneratedObservablePropertyViewModel();
            var changed = new List<string>();
            viewModel.PropertyChanged += (_, args) => changed.Add(args.PropertyName);
            var bindableEvents = 0;
            ((INotifyBindablePropertyChanged)viewModel).propertyChanged += (_, _) => bindableEvents++;

            Assert.IsTrue(PropertyContainer.TryGetValue(ref viewModel, nameof(viewModel.Test), out int initial));
            Assert.AreEqual(4, initial);
            Assert.IsTrue(PropertyContainer.TrySetValue(ref viewModel, nameof(viewModel.Test), 9));
            Assert.IsTrue(PropertyContainer.TrySetValue(ref viewModel, nameof(viewModel.Test), 9));

            Assert.AreEqual(18, viewModel.Doubled);
            Assert.AreEqual(1, viewModel.ExecuteCount);
            CollectionAssert.AreEqual(new[] { nameof(viewModel.Test), nameof(viewModel.Doubled) }, changed);
            Assert.AreEqual(2, bindableEvents);
        }

        [Test]
        public void PartialObservableProperty_InitValueRespectsReadOnlyBinding()
        {
            var viewModel = new GeneratedObservablePropertyViewModel { InitialValue = 8 };

            Assert.IsTrue(PropertyContainer.TryGetValue(ref viewModel, nameof(viewModel.InitialValue), out int actual));
            Assert.AreEqual(8, actual);
            Assert.IsFalse(PropertyContainer.TrySetValue(ref viewModel, nameof(viewModel.InitialValue), 9));
            Assert.AreEqual(8, viewModel.InitialValue);
        }

        [Test]
        public void SystemProperty_AttributeAlias_GeneratesWorkingAccessors()
        {
            var data = new AliasedSystemPropertyData { Value = 42 };

            Assert.AreEqual(42, data.Value);
        }

        [Test]
        public void GeneratedDependsOn_RaisesDependentNotificationsTransitively()
        {
            var viewModel = new GeneratedDependsOnViewModel();
            var changedProperties = new List<string>();
            viewModel.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

            viewModel.HasSave = true;

            CollectionAssert.AreEqual(
                new[]
                {
                    nameof(GeneratedDependsOnViewModel.HasSave),
                    nameof(GeneratedDependsOnViewModel.CanLoad),
                    nameof(GeneratedDependsOnViewModel.CanContinue),
                },
                changedProperties);
        }

        [Test]
        public void GeneratedDependsOn_ObservablePropertyFieldName_RaisesGeneratedPropertyDependency()
        {
            var viewModel = new GeneratedFieldDependsOnViewModel();
            var changedProperties = new List<string>();
            viewModel.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

            viewModel.Test = 4;

            CollectionAssert.AreEqual(
                new[] { nameof(GeneratedFieldDependsOnViewModel.Test), nameof(GeneratedFieldDependsOnViewModel.HasTest) },
                changedProperties);
        }

        [Test]
        public void GeneratedCommand_CanExecutePropertyObservesDependentNotifications()
        {
            var viewModel = new GeneratedCommandDependsOnViewModel();
            var command = viewModel.ContinueCommand;
            var eventCount = 0;
            command.CanExecuteChanged += (_, _) => eventCount++;

            Assert.IsFalse(command.CanExecute(null));

            viewModel.HasSave = true;

            Assert.AreEqual(1, eventCount);
            Assert.IsTrue(command.CanExecute(null));
        }

        public partial struct AliasedSystemPropertyData
        {
            [BoundSystemProperty]
            private int _value;
        }

        private sealed class TestViewModel : ObservableObject
        {
            private int _value;
            private int _otherValue;

            public int Value
            {
                get => _value;
                set => SetProperty(ref _value, value);
            }

            public int OtherValue
            {
                get => _otherValue;
                set => SetProperty(ref _otherValue, value);
            }

            public void SetBackingValue(int value)
            {
                _value = value;
            }
        }

        public partial class GeneratedCanExecuteViewModel
        {
            private bool _allowExecute;

            [ICommand(CanExecuteMethod = nameof(CanExecuteIncrement))]
            private void Increment()
            {
                ExecuteCount++;
            }

            public int ExecuteCount { get; private set; }

            public void SetAllowExecute(bool allowExecute)
            {
                _allowExecute = allowExecute;
            }

            private bool CanExecuteIncrement()
            {
                return _allowExecute;
            }
        }

        public partial class GeneratedCanExecuteWithParameterViewModel
        {
            [ICommand(CanExecuteMethod = nameof(CanSetValue))]
            private void SetValue(int value)
            {
                Value = value;
            }

            public int Value { get; private set; }

            private bool CanSetValue(int value)
            {
                return value > 0;
            }
        }

        public partial class GeneratedCanExecutePropertyViewModel
        {
            private bool _allowExecute;

            [ICommand(CanExecuteProperty = nameof(CanIncrement))]
            private void Increment()
            {
                ExecuteCount++;
            }

            [CreateProperty(ReadOnly = true)]
            public bool CanIncrement => _allowExecute;

            public int ExecuteCount { get; private set; }

            public void SetAllowExecute(bool allowExecute)
            {
                _allowExecute = allowExecute;
            }
        }

        public partial class GeneratedObservablePropertyViewModel : ObservableObject
        {
            [ObservableProperty]
            [AlsoNotifyChangeFor(nameof(Doubled))]
            [AlsoExecute(nameof(CountChange))]
            public partial int Test
            {
                get;
                set;
            } = 4;

            [ObservableProperty]
            [CreateProperty(ReadOnly = true)]
            public partial int InitialValue
            {
                get;
                init;
            }

            public int Doubled => Test * 2;

            public int ExecuteCount { get; private set; }

            private void CountChange()
            {
                ExecuteCount++;
            }
        }

        public partial class GeneratedDependsOnViewModel : ObservableObject
        {
            [ObservableProperty]
            public partial bool HasSave
            {
                get;
                set;
            }

            [DependsOn(nameof(HasSave))]
            public bool CanLoad => HasSave;

            [DependsOn(nameof(CanLoad))]
            public bool CanContinue => CanLoad;
        }

        public partial class GeneratedFieldDependsOnViewModel : ObservableObject
        {
            [ObservableProperty]
            private int _test;

            [CreateProperty]
            [DependsOn(nameof(_test))]
            public bool HasTest => _test != 0;
        }

        public partial class GeneratedCommandDependsOnViewModel : ObservableObject
        {
            [ObservableProperty]
            public partial bool HasSave
            {
                get;
                set;
            }

            [ICommand(CanExecuteProperty = nameof(CanContinue))]
            private void Continue()
            {
                ExecuteCount++;
            }

            [DependsOn(nameof(HasSave))]
            private bool CanContinue => HasSave;

            public int ExecuteCount { get; private set; }
        }

        public partial class GeneratedAlsoNotifyViewModel : ObservableObject
        {
            [ObservableProperty]
            [AlsoNotifyChangeFor(nameof(Test2))]
            private int _test;

            public int Test2 => Test * 2;
        }

        public partial class GeneratedAlsoExecuteViewModel : ObservableObject
        {
            [ObservableProperty]
            [AlsoExecute(nameof(OnTestChanged))]
            private int _test;

            public int ExecuteCount { get; private set; }

            private void OnTestChanged()
            {
                ExecuteCount++;
            }
        }

        public partial class GeneratedNotifyAndExecuteViewModel : ObservableObject
        {
            [ObservableProperty]
            [AlsoNotifyChangeFor(nameof(Test2))]
            [AlsoExecute(nameof(OnTestChanged))]
            private int _test;

            public List<string> Log { get; } = new List<string>();

            public int Test2 => Test + 1;

            private void OnTestChanged()
            {
                Log.Add("execute");
            }
        }
    }
}
