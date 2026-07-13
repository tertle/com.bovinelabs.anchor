// <copyright file="AnchorLinearProgressTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_APPUI
namespace BovineLabs.Anchor.Tests.Elements
{
    using BovineLabs.Anchor.Elements;
    using NUnit.Framework;
    using Unity.AppUI.UI;
    using UnityEngine;

    public class AnchorLinearProgressTests
    {
        [Test]
        public void Defaults_AreHorizontalAndUnmasked()
        {
            var progress = new AnchorLinearProgress();

            Assert.AreEqual(Direction.Horizontal, progress.direction);
            Assert.IsNull(progress.maskTexture);
            Assert.IsFalse(progress.ClassListContains(LinearProgress.ussClassName));
            Assert.IsTrue(progress.ClassListContains(AnchorLinearProgress.DirectionUssClassName + "horizontal"));
        }

        [Test]
        public void Direction_UpdatesOrientationClass()
        {
            var progress = new AnchorLinearProgress();

            progress.direction = Direction.Vertical;

            Assert.IsFalse(progress.ClassListContains(AnchorLinearProgress.DirectionUssClassName + "horizontal"));
            Assert.IsTrue(progress.ClassListContains(AnchorLinearProgress.DirectionUssClassName + "vertical"));
        }

        [Test]
        public void MaskTexture_RoundTripsAndClears()
        {
            var progress = new AnchorLinearProgress();
            var mask = new Texture2D(1, 1);

            try
            {
                progress.maskTexture = mask;
                Assert.AreSame(mask, progress.maskTexture);

                progress.maskTexture = null;
                Assert.IsNull(progress.maskTexture);
            }
            finally
            {
                Object.DestroyImmediate(mask);
            }
        }

        [Test]
        public void ShaderResource_IsAvailable()
        {
            var shader = Resources.Load<Shader>("BovineLabs.Anchor/AnchorLinearProgress");

            Assert.IsNotNull(shader);
        }
    }
}
#endif
