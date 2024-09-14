﻿// <copyright file="AssetList.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class AssetList : MonoBehaviour
    {
        public KeyAsset[] Assets = Array.Empty<KeyAsset>();

        public static AssetList Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        [Serializable]
        public class KeyAsset
        {
            public string Key;
            public Object Asset;
        }
    }
}
