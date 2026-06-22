// <copyright file="AnchorButton.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Input;
    using BovineLabs.Anchor.Audio;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.Scripting.APIUpdating;
    using UnityEngine.UIElements;
    using Button = Unity.AppUI.UI.Button;

    /// <summary>
    /// Button that forwards the full click event info to an ICommand.
    /// </summary>
    [MovedFrom(true, "BovineLabs.Anchor.Elements", "BovineLabs.Anchor")]
    [UxmlElement]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "UITK Standard")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1308:Variable names should not be prefixed", Justification = "UITK Standard")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "UITK Standard")]
    public partial class AnchorButton : Button, IAnchorAudioElement
    {
        private static readonly BindingId CommandWithEventInfoProperty = nameof(commandWithEventInfo);
        private static readonly BindingId AudioProfileProperty = nameof(audioProfile);
        private static readonly BindingId HoverAudioModeProperty = nameof(hoverAudioMode);
        private static readonly BindingId HoverAudioClipProperty = nameof(hoverAudioClip);
        private static readonly BindingId ActivateAudioModeProperty = nameof(activateAudioMode);
        private static readonly BindingId ActivateAudioClipProperty = nameof(activateAudioClip);

        private ICommand m_commandWithEventInfo;
        private string m_audioProfile = string.Empty;
        private AnchorAudioOverrideMode m_hoverAudioMode;
        private AudioClip m_hoverAudioClip;
        private AnchorAudioOverrideMode m_activateAudioMode;
        private AudioClip m_activateAudioClip;

        public AnchorButton()
            : base(null)
        {
            this.clickable.clickedWithEventInfo += evt => this.commandWithEventInfo?.Execute(evt);
        }

        /// <inheritdoc />
        string IAnchorAudioElement.AudioProfile => this.m_audioProfile;

        /// <inheritdoc />
        AnchorAudioCueOverride IAnchorAudioElement.HoverAudio => new(this.m_hoverAudioMode, this.m_hoverAudioClip);

        /// <inheritdoc />
        AnchorAudioCueOverride IAnchorAudioElement.ActivateAudio => new(this.m_activateAudioMode, this.m_activateAudioClip);

        /// <summary>Gets or sets the command invoked whenever the button is clicked with event data.</summary>
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

        /// <summary>Gets or sets the named audio profile key.</summary>
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

        /// <summary>Gets or sets the hover audio override mode.</summary>
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

        /// <summary>Gets or sets the hover audio clip used when <see cref="hoverAudioMode"/> is custom.</summary>
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

        /// <summary>Gets or sets the activation audio override mode.</summary>
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

        /// <summary>Gets or sets the activation audio clip used when <see cref="activateAudioMode"/> is custom.</summary>
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
    }
}
