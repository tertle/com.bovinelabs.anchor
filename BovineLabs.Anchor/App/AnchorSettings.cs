namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Audio;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Core.Settings;
    using UnityEngine;
    using UnityEngine.UIElements;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    [SettingsGroup("Anchor")]
    public class AnchorSettings : SettingsSingleton<AnchorSettings>
    {
        [SerializeField]
        private StyleSheet[] _debugStyleSheets = Array.Empty<StyleSheet>();

        [SerializeField]
        private Shader _linearProgressShader;

        [Header("Views")]
        [SerializeField]
        private string _startDestination = string.Empty;

        [SerializeField]
        private KeyUXML[] _views = Array.Empty<KeyUXML>();

        [SerializeField]
        private List<AnchorAction> _actions = new();

        [SerializeField]
        private List<AnchorNavAnimation> _animations = new();

        [Header("Audio")]
        [SerializeField]
        private AnchorAudioSettings _audio = new();

        public KeyUXML[] Views => _views;

        public string StartDestination => _startDestination;

        public IReadOnlyList<AnchorAction> Actions => _actions;

        public IReadOnlyList<AnchorNavAnimation> Animations => _animations;

        public IReadOnlyList<StyleSheet> DebugStyleSheets => _debugStyleSheets;

        public AnchorAudioSettings Audio => _audio;

        public Shader LinearProgressShader => _linearProgressShader;

#if UNITY_EDITOR
        private void Reset()
        {
            _linearProgressShader = AssetDatabase.LoadAssetAtPath<Shader>(
                "Packages/com.bovinelabs.anchor/BovineLabs.Anchor.Adapters/Shaders/AnchorLinearProgress.shader");
        }
#endif

        [Serializable]
        public class KeyUXML
        {
            public string Key;
            public VisualTreeAsset Asset;
        }
    }
}
