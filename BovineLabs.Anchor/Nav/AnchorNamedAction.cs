// <copyright file="AnchorNamedAction.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using BovineLabs.Core.ObjectManagement;
    using UnityEngine;

    /// <summary>
    /// Serializable pairing of an action name and its definition.
    /// </summary>
    [Serializable]
    [AutoRef("AnchorSettings", "actions", nameof(AnchorNamedAction), "UI/Actions")]
    public class AnchorNamedAction : ScriptableObject
    {
        [SerializeField]
        private string actionName = string.Empty;

        [SerializeField]
        private AnchorNavAction action = new();

        /// <summary>Gets the unique action name.</summary>
        public string ActionName => this.actionName;

        /// <summary>Gets the action definition.</summary>
        public AnchorNavAction Action => this.action ??= new AnchorNavAction();
    }
}