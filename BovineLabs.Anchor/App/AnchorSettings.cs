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
        private StyleSheet[] debugStyleSheets = Array.Empty<StyleSheet>();

        [SerializeField]
        private Shader linearProgressShader;

        [Header("Views")]
        [SerializeField]
        private string startDestination = string.Empty;

        [SerializeField]
        private KeyUXML[] views = Array.Empty<KeyUXML>();

        [SerializeField]
        private List<AnchorAction> actions = new();

        [SerializeField]
        private List<AnchorNavAnimation> animations = new();

        [Header("Audio")]
        [SerializeField]
        private AnchorAudioSettings audio = new();

        public KeyUXML[] Views => this.views;

        public string StartDestination => this.startDestination;

        public IReadOnlyList<AnchorAction> Actions => this.actions;

        public IReadOnlyList<AnchorNavAnimation> Animations => this.animations;

        public IReadOnlyList<StyleSheet> DebugStyleSheets => this.debugStyleSheets;

        public AnchorAudioSettings Audio => this.audio;

        public Shader LinearProgressShader => this.linearProgressShader;

#if UNITY_EDITOR
        private void Reset()
        {
            this.linearProgressShader = AssetDatabase.LoadAssetAtPath<Shader>(
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
