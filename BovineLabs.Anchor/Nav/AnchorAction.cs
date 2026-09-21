namespace BovineLabs.Anchor.Nav
{
    using System;
    using BovineLabs.Core.Asset;
    using UnityEngine;

    [Serializable]
    [AutoRef("AnchorSettings", "_actions", nameof(AnchorAction), "UI/Actions")]
    public class AnchorAction : ScriptableObject
    {
        [SerializeField]
        private string _actionName = string.Empty;

        [SerializeField]
        private AnchorNavAction _action = new();

        public string ActionName => _actionName;

        public AnchorNavAction Action => _action ??= new AnchorNavAction();
    }
}
