// <copyright file="AnchorNamedAction.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Serializable pairing of an action name and its definition.
    /// </summary>
    [Serializable]
    public class AnchorNamedAction
    {
        [SerializeField]
        private string name = string.Empty;

        [SerializeField]
        private AnchorNavAction action = new();

        /// <summary>Gets the unique action name.</summary>
        public string Name => this.name;

        /// <summary>Gets the action definition.</summary>
        public AnchorNavAction Action => this.action;
    }
}
