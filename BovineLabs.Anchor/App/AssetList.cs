namespace BovineLabs.Anchor
{
    using System;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class AssetList : MonoBehaviour
    {
        public KeyAsset[] Assets = Array.Empty<KeyAsset>();

        [Serializable]
        public class KeyAsset
        {
            public string Key;
            public Object Asset;
        }
    }
}
