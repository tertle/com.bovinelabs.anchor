// <copyright file="QualityToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Toolbar;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.UIElements;

    [AutoToolbar("Quality")]
    public class QualityToolbarView : View<QualityToolbarViewModel>
    {
        public const string UssClassName = "bl-quality-tab";

        /// <summary> Initializes a new instance of the <see cref="QualityToolbarView" /> class. </summary>
        public QualityToolbarView()
            : base(new QualityToolbarViewModel())
        {
            this.AddToClassList(UssClassName);

            var dropdownField = new Dropdown
            {
                dataSource = this.ViewModel,
                defaultMessage = string.Empty,
                bindItem = (item, i) => item.label = this.ViewModel.QualityChoices[i],
            };

            dropdownField.SetBinding(nameof(Dropdown.sourceItems), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                dataSourcePath = new PropertyPath(nameof(QualityToolbarViewModel.QualityChoices)),
            });

            dropdownField.SetBinding(nameof(Dropdown.selectedIndex),
                new DataBinding
                {
                    dataSourcePath = new PropertyPath(nameof(QualityToolbarViewModel.QualityValue)),
                });

            this.Add(dropdownField);

            this.schedule.Execute(this.ViewModel.Update).Every(1); // Every frame
        }
    }
}
