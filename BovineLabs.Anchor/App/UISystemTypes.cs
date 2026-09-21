namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BovineLabs.Core;
    using BovineLabs.Core.Keys;
    using BovineLabs.Core.Settings;
    using UnityEngine;

    [SettingsGroup("Anchor")]
    public class UISystemTypes : KSettingsBase<UISystemTypes, ulong>
    {
        [SerializeField]
        private NavigationComponent[] _types = Array.Empty<NavigationComponent>();

        public override IEnumerable<NameValue<ulong>> Keys => Types
            .Where(s => s.Component != null)
            .SelectMany(s => s.States.Select(n => new NameValue<ulong>(n, s.Component.GetStableTypeHash())))
            .ToArray();

        public IReadOnlyList<NavigationComponent> Types => _types;

        [Serializable]
        public class NavigationComponent
        {
            [Tooltip("Index 0 is the actual state that needs to match the NavigationGraph node. " +
                "Other keys indices are useful as sub states to conditionally start systems.")]
            [SerializeField]
            public string[] States = Array.Empty<string>();

            [SerializeField]
            public ComponentAsset Component;
        }
    }
}
