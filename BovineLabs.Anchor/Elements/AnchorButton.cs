// <copyright file="AnchorButton.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Input;
    using Unity.Properties;
    using UnityEngine.UIElements;
    using Button = Unity.AppUI.UI.Button;

    /// <summary>
    /// Button that forwards the full click event info to an ICommand.
    /// </summary>
    [UxmlElement]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "UITK Standard")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1308:Variable names should not be prefixed", Justification = "UITK Standard")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "UITK Standard")]
    public partial class AnchorButton : Button
    {
        private static readonly BindingId CommandWithEventInfoProperty = nameof(commandWithEventInfo);

        private ICommand m_commandWithEventInfo;

        public AnchorButton()
            : base(null)
        {
            this.clickable.clickedWithEventInfo += evt => this.commandWithEventInfo?.Execute(evt);
        }

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
    }
}
