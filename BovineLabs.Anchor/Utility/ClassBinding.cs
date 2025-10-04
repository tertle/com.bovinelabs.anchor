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
        private string className;
        private object localDataSource;
        private PropertyPath dataSourcePathValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassBinding"/> class.
        /// </summary>
        public ClassBinding()
        {
            this.updateTrigger = BindingUpdateTrigger.OnSourceChanged;
        }

        /// <summary>
        /// Gets or sets the USS class that will be toggled on the target element.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("class")]
        public string ClassName
        {
            get => this.className;
            set
            {
                if (this.className == value)
                {
                    return;
                }

                this.className = value;
                this.MarkDirty();
            }
        }

        /// <inheritdoc />
        [CreateProperty]
        public object dataSource
        {
            get => this.localDataSource;
            set
            {
                if (Equals(this.localDataSource, value))
                {
                    return;
                }

                this.localDataSource = value;
                this.MarkDirty();
            }
        }

        /// <inheritdoc />
        [CreateProperty]
        public PropertyPath dataSourcePath
        {
            get => this.dataSourcePathValue;
            set
            {
                if (this.dataSourcePathValue.Equals(value))
                {
                    return;
                }

                this.dataSourcePathValue = value;
                this.MarkDirty();
            }
        }

        /// <summary>
        /// Convenience accessor for setting the data source path using a string value.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("data-source-path")]
        public string DataSourcePath
        {
            get => this.dataSourcePathValue.ToString();
            set => this.dataSourcePath = string.IsNullOrEmpty(value) ? default : new PropertyPath(value);
        }

        /// <inheritdoc/>
        protected override BindingResult Update(in BindingContext context)
        {
            if (string.IsNullOrEmpty(this.ClassName))
            {
                return new BindingResult(BindingStatus.Failure, "[UI Toolkit] ClassBinding requires a non-empty class name.");
            }

            var element = context.targetElement;
            var result = this.TryResolveBoolean(in context, out var enabled);

            if (result.status != BindingStatus.Success)
            {
                element.EnableInClassList(this.ClassName, false);
                return result;
            }

            element.EnableInClassList(this.ClassName, enabled);
            return result;
        }

        /// <inheritdoc/>
        protected override void OnDeactivated(in BindingActivationContext context)
        {
            base.OnDeactivated(in context);

            if (!string.IsNullOrEmpty(this.ClassName))
            {
                context.targetElement.RemoveFromClassList(this.ClassName);
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
