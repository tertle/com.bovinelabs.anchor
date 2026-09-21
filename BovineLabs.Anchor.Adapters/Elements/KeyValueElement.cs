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
            KeyLabel = new Text();
            KeyLabel.AddToClassList(KeyUssClassName);
            ValueLabel = new Text();
            ValueLabel.AddToClassList(ValueUssClassName);
            ValueLabel.style.unityTextAlign = TextAnchor.UpperRight;

            KeyLabel.text = keyText;
            ValueLabel.text = valueText;
        }

        public Text KeyLabel { get; }

        public Text ValueLabel { get; }
    }
}
