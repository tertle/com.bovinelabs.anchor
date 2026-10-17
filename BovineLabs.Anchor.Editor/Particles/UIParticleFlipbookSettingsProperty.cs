namespace BovineLabs.Anchor.Editor.Particles
{
    using System.Collections.Generic;
    using BovineLabs.Anchor.Particles;
    using BovineLabs.Core.Editor.Inspectors;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;

    [CustomPropertyDrawer(typeof(UIParticleFlipbookSettings))]
    public sealed class UIParticleFlipbookSettingsProperty : ElementProperty
    {
        protected override VisualElement CreateElement(SerializedProperty property)
        {
            var field = base.CreateElement(property);
            Cache<Cache>().Fields[property.name] = field;
            return field;
        }

        protected override void PostElementCreation(VisualElement root, bool createdElements)
        {
            var cache = Cache<Cache>();
            var enabled = RootProperty.FindPropertyRelative(nameof(UIParticleFlipbookSettings.Enabled));
            var mode = RootProperty.FindPropertyRelative(nameof(UIParticleFlipbookSettings.Mode));
            var random = RootProperty.FindPropertyRelative(nameof(UIParticleFlipbookSettings.RandomRow));
            var timing = RootProperty.FindPropertyRelative(nameof(UIParticleFlipbookSettings.Timing));
            void Refresh()
            {
                var active = enabled.hasMultipleDifferentValues || enabled.boolValue;
                foreach (var field in cache.Fields)
                {
                    ElementUtility.SetVisible(field.Value, field.Key == nameof(UIParticleFlipbookSettings.Enabled) || active);
                }

                var row = mode.hasMultipleDifferentValues || mode.enumValueIndex == (int)UIParticleFlipbookMode.SingleRow;
                ElementUtility.SetVisible(cache.Fields[nameof(UIParticleFlipbookSettings.RandomRow)], active && row);
                ElementUtility.SetVisible(cache.Fields[nameof(UIParticleFlipbookSettings.Row)], active && row && (random.hasMultipleDifferentValues || !random.boolValue));
                ElementUtility.SetVisible(cache.Fields[nameof(UIParticleFlipbookSettings.Cycles)], active &&
                    (timing.hasMultipleDifferentValues || timing.enumValueIndex == (int)UIParticleFlipbookTiming.Lifetime));
                ElementUtility.SetVisible(cache.Fields[nameof(UIParticleFlipbookSettings.FramesPerSecond)], active &&
                    (timing.hasMultipleDifferentValues || timing.enumValueIndex == (int)UIParticleFlipbookTiming.FramesPerSecond));
            }

            root.TrackPropertyValue(enabled, _ => Refresh());
            root.TrackPropertyValue(mode, _ => Refresh());
            root.TrackPropertyValue(random, _ => Refresh());
            root.TrackPropertyValue(timing, _ => Refresh());
            Refresh();
        }

        private class Cache
        {
            public readonly Dictionary<string, VisualElement> Fields = new();
        }
    }
}
