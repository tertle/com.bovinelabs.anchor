// <copyright file="UXMLServiceAssets.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    using System;
    using BovineLabs.Core.Settings;
    using UnityEngine;
    using UnityEngine.UIElements;

    [SettingsGroup("Anchor")]
    public class UXMLServiceAssets : SettingsSingleton<UXMLServiceAssets>
    {
        [SerializeField]
        private KeyUXML[] values = Array.Empty<KeyUXML>();

        public KeyUXML[] Values => this.values;

        [Serializable]
        public class KeyUXML
        {
            public string Key;
            public VisualTreeAsset Asset;
        }
    }
}