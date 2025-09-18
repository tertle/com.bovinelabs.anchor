// <copyright file="UXMLServiceAssets.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE
namespace BovineLabs.Anchor.Services
{
    using System;
    using BovineLabs.Core.Settings;
    using UnityEngine;
    using UnityEngine.UIElements;

    [SettingsGroup("Anchor")]
    public class UXMLServiceAssets : SettingsSingleton
    {
        private static UXMLServiceAssets settings;

        [SerializeField]
        private KeyUXML[] values = Array.Empty<KeyUXML>();

        public KeyUXML[] Values => this.values;

        public static UXMLServiceAssets I
        {
            get => GetSingleton(ref settings);
            private set => settings = value;
        }

        /// <inheritdoc/>
        protected override void Initialize()
        {
            I = this;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            I = this;
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
#endif