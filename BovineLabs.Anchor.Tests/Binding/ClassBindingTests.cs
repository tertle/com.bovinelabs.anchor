// <copyright file="ClassBindingTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Binding
{
    using System;
    using System.Linq;
    using System.Reflection;
    using BovineLabs.Anchor;
    using NUnit.Framework;
    using Unity.Properties;
    using UnityEngine.UIElements;

    public class ClassBindingTests
    {
        [Test]
        public void Update_EmptyClassName_Fails()
        {
            var binding = new TestableClassBinding
            {
                Class = " ",
            };

            var context = CreateBindingContext(new VisualElement(), true, new PropertyPath());

            var result = binding.InvokeUpdate(context);

            Assert.AreEqual(BindingStatus.Failure, result.status);
        }

        [Test]
        public void Update_NullDataSource_IsPending()
        {
            var element = new VisualElement();
            var binding = new TestableClassBinding
            {
                Class = "enabled",
            };

            var context = CreateBindingContext(element, null, new PropertyPath());
            var result = binding.InvokeUpdate(context);

            Assert.AreEqual(BindingStatus.Pending, result.status);
            Assert.IsFalse(element.ClassListContains("enabled"));
        }

        [Test]
        public void Update_BooleanAndConvertibleSources_ToggleClass()
        {
            var element = new VisualElement();
            var binding = new TestableClassBinding
            {
                Class = "enabled",
            };

            var boolResult = binding.InvokeUpdate(CreateBindingContext(element, true, new PropertyPath()));
            Assert.AreEqual(BindingStatus.Success, boolResult.status);
            Assert.IsTrue(element.ClassListContains("enabled"));

            var convertibleResult = binding.InvokeUpdate(CreateBindingContext(element, 0, new PropertyPath()));
            Assert.AreEqual(BindingStatus.Success, convertibleResult.status);
            Assert.IsFalse(element.ClassListContains("enabled"));
        }

        [Test]
        public void Update_InvalidSourceType_Fails()
        {
            var element = new VisualElement();
            var binding = new TestableClassBinding
            {
                Class = "enabled",
            };

            var result = binding.InvokeUpdate(CreateBindingContext(element, new object(), new PropertyPath()));

            Assert.AreEqual(BindingStatus.Failure, result.status);
            Assert.IsFalse(element.ClassListContains("enabled"));
        }

        [Test]
        public void OnDeactivated_RemovesClassFromTarget()
        {
            var element = new VisualElement();
            element.AddToClassList("enabled");

            var binding = new TestableClassBinding
            {
                Class = "enabled",
            };

            var activation = CreateActivationContext(element);
            binding.InvokeDeactivate(activation);

            Assert.IsFalse(element.ClassListContains("enabled"));
        }

        private static BindingContext CreateBindingContext(VisualElement target, object dataSource, PropertyPath dataSourcePath)
        {
            var boxed = CreateContextInstance(
                typeof(BindingContext),
                ("targetElement", target),
                ("dataSource", dataSource),
                ("dataSourcePath", dataSourcePath));
            return (BindingContext)boxed;
        }

        private static BindingActivationContext CreateActivationContext(VisualElement target)
        {
            var boxed = CreateContextInstance(typeof(BindingActivationContext), ("targetElement", target));
            return (BindingActivationContext)boxed;
        }

        private static object CreateContextInstance(Type contextType, params (string Name, object Value)[] values)
        {
            foreach (var ctor in contextType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                         .OrderByDescending(c => c.GetParameters().Length))
            {
                var parameters = ctor.GetParameters();
                var arguments = new object[parameters.Length];

                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    if (TryGetNamedValue(parameter.Name, values, out var named))
                    {
                        arguments[i] = named;
                        continue;
                    }

                    if (parameter.HasDefaultValue)
                    {
                        arguments[i] = parameter.DefaultValue;
                        continue;
                    }

                    arguments[i] = parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null;
                }

                var constructed = ctor.Invoke(arguments);
                ApplyValues(contextType, constructed, values);
                return constructed;
            }

            var fallback = Activator.CreateInstance(contextType);
            ApplyValues(contextType, fallback, values);
            return fallback;
        }

        private static void ApplyValues(Type contextType, object target, (string Name, object Value)[] values)
        {
            foreach (var (name, value) in values)
            {
                var field = contextType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(f => MatchesName(f.Name, name));
                if (field != null)
                {
                    field.SetValue(target, value);
                    continue;
                }

                var property = contextType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(p => p.CanWrite && MatchesName(p.Name, name));
                property?.SetValue(target, value);
            }
        }

        private static bool TryGetNamedValue(string parameterName, (string Name, object Value)[] values, out object value)
        {
            foreach (var (name, parameterValue) in values)
            {
                if (!MatchesName(parameterName, name))
                {
                    continue;
                }

                value = parameterValue;
                return true;
            }

            value = null;
            return false;
        }

        private static bool MatchesName(string candidate, string expected)
        {
            if (string.Equals(candidate, expected, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (candidate.StartsWith("m_", StringComparison.Ordinal))
            {
                return string.Equals(candidate[2..], expected, StringComparison.OrdinalIgnoreCase);
            }

            if (candidate.StartsWith("<", StringComparison.Ordinal) && candidate.Contains(">"))
            {
                var end = candidate.IndexOf('>');
                var propertyName = candidate[1..end];
                return string.Equals(propertyName, expected, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private sealed class TestableClassBinding : ClassBinding
        {
            public BindingResult InvokeUpdate(BindingContext context)
            {
                return this.Update(in context);
            }

            public void InvokeDeactivate(BindingActivationContext context)
            {
                this.OnDeactivated(in context);
            }
        }
    }
}
