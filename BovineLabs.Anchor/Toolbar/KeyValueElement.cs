// <copyright file="KeyValueElement.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_DEBUG || UNITY_EDITOR
namespace BovineLabs.Anchor.Toolbar
{
    using JetBrains.Annotations;
    using Unity.Properties;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class KeyValueElement : VisualElement
    {
        private const string UssClassName = "bl-key-value";

        private const string KeyUssClassName = UssClassName + "__key";
        private const string ValueUssClassName = UssClassName + "__value";

        public static readonly BindingId KeyTextProperty = (BindingId)nameof(keyText);
        public static readonly BindingId ValueTextProperty = (BindingId)nameof(valueText);

        private readonly Label key;
        private readonly Label value;

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
        public KeyValueElement()
            : this(string.Empty)
        {
        }

        public KeyValueElement(string keyText = "", string valueText = "")
        {
            this.AddToClassList(UssClassName);

            this.style.flexDirection = FlexDirection.Row;

            this.key = new Label();
            this.key.AddToClassList(KeyUssClassName);
            this.value = new Label();
            this.value.AddToClassList(ValueUssClassName);

            this.Add(this.key);
            this.Add(this.value);

            this.keyText = keyText;
            this.valueText = valueText;
        }

        public static KeyValueElement Create(object viewModel, string keyName, string valuePath, BindingUpdateTrigger trigger = BindingUpdateTrigger.WhenDirty)
        {
            var dataBinding = new DataBinding { dataSourcePath = new PropertyPath(valuePath), updateTrigger = trigger };
            var element = new KeyValueElement(keyName) { dataSource = viewModel };
            element.SetBinding(nameof(valueText), dataBinding);
            return element;
        }

        public static KeyValueElement Create<TSource, TDestination>(
            object viewModel, TypeConverter<TSource, TDestination> converter, string keyName, string valuePath, BindingUpdateTrigger trigger = BindingUpdateTrigger.WhenDirty)
        {
            var dataBinding = new DataBinding { dataSourcePath = new PropertyPath(valuePath), updateTrigger = trigger };
            dataBinding.sourceToUiConverters.AddConverter(converter);

            var element = new KeyValueElement(keyName) { dataSource = viewModel };
            element.SetBinding(nameof(valueText), dataBinding);
            return element;
        }

        [CreateProperty]
        public string keyText
        {
            get => this.key.text;
            set
            {
                if (this.key.text == value)
                {
                    return;
                }

                this.key.text = value;
                this.NotifyPropertyChanged(KeyTextProperty);
            }
        }

        [CreateProperty]
        public string valueText
        {
            get => this.value.text;
            set
            {
                if (this.value.text == value)
                {
                    return;
                }

                this.value.text = value;
                this.NotifyPropertyChanged(ValueTextProperty);
            }
        }
    }
}
#endif
