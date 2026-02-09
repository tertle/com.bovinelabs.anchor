// <copyright file="ConvertersTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Binding
{
    using System;
    using System.Linq;
    using System.Reflection;
    using BovineLabs.Anchor.Binding;
    using NUnit.Framework;
    using UnityEngine.UIElements;

    public class ConvertersTests
    {
        [Test]
        public void RegisterConverters_RepeatedCalls_AreSafe()
        {
            Assert.DoesNotThrow(() =>
            {
                Converters.RegisterConverters();
                Converters.RegisterConverters();
            });
        }

        [Test]
        public void RegisterConverters_RegistersExpectedGroups()
        {
            Converters.RegisterConverters();

            Assert.IsTrue(TryGetGroup("DisplayStyle", out _));
            Assert.IsTrue(TryGetGroup("DisplayStyleInverted", out _));
            Assert.IsTrue(TryGetGroup("Invert", out _));
        }

        [Test]
        public void ConverterGroups_PerformExpectedConversions()
        {
            Converters.RegisterConverters();

            Assert.IsTrue(TryGetGroup("DisplayStyle", out var displayGroup));
            Assert.IsTrue(TryConvert(displayGroup, true, out StyleEnum<DisplayStyle> style));
            Assert.AreEqual(DisplayStyle.Flex, style.value);
            Assert.IsTrue(TryConvert(displayGroup, new StyleEnum<DisplayStyle>(DisplayStyle.None), out bool visible));
            Assert.IsFalse(visible);

            Assert.IsTrue(TryGetGroup("DisplayStyleInverted", out var invertedGroup));
            Assert.IsTrue(TryConvert(invertedGroup, true, out StyleEnum<DisplayStyle> invertedStyle));
            Assert.AreEqual(DisplayStyle.None, invertedStyle.value);

            Assert.IsTrue(TryGetGroup("Invert", out var invertGroup));
            Assert.IsTrue(TryConvert(invertGroup, true, out bool invertedBool));
            Assert.IsFalse(invertedBool);
        }

        private static bool TryGetGroup(string name, out ConverterGroup group)
        {
            var methods = typeof(ConverterGroups).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var method = methods.FirstOrDefault(m =>
            {
                if (!m.Name.Contains("TryGet", StringComparison.Ordinal) || m.ReturnType != typeof(bool))
                {
                    return false;
                }

                var parameters = m.GetParameters();
                return parameters.Length == 2 &&
                       parameters[0].ParameterType == typeof(string) &&
                       parameters[1].IsOut &&
                       parameters[1].ParameterType.GetElementType() == typeof(ConverterGroup);
            });

            if (method == null)
            {
                group = null;
                return false;
            }

            object[] args = { name, null };
            var success = (bool)method.Invoke(null, args);
            group = (ConverterGroup)args[1];
            return success;
        }

        private static bool TryConvert<TInput, TOutput>(ConverterGroup group, TInput input, out TOutput output)
        {
            var methods = typeof(ConverterGroup).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var method = methods.FirstOrDefault(m =>
            {
                if (!m.Name.Contains("TryConvert", StringComparison.Ordinal))
                {
                    return false;
                }

                var candidate = m.IsGenericMethodDefinition ? m.MakeGenericMethod(typeof(TInput), typeof(TOutput)) : m;
                var parameters = candidate.GetParameters();
                return parameters.Length == 2 &&
                       parameters[0].ParameterType == typeof(TInput).MakeByRefType() &&
                       parameters[1].IsOut &&
                       parameters[1].ParameterType == typeof(TOutput).MakeByRefType();
            });

            if (method == null)
            {
                output = default;
                return false;
            }

            if (method.IsGenericMethodDefinition)
            {
                method = method.MakeGenericMethod(typeof(TInput), typeof(TOutput));
            }

            object boxedInput = input;
            object[] args = { boxedInput, null };
            var success = (bool)method.Invoke(group, args);

            output = success ? (TOutput)args[1] : default;
            return success;
        }
    }
}
