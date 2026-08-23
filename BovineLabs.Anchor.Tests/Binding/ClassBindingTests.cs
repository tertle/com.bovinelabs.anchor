// <copyright file="ClassBindingTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Binding
{
    using System.Collections;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine.TestTools;
    using UnityEngine.UIElements;

    public class ClassBindingTests
    {
        private const string ActiveClass = "test-active";

        [UnityTest]
        public IEnumerator ClearBindingBeforeDelayedUpdate_DoesNotReapplyClass()
        {
            var window = EditorWindow.CreateWindow<TestWindow>();

            try
            {
                window.Show();

                var element = new VisualElement();
                window.rootVisualElement.Add(element);
                var binding = new TrackingClassBinding
                {
                    Class = ActiveClass,
                    Delay = true,
                    dataSource = true,
                    ClearOnUpdate = true,
                };

                element.SetBinding(nameof(VisualElement.enabledSelf), binding);

                yield return null;

                Assert.Greater(binding.UpdateCalls, 0);
                Assert.IsFalse(element.ClassListContains(ActiveClass));
            }
            finally
            {
                window.Close();
            }
        }

        private class TestWindow : EditorWindow
        {
        }

        private class TrackingClassBinding : ClassBinding
        {
            public bool ClearOnUpdate { get; set; }

            public int UpdateCalls { get; private set; }

            protected override BindingResult Update(in BindingContext context)
            {
                this.UpdateCalls++;
                var result = base.Update(in context);

                if (this.ClearOnUpdate)
                {
                    this.ClearOnUpdate = false;
                    context.targetElement.ClearBinding(nameof(VisualElement.enabledSelf));
                }

                return result;
            }
        }
    }
}
