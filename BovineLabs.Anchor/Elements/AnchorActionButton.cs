// <copyright file="AnchorActionButton.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Input;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.UIElements;

    /// <summary>
    /// ActionButton variant that exposes the click event data to bound commands.
    /// </summary>
    [UxmlElement]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "UITK Standard")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1308:Variable names should not be prefixed", Justification = "UITK Standard")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "UITK Standard")]
    public partial class AnchorActionButton : ActionButton
    {
        private static readonly BindingId CommandWithEventInfoProperty = nameof(commandWithEventInfo);

        private ICommand m_commandWithEventInfo;

        public AnchorActionButton()
            : base(null)
        {
            this.clickable.clickedWithEventInfo += evt => this.commandWithEventInfo?.Execute(evt);
        }

        /// <summary>
        /// Clickable Manipulator for this ActionButton.
        /// </summary>
        [CreateProperty]
        public new Pressable clickable
        {
            get => base.clickable;
            set => base.clickable = value;
        }

        /// <summary>Gets or sets the command invoked when the button is clicked with the event payload.</summary>
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
    }
}
