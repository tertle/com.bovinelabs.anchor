// <copyright file="AnchorSettings.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Core.Settings;
    using UnityEngine;
    using UnityEngine.UIElements;

    [SettingsGroup("Anchor")]
    public class AnchorSettings : SettingsSingleton<AnchorSettings>
    {
        [Header("Options")]
        [SerializeField]
        [Tooltip("If true, will disable instantiation in builds without toolbar to speed up initialization.")]
        private bool toolbarOnly;

        [SerializeField]
        private StyleSheet[] debugStyleSheets = Array.Empty<StyleSheet>();

        [Header("Views")]
        [SerializeField]
        private string startDestination = string.Empty;

        [SerializeField]
        private KeyUXML[] views = Array.Empty<KeyUXML>();

        [SerializeField]
        private List<AnchorNamedAction> actions = new();

        public KeyUXML[] Views => this.views;

        public bool ToolbarOnly => this.toolbarOnly;

        public string StartDestination => this.startDestination;

        public IReadOnlyList<AnchorNamedAction> Actions => this.actions;

        public IReadOnlyList<StyleSheet> DebugStyleSheets => this.debugStyleSheets;

        [Serializable]
        public class KeyUXML
        {
            public string Key;
            public VisualTreeAsset Asset;
        }
    }
}
