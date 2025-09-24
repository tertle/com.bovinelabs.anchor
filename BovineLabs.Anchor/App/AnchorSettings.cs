// <copyright file="AnchorSettings.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE
namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Core.Settings;
    using Unity.AppUI.Navigation;
    using UnityEngine;
    using UnityEngine.UIElements;

    [SettingsGroup("Anchor")]
    public class AnchorSettings : SettingsSingleton<AnchorSettings>
    {
        [SerializeField]
        [Tooltip("If true, will disable instantiation in builds without toolbar to speed up initialization.")]
        private bool toolbarOnly;

        [SerializeField]
        private List<AnchorNamedAction> actions = new();

        [SerializeField]
        private StyleSheet[] debugStyleSheets = Array.Empty<StyleSheet>();

        public bool ToolbarOnly => this.toolbarOnly;

        public IReadOnlyList<AnchorNamedAction> Actions => this.actions;

        public IReadOnlyList<StyleSheet> DebugStyleSheets => this.debugStyleSheets;
    }
}
#endif
