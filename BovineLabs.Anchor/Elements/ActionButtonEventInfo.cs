// <copyright file="ActionButtonEventInfo.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
    using System.Windows.Input;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class ActionButtonEventInfo : ActionButton
    {
        private static readonly BindingId CommandWithEventInfoProperty = nameof(commandWithEventInfo);

        private ICommand command;

        public ActionButtonEventInfo()
            : base(null)
        {
            this.clickable.clickedWithEventInfo += evt => this.commandWithEventInfo?.Execute(evt);
        }

        [CreateProperty]
        public ICommand commandWithEventInfo
        {
            get => this.command;
            set
            {
                var changed = this.command != value;
                this.command = value;

                if (changed)
                {
                    this.NotifyPropertyChanged(in CommandWithEventInfoProperty);
                }
            }
        }
    }
}
