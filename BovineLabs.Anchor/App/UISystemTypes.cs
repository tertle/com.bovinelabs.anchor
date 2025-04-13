﻿// <copyright file="UISystemTypes.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE
namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BovineLabs.Core.Keys;
    using BovineLabs.Core.PropertyDrawers;
    using BovineLabs.Core.Settings;
    using UnityEngine;

    [SettingsGroup("Anchor")]
    public class UISystemTypes : KSettingsBase<UISystemTypes, ulong>
    {
        [SerializeField]
        private NavigationComponent[] types = Array.Empty<NavigationComponent>();

        public override IEnumerable<NameValue<ulong>> Keys => this.Types.SelectMany(s => s.States.Select(n => new NameValue<ulong>(n, s.Component))).ToArray();

        public IReadOnlyList<NavigationComponent> Types => this.types;

        [Serializable]
        public class NavigationComponent
        {
            [Tooltip("Index 0 is the actual state that needs to match the NavigationGraph node. " +
                "Other keys indices are useful as sub states to conditionally start systems.")]
            [SerializeField]
            public string[] States = Array.Empty<string>();

            [StableTypeHash(StableTypeHashAttribute.TypeCategory.ComponentData, OnlyZeroSize = true, AllowUnityNamespace = false)]
            public ulong Component;
        }
    }
}
#endif
