namespace BovineLabs.Anchor.Elements
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Input;
    using BovineLabs.Anchor.Components;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using Unity.Scripting.LifecycleManagement;
    using UnityEngine.UIElements;

    [UxmlElement]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "UITK Standard")]
    public partial class AnchorActionButton : ActionButton
    {
        [NoAutoStaticsCleanup]
        private static readonly BindingId CommandWithEventInfoProperty = nameof(commandWithEventInfo);

        private ICommand _commandWithEventInfo;

        public AnchorActionButton()
            : base(null)
        {
            RegisterCallback<ActionTriggeredEvent>(OnActionTriggered);
            GetOrAddComponent<AudioFeedback>();
        }

        [CreateProperty]
        public ICommand commandWithEventInfo
        {
            get => _commandWithEventInfo;
            set
            {
                if (_commandWithEventInfo != value)
                {
                    _commandWithEventInfo = value;
                    NotifyPropertyChanged(in CommandWithEventInfoProperty);
                }
            }
        }

        private void OnActionTriggered(ActionTriggeredEvent evt)
        {
            if (evt.target == this)
            {
                commandWithEventInfo?.Execute(evt);
            }
        }
    }
}
