// <copyright file="ToolbarOffset.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Toolbar
{
    using UnityEngine;

    [RequireComponent(typeof(RectTransform))]
    [ExecuteInEditMode]
    public class ToolbarOffset : MonoBehaviour
    {
        private void Awake()
        {
            var tr = (RectTransform)this.transform;

            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;

            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
        }
    }
}
