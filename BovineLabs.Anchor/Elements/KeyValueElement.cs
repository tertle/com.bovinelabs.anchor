// <copyright file="KeyValueElement.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
    using Unity.AppUI.UI;
    using UnityEngine;

    public class KeyValueElement
    {
        private const string UssClassName = "bl-key-value";

        private const string KeyUssClassName = UssClassName + "__key";
        private const string ValueUssClassName = UssClassName + "__value";

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

        public Text KeyLabel { get; }

        public Text ValueLabel { get; }
    }
}
