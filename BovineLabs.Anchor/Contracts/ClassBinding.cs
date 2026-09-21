namespace BovineLabs.Anchor
{
    using System;
    using Unity.Properties;
    using UnityEngine.UIElements;

    [UxmlObject]
    public partial class ClassBinding : CustomBinding, IDataSourceProvider
    {
        private IVisualElementScheduledItem _scheduledItem;

        public ClassBinding()
        {
            updateTrigger = BindingUpdateTrigger.OnSourceChanged;
        }

        [CreateProperty]
        public object dataSource { get; set; }

        [CreateProperty]
        public PropertyPath dataSourcePath { get; set; }

        [CreateProperty]
        [UxmlAttribute("class")]
        public string Class { get; set; }

        // This exists to allow toggling 1 frame after startup to enforce a transition event
        [CreateProperty]
        [UxmlAttribute("delay")]
        public bool Delay { get; set; }

        [UxmlAttribute("data-source-path")]
        public string DataSourcePathString
        {
            get => dataSourcePath.ToString();
            set => dataSourcePath = new PropertyPath(value);
        }

        protected override BindingResult Update(in BindingContext context)
        {
            CancelScheduledUpdate();

            if (string.IsNullOrWhiteSpace(Class))
            {
                return new BindingResult(BindingStatus.Failure, "[UI Toolkit] ClassBinding requires a non-empty class name.");
            }

            var result = TryResolveBoolean(in context, out var enabled);

            if (result.status != BindingStatus.Success)
            {
                SetState(context.targetElement, Delay, Class, false);
                return result;
            }

            SetState(context.targetElement, Delay, Class, enabled);
            return result;
        }

        private void SetState(VisualElement element, bool delay, string className, bool state)
        {
            if (delay)
            {
                _scheduledItem = element.schedule.Execute(() => element.EnableInClassList(className, state));
            }
            else
            {
                element.EnableInClassList(className, state);
            }
        }

        protected override void OnDeactivated(in BindingActivationContext context)
        {
            CancelScheduledUpdate();
            base.OnDeactivated(in context);

            if (!string.IsNullOrWhiteSpace(Class))
            {
                context.targetElement.RemoveFromClassList(Class);
            }
        }

        private void CancelScheduledUpdate()
        {
            _scheduledItem?.Pause();
            _scheduledItem = null;
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
