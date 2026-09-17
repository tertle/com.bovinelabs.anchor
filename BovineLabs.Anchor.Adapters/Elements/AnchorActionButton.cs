namespace BovineLabs.Anchor.Elements
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Input;
    using BovineLabs.Anchor.Audio;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.UIElements;

    [UxmlElement]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "UITK Standard")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1308:Variable names should not be prefixed", Justification = "UITK Standard")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "UITK Standard")]
    public partial class AnchorActionButton : ActionButton
    {
        private static readonly BindingId CommandWithEventInfoProperty = nameof(commandWithEventInfo);
        private static readonly BindingId AudioProfileProperty = nameof(audioProfile);
        private static readonly BindingId HoverAudioModeProperty = nameof(hoverAudioMode);
        private static readonly BindingId HoverAudioClipProperty = nameof(hoverAudioClip);
        private static readonly BindingId ActivateAudioModeProperty = nameof(activateAudioMode);
        private static readonly BindingId ActivateAudioClipProperty = nameof(activateAudioClip);

        private ICommand m_commandWithEventInfo;
        private string m_audioProfile = AnchorAudioSettings.DefaultProfileKey;
        private AnchorAudioOverrideMode m_hoverAudioMode;
        private AudioClip m_hoverAudioClip;
        private AnchorAudioOverrideMode m_activateAudioMode;
        private AudioClip m_activateAudioClip;

        public AnchorActionButton()
            : base(null)
        {
            this.RegisterCallback<ActionTriggeredEvent>(this.OnActionTriggered);
            this.RegisterCallback<PointerEnterEvent>(this.OnAudioPointerEnter);
        }

        [CreateProperty]
        public ICommand commandWithEventInfo
        {
            get => this.m_commandWithEventInfo;
            set
            {
                if (this.m_commandWithEventInfo != value)
                {
                    this.m_commandWithEventInfo = value;
                    this.NotifyPropertyChanged(in CommandWithEventInfoProperty);
                }
            }
        }

        [CreateProperty]
        [UxmlAttribute]
        public string audioProfile
        {
            get => this.m_audioProfile;
            set
            {
                value ??= string.Empty;
                if (this.m_audioProfile != value)
                {
                    this.m_audioProfile = value;
                    this.NotifyPropertyChanged(in AudioProfileProperty);
                }
            }
        }

        [CreateProperty]
        [UxmlAttribute]
        public AnchorAudioOverrideMode hoverAudioMode
        {
            get => this.m_hoverAudioMode;
            set
            {
                if (this.m_hoverAudioMode != value)
                {
                    this.m_hoverAudioMode = value;
                    this.NotifyPropertyChanged(in HoverAudioModeProperty);
                }
            }
        }

        [CreateProperty]
        [UxmlAttribute]
        public AudioClip hoverAudioClip
        {
            get => this.m_hoverAudioClip;
            set
            {
                if (this.m_hoverAudioClip != value)
                {
                    this.m_hoverAudioClip = value;
                    this.NotifyPropertyChanged(in HoverAudioClipProperty);
                }
            }
        }

        [CreateProperty]
        [UxmlAttribute]
        public AnchorAudioOverrideMode activateAudioMode
        {
            get => this.m_activateAudioMode;
            set
            {
                if (this.m_activateAudioMode != value)
                {
                    this.m_activateAudioMode = value;
                    this.NotifyPropertyChanged(in ActivateAudioModeProperty);
                }
            }
        }

        [CreateProperty]
        [UxmlAttribute]
        public AudioClip activateAudioClip
        {
            get => this.m_activateAudioClip;
            set
            {
                if (this.m_activateAudioClip != value)
                {
                    this.m_activateAudioClip = value;
                    this.NotifyPropertyChanged(in ActivateAudioClipProperty);
                }
            }
        }

        private void OnActionTriggered(ActionTriggeredEvent evt)
        {
            if (evt.target != this)
            {
                return;
            }

            this.commandWithEventInfo?.Execute(evt);
            AnchorAudio.Play(this.m_audioProfile, AnchorAudioCue.Activate, new AnchorAudioCueOverride(this.m_activateAudioMode, this.m_activateAudioClip));
        }

        private void OnAudioPointerEnter(PointerEnterEvent evt)
        {
            if (!this.enabledInHierarchy)
            {
                return;
            }

            if (evt.pointerId != PointerId.mousePointerId && evt.pointerId < PointerId.trackedPointerIdBase)
            {
                return;
            }

            AnchorAudio.Play(this.m_audioProfile, AnchorAudioCue.Hover, new AnchorAudioCueOverride(this.m_hoverAudioMode, this.m_hoverAudioClip));
        }
    }
}
