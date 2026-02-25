// <copyright file="MvvmRuntimeTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.App
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.MVVM;
    using NUnit.Framework;
    using UnityEngine.UIElements;

    public class MvvmRuntimeTests
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
    }
}
