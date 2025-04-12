// <copyright file="AnchorUITypes.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE
namespace BovineLabs.Anchor
{
    using System;
    using BovineLabs.Core.Keys;
    using BovineLabs.Core.PropertyDrawers;
    using BovineLabs.Core.Settings;
    using Unity.AppUI.Navigation;
    using UnityEngine;

    [Serializable]
    [ResourceSettings]
    public class AnchorUITypes : ScriptableObject
    {
        [SerializeField]
        private NavGraphViewAsset graphAsset;

        [SerializeField]
        private NameValue[] keys = Array.Empty<NameValue>();

        [Serializable]
        public struct ComponentMap : IKKeyValue
        {
            [SerializeField]
            private string name;

            [SerializeField]
            [StableTypeHash(StableTypeHashAttribute.TypeCategory.ComponentData, OnlyZeroSize = true, AllowUnityNamespace = false)]
            private ulong component;

            public string Name { get; set; }

            public int Value { get; set; }
        }
    }
}
#endif
