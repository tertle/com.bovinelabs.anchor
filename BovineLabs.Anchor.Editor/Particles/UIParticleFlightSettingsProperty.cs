namespace BovineLabs.Anchor.Editor.Particles
{
    using System.Collections.Generic;
    using BovineLabs.Anchor.Particles;
    using BovineLabs.Core.Editor.Inspectors;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;

    [CustomPropertyDrawer(typeof(UIParticleFlightSettings))]
    public sealed class UIParticleFlightSettingsProperty : ElementProperty
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
            var enabled = RootProperty.FindPropertyRelative(nameof(UIParticleFlightSettings.Enabled));
            var hint = new HelpBox("Requires a Panel-space particle control and a target in the same panel. Scatter + flight duration replaces ordinary lifetime. " +
                "Follow reshapes the remaining curve when the target moves; arrival feedback is cosmetic.", HelpBoxMessageType.Info);
            root.Add(hint);
            void Refresh()
            {
                var active = enabled.hasMultipleDifferentValues || enabled.boolValue;
                foreach (var field in cache.Fields)
                {
                    ElementUtility.SetVisible(field.Value, field.Key == nameof(UIParticleFlightSettings.Enabled) || active);
                }

                ElementUtility.SetVisible(hint, active);
            }

            root.TrackPropertyValue(enabled, _ => Refresh());
            Refresh();
        }

        private class Cache
        {
            public readonly Dictionary<string, VisualElement> Fields = new();
        }
    }
}
