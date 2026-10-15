namespace BovineLabs.Anchor.Elements
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.Assertions;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class KeyValueGroup : VisualElement
    {
        public const string UssClassName = "bl-key-value-group";

        public const string LeftUssClassName = UssClassName + "__left";
        public const string RightUssClassName = UssClassName + "__right";

        private readonly List<KeyValueElement> _elements = new();

        [UsedImplicitly]
        public KeyValueGroup()
            : this(null)
        {
        }

        public KeyValueGroup(IEnumerable<string> keys)
        {
            CreateColumns(this, out var left, out var right);

            if (keys == null)
            {
                return;
            }

            foreach (var k in keys)
            {
                var e = new KeyValueElement(k);

                left.Add(e.KeyLabel);
                right.Add(e.ValueLabel);

                _elements.Add(e);
            }
        }

        public IReadOnlyList<KeyValueElement> Elements => _elements;

        /// <summary>
        /// Creates aligned numeric readouts. The returned container has no string-row Elements contract.
        /// </summary>
        public static VisualElement CreateNumeric(
            object viewModel, (string Key, string Path, AnchorNumericText Value)[] fields, BindingUpdateTrigger trigger = BindingUpdateTrigger.OnSourceChanged)
        {
            var group = new VisualElement();
            CreateColumns(group, out var left, out var right);

            foreach (var field in fields)
            {
                var key = new Text(field.Key);
                key.AddToClassList("bl-key-value__key");
                left.Add(key);

                var value = field.Value;
                value.AddToClassList("bl-key-value__value");
                value.style.unityTextAlign = TextAnchor.UpperRight;
                value.dataSource = viewModel;
                value.SetBinding(AnchorNumericText.NumberProperty, new DataBinding
                {
                    bindingMode = BindingMode.ToTarget,
                    dataSourcePath = new PropertyPath(field.Path),
                    updateTrigger = trigger,
                });
                right.Add(value);
            }

            return group;
        }

        public static KeyValueGroup Create(
            object viewModel, (string Key, string Path)[] fields, BindingUpdateTrigger trigger = BindingUpdateTrigger.OnSourceChanged)
        {
            var group = new KeyValueGroup(fields.Select(k => k.Key));
            Assert.AreEqual(group.Elements.Count, fields.Length);

            for (var i = 0; i < fields.Length; i++)
            {
                var element = group._elements[i];
                var valuePath = fields[i].Path;
                var db = new DataBinding
                {
                    bindingMode = BindingMode.ToTarget,
                    dataSourcePath = new PropertyPath(valuePath),
                    updateTrigger = trigger,
                };

                element.ValueLabel.dataSource = viewModel;
                element.ValueLabel.SetBinding(nameof(Label.text), db);
            }

            return group;
        }

        public static KeyValueGroup Create(
            object viewModel, (string Key, string Path, Action<DataBinding> BindCallback)[] fields,
            BindingUpdateTrigger trigger = BindingUpdateTrigger.OnSourceChanged)
        {
            var group = new KeyValueGroup(fields.Select(k => k.Key));

            Assert.AreEqual(group.Elements.Count, fields.Length);

            for (var i = 0; i < fields.Length; i++)
            {
                var valuePath = fields[i].Path;
                var db = new DataBinding
                {
                    bindingMode = BindingMode.ToTarget,
                    dataSourcePath = new PropertyPath(valuePath),
                    updateTrigger = trigger,
                };

                fields[i].BindCallback?.Invoke(db);

                var element = group._elements[i];
                element.ValueLabel.dataSource = viewModel;
                element.ValueLabel.SetBinding(nameof(Text.text), db);
            }

            return group;
        }

        private static void CreateColumns(VisualElement group, out VisualElement left, out VisualElement right)
        {
            group.AddToClassList(UssClassName);
            group.style.flexDirection = FlexDirection.Row;

            left = new VisualElement();
            left.AddToClassList(LeftUssClassName);
            group.Add(left);

            right = new VisualElement();
            right.AddToClassList(RightUssClassName);
            group.Add(right);
        }
    }
}
