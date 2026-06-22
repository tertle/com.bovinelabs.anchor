// <copyright file="AnchorAudioScopeRouterTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Audio
{
    using System.Collections.Generic;
    using BovineLabs.Anchor.Audio;
    using BovineLabs.Anchor.Elements;
    using BovineLabs.Anchor.Nav;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;
    using AppUIButton = Unity.AppUI.UI.Button;
    using UIToolkitButton = UnityEngine.UIElements.Button;

    public class AnchorAudioScopeRouterTests
    {
        private readonly List<AudioClip> clips = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var clip in this.clips)
            {
                Object.DestroyImmediate(clip);
            }

            this.clips.Clear();
        }

        [Test]
        public void ResolveAudioTarget_UnconfiguredAppUiButtonInsideScope_ReturnsNull()
        {
            var (router, host, _) = this.CreateRouter();
            var button = new AppUIButton();
            host.Add(button);

            Assert.IsNull(router.ResolveAudioTargetForTesting(button));
        }

        [Test]
        public void ResolveAudioTarget_UnconfiguredToolkitButtonInsideScope_ReturnsNull()
        {
            var (router, host, _) = this.CreateRouter();
            var button = new UIToolkitButton();
            host.Add(button);

            Assert.IsNull(router.ResolveAudioTargetForTesting(button));
        }

        [Test]
        public void ResolveAudioTarget_ScrollViewInternalScroller_ReturnsNull()
        {
            var (router, host, _) = this.CreateRouter();
            var scrollView = new ScrollView();
            host.Add(scrollView);

            Assert.IsNull(router.ResolveAudioTargetForTesting(scrollView.verticalScroller));
        }

        [Test]
        public void ResolveAudioTarget_AnchorButtonIsTarget()
        {
            var (router, host, _) = this.CreateRouter();
            var button = new AnchorButton();
            host.Add(button);

            Assert.AreSame(button, router.ResolveAudioTargetForTesting(button));
        }

        [Test]
        public void ResolveAudioTarget_NearestExplicitAncestorWins()
        {
            var (router, host, _) = this.CreateRouter();
            var parent = new AnchorButton();
            var child = new AnchorButton();
            var leaf = new VisualElement();
            host.Add(parent);
            parent.Add(child);
            child.Add(leaf);

            Assert.AreSame(child, router.ResolveAudioTargetForTesting(leaf));
        }

        [Test]
        public void ResolveAudioTarget_ExplicitSilentChildPreventsAudibleAncestorSelection()
        {
            var hover = this.CreateClip("hover");
            var (router, host, service) = this.CreateRouter(hoverClip: hover);
            var parent = new AnchorButton();
            var child = new AnchorButton { audioProfile = string.Empty };
            var leaf = new VisualElement();
            host.Add(parent);
            parent.Add(child);
            child.Add(leaf);

            Assert.AreSame(child, router.ResolveAudioTargetForTesting(leaf));

            router.HandlePointerOverForTesting(PointerId.mousePointerId, leaf);

            Assert.IsEmpty(service.PlayedClips);
        }

        [Test]
        public void Hover_RoutesOncePerExplicitLogicalTarget()
        {
            var hover = this.CreateClip("hover");
            var (router, host, service) = this.CreateRouter(hoverClip: hover);
            var button = new AnchorButton();
            var leaf = new VisualElement();
            button.Add(leaf);
            host.Add(button);

            router.HandlePointerOverForTesting(PointerId.mousePointerId, button);
            router.HandlePointerOverForTesting(PointerId.mousePointerId, leaf);
            router.HandlePointerOverForTesting(PointerId.mousePointerId, host.contentContainer);
            router.HandlePointerOverForTesting(PointerId.mousePointerId, button);

            CollectionAssert.AreEqual(new[] { hover, hover }, service.PlayedClips);
        }

        [Test]
        public void Hover_MovingFromExplicitTargetToUnconfiguredPressableClearsHoverState()
        {
            var hover = this.CreateClip("hover");
            var (router, host, service) = this.CreateRouter(hoverClip: hover);
            var anchorButton = new AnchorButton();
            var appUiButton = new AppUIButton();
            host.Add(anchorButton);
            host.Add(appUiButton);

            router.HandlePointerOverForTesting(PointerId.mousePointerId, anchorButton);
            router.HandlePointerOverForTesting(PointerId.mousePointerId, appUiButton);

            CollectionAssert.AreEqual(new[] { hover }, service.PlayedClips);
            Assert.AreEqual(0, router.HoverPointerCount);
        }

        [Test]
        public void Hover_IgnoresTouchPointers()
        {
            var hover = this.CreateClip("hover");
            var (router, host, service) = this.CreateRouter(hoverClip: hover);
            var button = new AnchorButton();
            host.Add(button);

            router.HandlePointerOverForTesting(PointerId.touchPointerIdBase, button);

            Assert.IsEmpty(service.PlayedClips);
        }

        [Test]
        public void Hover_RoutesTrackedPointers()
        {
            var hover = this.CreateClip("hover");
            var (router, host, service) = this.CreateRouter(hoverClip: hover);
            var button = new AnchorButton();
            host.Add(button);

            router.HandlePointerOverForTesting(PointerId.trackedPointerIdBase, button);

            CollectionAssert.AreEqual(new[] { hover }, service.PlayedClips);
        }

        [Test]
        public void ClickFallback_IgnoresExplicitAppUiPressableToAvoidDuplicateAudio()
        {
            var activate = this.CreateClip("activate");
            var (router, host, service) = this.CreateRouter(activateClip: activate);
            var button = new AnchorButton();
            host.Add(button);

            router.HandleClickForTesting(button);

            Assert.IsEmpty(service.PlayedClips);
        }

        [Test]
        public void ClickFallback_PlaysForExplicitNonPressableElement()
        {
            var activate = this.CreateClip("activate");
            var (router, host, service) = this.CreateRouter(activateClip: activate);
            var element = new AudioElement();
            host.Add(element);

            router.HandleClickForTesting(element);

            CollectionAssert.AreEqual(new[] { activate }, service.PlayedClips);
        }

        [Test]
        public void ResolveAudioTarget_RequiresRegisteredScopeAncestry()
        {
            var (router, host, _) = this.CreateRouter();
            var inside = new AnchorButton();
            var outside = new AnchorButton();
            host.Add(inside);

            Assert.AreSame(inside, router.ResolveAudioTargetForTesting(inside));
            Assert.IsNull(router.ResolveAudioTargetForTesting(outside));
        }

        [Test]
        public void DynamicExplicitControl_WorksWithoutRegistrationOrRescan()
        {
            var hover = this.CreateClip("hover");
            var (router, host, service) = this.CreateRouter(hoverClip: hover);
            var element = new AudioElement();
            host.Add(element);

            Assert.AreSame(element, router.ResolveAudioTargetForTesting(element));

            router.HandlePointerOverForTesting(PointerId.mousePointerId, element);

            CollectionAssert.AreEqual(new[] { hover }, service.PlayedClips);
        }

        [Test]
        public void UnregisterScope_ClearsHoverState()
        {
            var hover = this.CreateClip("hover");
            var (router, host, _) = this.CreateRouter(hoverClip: hover);
            var button = new AnchorButton();
            host.Add(button);

            router.HandlePointerOverForTesting(PointerId.mousePointerId, button);
            router.UnregisterScope();

            Assert.IsNull(router.Scope);
            Assert.AreEqual(0, router.HoverPointerCount);
        }

        private (AnchorAudioScopeRouter Router, AnchorNavHost Host, FakeAudioService Service) CreateRouter(
            AudioClip hoverClip = null,
            AudioClip activateClip = null)
        {
            var settings = AnchorAudioSettingsTestUtility.CreateSettings(
                new[]
                {
                    new AnchorAudioProfile
                    {
                        Key = AnchorAudioSettings.DefaultProfileKey,
                        HoverClip = hoverClip,
                        ActivateClip = activateClip,
                    },
                });
            var service = new FakeAudioService();
            var resolver = new AnchorAudioProfileResolver(settings);
            var feedback = new AnchorAudioFeedback(resolver, service);
            var router = new AnchorAudioScopeRouter(feedback);
            var host = new AnchorNavHost();
            router.RegisterScope(host);
            return (router, host, service);
        }

        private sealed class AudioElement : VisualElement, IAnchorAudioElement
        {
            public string AudioProfile { get; set; } = AnchorAudioSettings.DefaultProfileKey;

            public AnchorAudioCueOverride HoverAudio { get; set; } = AnchorAudioCueOverride.Inherit;

            public AnchorAudioCueOverride ActivateAudio { get; set; } = AnchorAudioCueOverride.Inherit;
        }

        private AudioClip CreateClip(string name)
        {
            var clip = AudioClip.Create(name, 8, 1, 8000, false);
            this.clips.Add(clip);
            return clip;
        }

        private sealed class FakeAudioService : IAnchorAudioService
        {
            public List<AudioClip> PlayedClips { get; } = new();

            public void PlayOneShot(AudioClip clip)
            {
                if (clip != null)
                {
                    this.PlayedClips.Add(clip);
                }
            }
        }
    }
}
