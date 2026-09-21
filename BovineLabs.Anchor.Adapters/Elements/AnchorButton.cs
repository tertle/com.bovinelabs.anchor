namespace BovineLabs.Anchor.Elements
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Input;
    using BovineLabs.Anchor.Audio;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Button = Unity.AppUI.UI.Button;

    [UxmlElement]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "UITK Standard")]
    public partial class AnchorButton : Button
    {
        private static readonly BindingId CommandWithEventInfoProperty = nameof(commandWithEventInfo);
        private static readonly BindingId AudioProfileProperty = nameof(audioProfile);
        private static readonly BindingId HoverAudioModeProperty = nameof(hoverAudioMode);
        private static readonly BindingId HoverAudioClipProperty = nameof(hoverAudioClip);
        private static readonly BindingId ActivateAudioModeProperty = nameof(activateAudioMode);
        private static readonly BindingId ActivateAudioClipProperty = nameof(activateAudioClip);

        private ICommand _commandWithEventInfo;
        private string _audioProfile = AnchorAudioSettings.DefaultProfileKey;
        private AnchorAudioOverrideMode _hoverAudioMode;
        private AudioClip _hoverAudioClip;
        private AnchorAudioOverrideMode _activateAudioMode;
        private AudioClip _activateAudioClip;

        public AnchorButton()
            : base(null)
        {
            clickable.clickedWithEventInfo += OnClickedWithEventInfo;
            clicked += OnAudioActivated;
            RegisterCallback<PointerEnterEvent>(OnAudioPointerEnter);
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

        [CreateProperty]
        [UxmlAttribute]
        public string audioProfile
        {
            get => _audioProfile;
            set
            {
                value ??= string.Empty;
                if (_audioProfile != value)
                {
                    _audioProfile = value;
                    NotifyPropertyChanged(in AudioProfileProperty);
                }
            }
        }

        [CreateProperty]
        [UxmlAttribute]
        public AnchorAudioOverrideMode hoverAudioMode
        {
            get => _hoverAudioMode;
            set
            {
                if (_hoverAudioMode != value)
                {
                    _hoverAudioMode = value;
                    NotifyPropertyChanged(in HoverAudioModeProperty);
                }
            }
        }

        [CreateProperty]
        [UxmlAttribute]
        public AudioClip hoverAudioClip
        {
            get => _hoverAudioClip;
            set
            {
                if (_hoverAudioClip != value)
                {
                    _hoverAudioClip = value;
                    NotifyPropertyChanged(in HoverAudioClipProperty);
                }
            }
        }

        [CreateProperty]
        [UxmlAttribute]
        public AnchorAudioOverrideMode activateAudioMode
        {
            get => _activateAudioMode;
            set
            {
                if (_activateAudioMode != value)
                {
                    _activateAudioMode = value;
                    NotifyPropertyChanged(in ActivateAudioModeProperty);
                }
            }
        }

        [CreateProperty]
        [UxmlAttribute]
        public AudioClip activateAudioClip
        {
            get => _activateAudioClip;
            set
            {
                if (_activateAudioClip != value)
                {
                    _activateAudioClip = value;
                    NotifyPropertyChanged(in ActivateAudioClipProperty);
                }
            }
        }

        private void OnClickedWithEventInfo(EventBase evt)
        {
            commandWithEventInfo?.Execute(evt);
        }

        private void OnAudioPointerEnter(PointerEnterEvent evt)
        {
            if (!enabledInHierarchy)
            {
                return;
            }

            if (evt.pointerId != PointerId.mousePointerId && evt.pointerId < PointerId.trackedPointerIdBase)
            {
                return;
            }

            AnchorAudio.Play(_audioProfile, AnchorAudioCue.Hover, new AnchorAudioCueOverride(_hoverAudioMode, _hoverAudioClip));
        }

        private void OnAudioActivated()
        {
            AnchorAudio.Play(_audioProfile, AnchorAudioCue.Activate, new AnchorAudioCueOverride(_activateAudioMode, _activateAudioClip));
        }
    }
}
