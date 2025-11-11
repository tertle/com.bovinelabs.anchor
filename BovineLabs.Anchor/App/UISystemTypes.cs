// <copyright file="UISystemTypes.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BovineLabs.Core;
    using BovineLabs.Core.Keys;
    using BovineLabs.Core.Settings;
    using UnityEngine;

    /// <summary>
    /// Maps navigation states to the DOTS systems that should run while that state is active.
    /// </summary>
    [SettingsGroup("Anchor")]
    public class UISystemTypes : KSettingsBase<UISystemTypes, ulong>
    {
        [SerializeField]
        private NavigationComponent[] types = Array.Empty<NavigationComponent>();

        /// <summary>Gets the registered navigation state keys paired with their stable type hash.</summary>
        public override IEnumerable<NameValue<ulong>> Keys => this.Types
            .Where(s => s.Component != null)
            .SelectMany(s => s.States.Select(n => new NameValue<ulong>(n, s.Component.GetStableTypeHash())))
            .ToArray();

        /// <summary>Gets the navigation components that should be toggled per destination.</summary>
        public IReadOnlyList<NavigationComponent> Types => this.types;

        /// <summary>
        /// Describes which navigation states map to a specific MonoBehaviour component.
        /// </summary>
        [Serializable]
        public class NavigationComponent
        {
            [Tooltip("Index 0 is the actual state that needs to match the NavigationGraph node. " +
                "Other keys indices are useful as sub states to conditionally start systems.")]
            [SerializeField]
            public string[] States = Array.Empty<string>();

            [SerializeField]
            public ComponentAssetBase Component;
        }
    }
}
