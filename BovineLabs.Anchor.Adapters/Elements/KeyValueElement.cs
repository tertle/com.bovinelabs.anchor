// <copyright file="KeyValueElement.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
    using Unity.AppUI.UI;
    using UnityEngine;

    /// <summary>
    /// Utility class that creates a label pair formatted for key/value display.
    /// </summary>
    public class KeyValueElement
    {
        private const string UssClassName = "bl-key-value";

        private const string KeyUssClassName = UssClassName + "__key";
        private const string ValueUssClassName = UssClassName + "__value";

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueElement"/> class.
        /// </summary>
        /// <param name="keyText">Initial text displayed in the key column.</param>
        /// <param name="valueText">Initial text displayed in the value column.</param>
        public KeyValueElement(string keyText = "", string valueText = "")
        {
            this.KeyLabel = new Text();
            this.KeyLabel.AddToClassList(KeyUssClassName);
            this.ValueLabel = new Text();
            this.ValueLabel.AddToClassList(ValueUssClassName);
            this.ValueLabel.style.unityTextAlign = TextAnchor.UpperRight;

            this.KeyLabel.text = keyText;
            this.ValueLabel.text = valueText;
        }

        /// <summary>Gets the label used to render the key.</summary>
        public Text KeyLabel { get; }

        /// <summary>Gets the label used to render the value.</summary>
        public Text ValueLabel { get; }
    }
}
