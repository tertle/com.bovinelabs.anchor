namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Audio;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Core.Settings;
    using UnityEngine;
    using UnityEngine.UIElements;

    [SettingsGroup("Anchor")]
    public class AnchorSettings : SettingsSingleton<AnchorSettings>
    {
        [SerializeField]
        private StyleSheet[] debugStyleSheets = Array.Empty<StyleSheet>();

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

        [Serializable]
        public class KeyUXML
        {
            public string Key;
            public VisualTreeAsset Asset;
        }
    }
}
