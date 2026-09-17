namespace BovineLabs.Anchor.Nav
{
    using System;
    using BovineLabs.Core.Asset;
    using UnityEngine;

    [Serializable]
    [AutoRef("AnchorSettings", "actions", nameof(AnchorAction), "UI/Actions")]
    public class AnchorAction : ScriptableObject
    {
        [SerializeField]
        private string actionName = string.Empty;

        [SerializeField]
        private AnchorNavAction action = new();

        public string ActionName => this.actionName;

        public AnchorNavAction Action => this.action ??= new AnchorNavAction();
    }
}
