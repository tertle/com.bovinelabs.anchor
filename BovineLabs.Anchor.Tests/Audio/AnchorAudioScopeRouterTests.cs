// <copyright file="AnchorAudioScopeRouterTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Audio
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Audio;
    using BovineLabs.Anchor.Elements;
    using BovineLabs.Anchor.Nav;
    using NUnit.Framework;
    using Unity.AppUI.UI;
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
        public void ResolveAudioTarget_RequiresRegisteredScopeAncestry()
        {
            var (router, host, _) = this.CreateRouter();
            var inside = new AppUIButton();
            var outside = new AppUIButton();
            host.Add(inside);

            Assert.AreSame(inside, router.ResolveAudioTargetForTesting(inside));
            Assert.IsNull(router.ResolveAudioTargetForTesting(outside));
        }

        [Test]
        public void ResolveAudioTarget_NearestSupportedAncestorWins()
        {
            var (router, host, _) = this.CreateRouter();
            var parent = new AppUIButton();
            var child = new AnchorButton();
            var leaf = new VisualElement();
            host.Add(parent);
            parent.Add(child);
            child.Add(leaf);

            Assert.AreSame(child, router.ResolveAudioTargetForTesting(leaf));
        }

        [Test]
        public void Hover_RoutesOncePerLogicalTarget()
        {
            var hover = this.CreateClip("hover");
            var (router, host, service) = this.CreateRouter(hoverClip: hover);
            var button = new AppUIButton();
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
        public void Hover_IgnoresTouchPointers()
        {
            var hover = this.CreateClip("hover");
            var (router, host, service) = this.CreateRouter(hoverClip: hover);
            var button = new AppUIButton();
            host.Add(button);

            router.HandlePointerOverForTesting(PointerId.touchPointerIdBase, button);

            Assert.IsEmpty(service.PlayedClips);
        }

        [Test]
        public void ClickFallback_PlaysForStandardToolkitButton()
        {
            var activate = this.CreateClip("activate");
            var (router, host, service) = this.CreateRouter(activateClip: activate);
            var button = new UIToolkitButton();
            host.Add(button);

            router.HandleClickForTesting(button);

            CollectionAssert.AreEqual(new[] { activate }, service.PlayedClips);
        }

        [Test]
        public void ClickFallback_IgnoresAppUiPressableToAvoidDuplicateAudio()
        {
            var activate = this.CreateClip("activate");
            var (router, host, service) = this.CreateRouter(activateClip: activate);
            var button = new AppUIButton();
            host.Add(button);

            router.HandleClickForTesting(button);

            Assert.IsEmpty(service.PlayedClips);
        }

        [Test]
        public void UnregisterScope_ClearsHoverState()
        {
            var hover = this.CreateClip("hover");
            var (router, host, _) = this.CreateRouter(hoverClip: hover);
            var button = new AppUIButton();
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
            var settings = new AnchorAudioSettings(
                new AnchorAudioCueProfile { HoverClip = hoverClip, ActivateClip = activateClip },
                Array.Empty<AnchorAudioProfile>());
            var service = new FakeAudioService();
            var resolver = new AnchorAudioProfileResolver(settings);
            var feedback = new AnchorAudioFeedback(resolver, service);
            var router = new AnchorAudioScopeRouter(feedback);
            var host = new AnchorNavHost();
            router.RegisterScope(host);
            return (router, host, service);
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
