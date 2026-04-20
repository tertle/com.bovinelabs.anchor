// <copyright file="UXMLServiceTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Services
{
    using System.Text.RegularExpressions;
    using System.Reflection;
    using BovineLabs.Anchor.Services;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.UIElements;
    using UnityEngine.TestTools;

    public class UXMLServiceTests
    {
        [Test]
        public void GetAsset_ExistingKey_ReturnsExpectedAsset()
        {
            var asset = ScriptableObject.CreateInstance<VisualTreeAsset>();

            try
            {
                WithViews(new[]
                {
                    new AnchorSettings.KeyUXML { Key = "existing", Asset = asset },
                }, () =>
                {
                    var service = new UXMLService();
                    var loaded = service.GetAsset("existing");
                    Assert.AreSame(asset, loaded);
                });
            }
            finally
            {
                Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void GetAsset_MissingKey_ReturnsNull()
        {
            var asset = ScriptableObject.CreateInstance<VisualTreeAsset>();

            try
            {
                WithViews(new[]
                {
                    new AnchorSettings.KeyUXML { Key = "existing", Asset = asset },
                }, () =>
                {
                    var service = new UXMLService();
                    LogAssert.Expect(LogType.Error, new Regex("VisualTreeAsset for the key missing was not found"));
                    var loaded = service.GetAsset("missing");
                    Assert.IsNull(loaded);
                });
            }
            finally
            {
                Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void Instantiate_ExistingKey_ReturnsContainer()
        {
            var asset = ScriptableObject.CreateInstance<VisualTreeAsset>();

            try
            {
                WithViews(new[]
                {
                    new AnchorSettings.KeyUXML { Key = "screen", Asset = asset },
                }, () =>
                {
                    var service = new UXMLService();
                    var element = service.Instantiate("screen");

                    Assert.IsNotNull(element);
                });
            }
            finally
            {
                Object.DestroyImmediate(asset);
            }
        }

        private static void WithViews(AnchorSettings.KeyUXML[] views, System.Action body)
        {
            var settings = AnchorSettings.I;
            var field = typeof(AnchorSettings).GetField("views", BindingFlags.Instance | BindingFlags.NonPublic);
            var original = (AnchorSettings.KeyUXML[])field!.GetValue(settings);

            try
            {
                field.SetValue(settings, views);
                body.Invoke();
            }
            finally
            {
                field.SetValue(settings, original);
            }
        }
    }
}
