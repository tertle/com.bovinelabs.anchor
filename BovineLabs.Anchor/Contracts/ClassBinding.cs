// <copyright file="ClassBinding.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using Unity.Properties;
    using UnityEngine.UIElements;

    /// <summary>
    /// Custom binding that toggles a USS class on a visual element based on a boolean data source.
    /// </summary>
    [UxmlObject]
    public partial class ClassBinding : CustomBinding, IDataSourceProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassBinding"/> class.
        /// </summary>
        public ClassBinding()
        {
            this.updateTrigger = BindingUpdateTrigger.OnSourceChanged;
        }

        /// <inheritdoc />
        [CreateProperty]
        public object dataSource { get; set; }

        /// <inheritdoc />
        [CreateProperty]
        public PropertyPath dataSourcePath { get; set; }

        /// <summary>
        /// Gets or sets the USS class that will be toggled on the target element.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("class")]
        public string Class { get; set; }

        /// <summary>
        /// Gets or sets convenience accessor for setting the data source path using a string value.
        /// </summary>
        [UxmlAttribute("data-source-path")]
        public string DataSourcePathString
        {
            get => this.dataSourcePath.ToString();
            set => this.dataSourcePath = new PropertyPath(value);
        }

        /// <inheritdoc/>
        protected override BindingResult Update(in BindingContext context)
        {
            if (string.IsNullOrWhiteSpace(this.Class))
            {
                return new BindingResult(BindingStatus.Failure, "[UI Toolkit] ClassBinding requires a non-empty class name.");
            }

            var result = this.TryResolveBoolean(in context, out var enabled);

            if (result.status != BindingStatus.Success)
            {
                context.targetElement.EnableInClassList(this.Class, false);
                return result;
            }

            context.targetElement.EnableInClassList(this.Class, enabled);
            return result;
        }

        /// <inheritdoc/>
        protected override void OnDeactivated(in BindingActivationContext context)
        {
            base.OnDeactivated(in context);

            if (!string.IsNullOrWhiteSpace(this.Class))
            {
                context.targetElement.RemoveFromClassList(this.Class);
            }
        }

        private BindingResult TryResolveBoolean(in BindingContext context, out bool value)
        {
            value = false;

            var source = context.dataSource;
            if (source == null)
            {
                return new BindingResult(BindingStatus.Pending);
            }

            object resolved = source;

            if (!context.dataSourcePath.IsEmpty)
            {
                var container = source;
                if (!PropertyContainer.TryGetValue(ref container, context.dataSourcePath, out object propertyValue))
                {
                    var path = context.dataSourcePath.ToString();
                    var sourceType = TypeUtility.GetTypeDisplayName(source.GetType());
                    return new BindingResult(BindingStatus.Failure, $"[UI Toolkit] ClassBinding could not resolve `{path}` on `{sourceType}`.");
                }

                resolved = propertyValue;
            }

            if (resolved == null)
            {
                return new BindingResult(BindingStatus.Pending);
            }

            if (resolved is bool boolValue)
            {
                value = boolValue;
                return new BindingResult(BindingStatus.Success);
            }

            if (resolved is IConvertible convertible)
            {
                try
                {
                    value = convertible.ToBoolean(null);
                    return new BindingResult(BindingStatus.Success);
                }
                catch (Exception)
                {
                    // Fall through to failure reporting below if conversion is not possible.
                }
            }

            var resolvedType = TypeUtility.GetTypeDisplayName(resolved.GetType());
            return new BindingResult(BindingStatus.Failure, $"[UI Toolkit] ClassBinding expected a boolean value but resolved `{resolvedType}`.");
        }
    }
}
