// <copyright file="ViewTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.App
{
    using NUnit.Framework;

    public class ViewTests
    {
        [Test]
        public void Constructor_PreservesTypedViewModelReference()
        {
            var viewModel = new TestViewModel { Value = 5 };
            var view = new TestView(viewModel);

            Assert.AreSame(viewModel, view.ViewModel);
            Assert.AreEqual(5, view.ViewModel.Value);
        }

        private sealed class TestViewModel
        {
            public int Value { get; set; }
        }

        private sealed class TestView : View<TestViewModel>
        {
            public TestView(TestViewModel viewModel)
                : base(viewModel)
            {
            }
        }
    }
}
