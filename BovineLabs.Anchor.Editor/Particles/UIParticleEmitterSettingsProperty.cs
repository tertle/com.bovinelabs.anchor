namespace BovineLabs.Anchor.Editor.Particles
{
    using BovineLabs.Anchor.Particles;
    using BovineLabs.Core.Editor.Inspectors;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;

    [CustomPropertyDrawer(typeof(UIParticleEmitterSettings))]
    public sealed class UIParticleEmitterSettingsProperty : ElementProperty
    {
        protected override VisualElement CreateElement(SerializedProperty property)
        {
            var cache = Cache<Cache>();
            switch (property.name)
            {
                case nameof(UIParticleEmitterSettings.TextureSource):
                    cache.Source = property;
                    break;
                case nameof(UIParticleEmitterSettings.Texture):
                    return cache.Texture = CreatePropertyField(property);
                case nameof(UIParticleEmitterSettings.Uv):
                    return cache.Uv = CreatePropertyField(property);
                case nameof(UIParticleEmitterSettings.Sprite):
                    return cache.Sprite = CreatePropertyField(property);
                case nameof(UIParticleEmitterSettings.Flipbook):
                    cache.Animated = property.FindPropertyRelative(nameof(UIParticleFlipbookSettings.Enabled));
                    return cache.Flipbook = CreatePropertyField(property);
            }

            return base.CreateElement(property);
        }

        protected override void PostElementCreation(VisualElement root, bool createdElements)
        {
            var cache = Cache<Cache>();
            cache.Hint = new HelpBox("Sprite sources are static. Use Texture with a rectangular grid for flipbooks.", HelpBoxMessageType.Info);
            root.Add(cache.Hint);
            root.TrackPropertyValue(cache.Source, _ => UpdateVisibility(cache));
            root.TrackPropertyValue(cache.Animated, _ => UpdateVisibility(cache));
            UpdateVisibility(cache);
        }

        private static void UpdateVisibility(Cache cache)
        {
            var mixed = cache.Source.hasMultipleDifferentValues;
            var sprite = cache.Source.enumValueIndex == (int)UIParticleTextureSource.Sprite;
            ElementUtility.SetVisible(cache.Texture, mixed || !sprite);
            ElementUtility.SetVisible(cache.Uv, mixed || !sprite);
            ElementUtility.SetVisible(cache.Sprite, mixed || sprite);
            ElementUtility.SetVisible(cache.Hint, mixed || sprite);
            // Keep an incompatible enabled section editable so the user can disable it after changing source.
            ElementUtility.SetVisible(cache.Flipbook, mixed || !sprite || cache.Animated.hasMultipleDifferentValues || cache.Animated.boolValue);
        }

        private class Cache
        {
            public SerializedProperty Source;
            public SerializedProperty Animated;
            public PropertyField Texture;
            public PropertyField Uv;
            public PropertyField Sprite;
            public PropertyField Flipbook;
            public HelpBox Hint;
        }
    }
}
