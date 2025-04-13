// <copyright file="AnchorSettings.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE
namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Core.Settings;
    using Unity.AppUI.Navigation;
    using UnityEngine;
    using UnityEngine.UIElements;

    [SettingsGroup("Anchor")]
    public class AnchorSettings : ScriptableObject, ISettings
    {
        [SerializeField]
        private NavGraphViewAsset navigationGraph;

        [SerializeField]
        [Tooltip("If true, will disable instantiation in builds without toolbar to speed up initialization.")]
        private bool toolbarOnly;

#if UNITY_EDITOR || BL_DEBUG
        [SerializeField]
        private StyleSheet[] debugStyleSheets = Array.Empty<StyleSheet>();
#endif

        public NavGraphViewAsset NavigationGraph => this.navigationGraph;

        public bool ToolbarOnly => this.toolbarOnly;

#if UNITY_EDITOR || BL_DEBUG
        public IReadOnlyList<StyleSheet> DebugStyleSheets => this.debugStyleSheets;
#endif
    }
}
#endif
