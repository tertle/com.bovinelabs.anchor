// <copyright file="AnchorNavHost.StateAndAnimations.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Unity.AppUI.Navigation;
    using Unity.AppUI.UI;
    using UnityEngine;
    using UnityEngine.UIElements;
    using UnityEngine.UIElements.Experimental;

    public partial class AnchorNavHost
    {
        /// <summary> No animation. </summary>
        private static readonly AnimationDescription NoneAnimation = new()
        {
            easing = Easing.Linear,
            durationMs = 0,
            callback = null,
        };

        /// <summary> Scale down and fade in animation. </summary>
        private static readonly AnimationDescription ScaleDownFadeInAnimation = new()
        {
            easing = Easing.OutCubic,
            durationMs = 150,
            callback = (v, f) =>
            {
                var delta = 1.2f - (f * 0.2f);
                v.style.scale = new Scale(new Vector3(delta, delta, 1));
                v.style.opacity = f;
            },
        };

        /// <summary> Scale up and fade out animation. </summary>
        private static readonly AnimationDescription ScaleUpFadeOutAnimation = new()
        {
            easing = Easing.OutCubic,
            durationMs = 150,
            callback = (v, f) =>
            {
                var delta = 1.0f + (f * 0.2f);
                v.style.scale = new Scale(new Vector3(delta, delta, 1));
                v.style.opacity = 1.0f - f;
            },
        };

        /// <summary> Fade in animation. </summary>
        private static readonly AnimationDescription FadeInAnimation = new()
        {
            easing = Easing.OutCubic,
            durationMs = 500,
            callback = (v, f) => v.style.opacity = f,
        };

        /// <summary> Fade out animation. </summary>
        private static readonly AnimationDescription FadeOutAnimation = new()
        {
            easing = Easing.OutCubic,
            durationMs = 500,
            callback = (v, f) => v.style.opacity = 1.0f - f,
        };

        /// <summary>
        /// Captures the current visual stack, back stack, and popup configuration so it can be restored later.
        /// </summary>
        /// <returns>A snapshot containing the navigation state that can be supplied to <see cref="RestoreState"/>.</returns>
        public AnchorNavHostSaveState SaveState()
        {
            var activeItems = new List<AnchorNavHostSaveState.StackItem>(this.activeStack.Count);

            foreach (var entry in this.activeStack)
            {
                var savedItem = CreateSavedStackItem(entry);
                if (savedItem != null)
                {
                    activeItems.Add(savedItem);
                }
            }

            var backStackEntries = new List<AnchorNavHostSaveState.BackStackEntry>(this.backStack.Count);
            foreach (var entry in this.backStack.Reverse())
            {
                var savedEntry = CreateSavedBackStackEntry(entry);
                if (savedEntry != null)
                {
                    backStackEntries.Add(savedEntry);
                }
            }

            return new AnchorNavHostSaveState(
                this.currentDestination,
                this.currentPopEnterAnimation,
                this.currentPopExitAnimation,
                activeItems,
                backStackEntries);
        }

        /// <summary>
        /// Restores a previously captured navigation snapshot.
        /// </summary>
        /// <param name="state">Snapshot created via <see cref="SaveState"/>.</param>
        public void RestoreState(AnchorNavHostSaveState state)
        {
            if (state == null)
            {
                return;
            }

            this.CancelRunningAnimations();

            while (this.activeStack.Count > 0)
            {
                this.RemoveActiveEntryAt(this.activeStack.Count - 1, NavigationAnimation.None);
            }

            this.backStack.Clear();

            if (state.BackStack != null)
            {
                foreach (var savedEntry in state.BackStack)
                {
                    var snapshot = CreateSnapshotFromSaved(savedEntry.Snapshot);
                    var entry = new AnchorNavBackStackEntry(
                        savedEntry.Destination,
                        savedEntry.Options?.Clone(),
                        savedEntry.Arguments?.ToArray() ?? Array.Empty<Argument>(),
                        snapshot);
                    this.backStack.Push(entry);
                }
            }

            var activeSnapshot = CreateSnapshotFromSaved(state.ActiveStack);
            var topOptions = activeSnapshot.Top?.Options;
            this.ApplySnapshot(activeSnapshot, NavigationAnimation.None, NavigationAnimation.None, topOptions);

            this.currentPopEnterAnimation = state.CurrentPopEnterAnimation;
            this.currentPopExitAnimation = state.CurrentPopExitAnimation;
            this.CurrentDestination = state.CurrentDestination;
        }

        private static AnchorNavHostSaveState.StackItem CreateSavedStackItem(AnchorNavActiveEntry entry)
        {
            if (entry == null)
            {
                return null;
            }

            return new AnchorNavHostSaveState.StackItem(entry.Destination, entry.Options, entry.Arguments, entry.IsPopup);
        }

        private static AnchorNavHostSaveState.StackItem CreateSavedStackItem(AnchorNavStackItem item)
        {
            if (item == null)
            {
                return null;
            }

            return new AnchorNavHostSaveState.StackItem(item.Destination, item.Options, item.Arguments, item.IsPopup);
        }

        private static AnchorNavHostSaveState.BackStackEntry CreateSavedBackStackEntry(AnchorNavBackStackEntry entry)
        {
            if (entry == null)
            {
                return null;
            }

            var snapshotItems = entry.Snapshot?.Items ?? Array.Empty<AnchorNavStackItem>();
            var savedSnapshot = new List<AnchorNavHostSaveState.StackItem>(snapshotItems.Count);

            foreach (var item in snapshotItems)
            {
                var savedItem = CreateSavedStackItem(item);
                if (savedItem != null)
                {
                    savedSnapshot.Add(savedItem);
                }
            }

            return new AnchorNavHostSaveState.BackStackEntry(entry.Destination, entry.Options, entry.Arguments, savedSnapshot);
        }

        private static AnchorNavStackSnapshot CreateSnapshotFromSaved(IReadOnlyList<AnchorNavHostSaveState.StackItem> items)
        {
            if (items == null || items.Count == 0)
            {
                return AnchorNavStackSnapshot.Empty;
            }

            var stackItems = new List<AnchorNavStackItem>(items.Count);
            foreach (var item in items)
            {
                var stackItem = CreateStackItem(item);
                if (stackItem != null)
                {
                    stackItems.Add(stackItem);
                }
            }

            if (stackItems.Count == 0)
            {
                return AnchorNavStackSnapshot.Empty;
            }

            return new AnchorNavStackSnapshot(stackItems);
        }

        private static AnchorNavStackItem CreateStackItem(AnchorNavHostSaveState.StackItem item)
        {
            if (item == null)
            {
                return null;
            }

            var options = item.Options?.Clone();
            var arguments = item.Arguments?.ToArray() ?? Array.Empty<Argument>();
            return new AnchorNavStackItem(item.Destination, options, arguments, item.IsPopup);
        }

        private static AnimationDescription GetAnimationFunc(NavigationAnimation anim)
        {
            switch (anim)
            {
                case NavigationAnimation.ScaleDownFadeIn:
                    return ScaleDownFadeInAnimation;
                case NavigationAnimation.ScaleUpFadeOut:
                    return ScaleUpFadeOutAnimation;
                case NavigationAnimation.FadeIn:
                    return FadeInAnimation;
                case NavigationAnimation.FadeOut:
                    return FadeOutAnimation;
                case NavigationAnimation.None:
                default:
                    return NoneAnimation;
            }
        }

        private bool TryPlayAnimation(VisualElement element, NavigationAnimation animation, Action onCompleted)
        {
            var description = GetAnimationFunc(animation);
            if (description is { durationMs: <= 0, callback: null })
            {
                onCompleted?.Invoke();
                return false;
            }

            var handleInfo = new AnchorNavAnimationHandle(element, description, onCompleted);
            var handle = element.experimental.animation
                .Start(0, 1, description.durationMs, description.callback)
                .Ease(description.easing)
                .OnCompleted(() =>
                {
                    if (!handleInfo.TryFinalizeFromAnimation())
                    {
                        return;
                    }

                    onCompleted?.Invoke();
                    this.runningAnimations.Remove(handleInfo);
                })
                .KeepAlive();

            handleInfo.Handle = handle;
            this.runningAnimations.Add(handleInfo);
            return true;
        }

        private void CancelRunningAnimations()
        {
            if (this.runningAnimations.Count == 0)
            {
                return;
            }

            foreach (var animation in this.runningAnimations)
            {
                animation.CompleteImmediately();
            }

            this.runningAnimations.Clear();
        }

        private void CompleteAnimationsFor(VisualElement element)
        {
            for (var i = this.runningAnimations.Count - 1; i >= 0; i--)
            {
                var handle = this.runningAnimations[i];
                if (handle.Element != element)
                {
                    continue;
                }

                handle.CompleteImmediately();
                this.runningAnimations.RemoveAt(i);
            }
        }
    }
}
