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

    /// <summary>
    /// Centralized configuration for Anchor UI behaviour, including views, navigation defaults, and debug styles.
    /// </summary>
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

#if APP_UI_EDITOR_ONLY
        public override bool IncludeInBuild => false;
#endif

        /// <summary>Gets the list of UXML assets that can be instantiated by key.</summary>
        public KeyUXML[] Views => this.views;

        /// <summary>Gets a value indicating whether the runtime should initialize only when the toolbar is available.</summary>
        public bool ToolbarOnly => this.toolbarOnly;

        /// <summary>Gets the navigation destination that should be loaded when the app starts.</summary>
        public string StartDestination => this.startDestination;

        /// <summary>Gets the collection of named navigation actions that are available globally.</summary>
        public IReadOnlyList<AnchorNamedAction> Actions => this.actions;

        /// <summary>Gets any additional style sheets that should be injected while running in debug contexts.</summary>
        public IReadOnlyList<StyleSheet> DebugStyleSheets => this.debugStyleSheets;

        /// <summary>Maps a unique string key to a visual tree asset.</summary>
        [Serializable]
        public class KeyUXML
        {
            public string Key;
            public VisualTreeAsset Asset;
        }
    }
}