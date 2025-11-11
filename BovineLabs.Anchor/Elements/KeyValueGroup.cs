// <copyright file="KeyValueGroup.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.Assertions;
    using UnityEngine.UIElements;

    /// <summary>
    /// Visual element that lays out a set of key/value text pairs in two aligned columns.
    /// </summary>
    [UxmlElement]
    public partial class KeyValueGroup : VisualElement
    {
        public const string UssClassName = "bl-key-value-group";

        public const string LeftUssClassName = UssClassName + "__left";
        public const string RightUssClassName = UssClassName + "__right";

        private readonly List<KeyValueElement> elements = new();
        private string test;

        /// <summary>Initializes a new instance of the <see cref="KeyValueGroup"/> class.</summary>
        [UsedImplicitly]
        public KeyValueGroup()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueGroup"/> class with the provided keys.
        /// </summary>
        /// <param name="keys">Keys to pre-populate along the left column.</param>
        public KeyValueGroup(IEnumerable<string> keys)
        {
            this.AddToClassList(UssClassName);
            this.style.flexDirection = FlexDirection.Row;

            var left = new VisualElement();
            left.AddToClassList(LeftUssClassName);
            this.Add(left);

            var right = new VisualElement();
            right.AddToClassList(RightUssClassName);
            this.Add(right);

            if (keys == null)
            {
                return;
            }

            foreach (var k in keys)
            {
                var e = new KeyValueElement(k);

                left.Add(e.KeyLabel);
                right.Add(e.ValueLabel);

                this.elements.Add(e);
            }
        }

        /// <summary>Gets the rendered key/value elements in display order.</summary>
        public IReadOnlyList<KeyValueElement> Elements => this.elements;

        /// <summary>
        /// Creates a key/value group and binds each value to the provided view-model path.
        /// </summary>
        public static KeyValueGroup Create(
            object viewModel, (string Key, string Path)[] fields, BindingUpdateTrigger trigger = BindingUpdateTrigger.OnSourceChanged)
        {
            var group = new KeyValueGroup(fields.Select(k => k.Key));
            Assert.AreEqual(group.Elements.Count, fields.Length);

            for (var i = 0; i < fields.Length; i++)
            {
                var element = group.elements[i];
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

        /// <summary>
        /// Creates a key/value group and binds each value using a per-field binding callback.
        /// </summary>
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

                var element = group.elements[i];
                element.ValueLabel.dataSource = viewModel;
                element.ValueLabel.SetBinding(nameof(Text.text), db);
            }

            return group;
        }
    }
}
