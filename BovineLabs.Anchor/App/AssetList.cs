// <copyright file="AssetList.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Simple asset container that exposes an array of named references for lookup at runtime.
    /// </summary>
    public class AssetList : MonoBehaviour
    {
        public KeyAsset[] Assets = Array.Empty<KeyAsset>();

        /// <summary>
        /// Associates a string key with a Unity asset reference.
        /// </summary>
        [Serializable]
        public class KeyAsset
        {
            public string Key;
            public Object Asset;
        }
    }
}
