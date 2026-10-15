namespace BovineLabs.Anchor.Elements
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Input;
    using BovineLabs.Anchor.Components;
    using Unity.Properties;
    using Unity.Scripting.LifecycleManagement;
    using UnityEngine.UIElements;
    using Button = Unity.AppUI.UI.Button;

    [UxmlElement]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "UITK Standard")]
    public partial class AnchorButton : Button
    {
        [NoAutoStaticsCleanup]
        private static readonly BindingId CommandWithEventInfoProperty = nameof(commandWithEventInfo);

        private ICommand _commandWithEventInfo;

        public AnchorButton()
            : base(null)
        {
            GetOrAddComponent<AudioFeedback>();
            clickable.clickedWithEventInfo += OnClickedWithEventInfo;
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

        private void OnClickedWithEventInfo(EventBase evt)
        {
            commandWithEventInfo?.Execute(evt);
        }
    }
}
