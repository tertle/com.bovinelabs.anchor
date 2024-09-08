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
    public class QualityToolbarView : VisualElement, IView
    {
        public const string UssClassName = "bl-quality-tab";

        private readonly QualityToolbarViewModel viewModel = new();

        /// <summary> Initializes a new instance of the <see cref="QualityToolbarView"/> class. </summary>
        /// <param name="viewModel"> The view model. </param>
        public QualityToolbarView()
        {
            this.AddToClassList(UssClassName);

            var dropdownField = new Dropdown
            {
                dataSource = this.viewModel,
                defaultMessage = string.Empty,
                bindItem = (item, i) => item.label = this.viewModel.QualityChoices[i],
            };

            dropdownField.SetBinding(nameof(Dropdown.sourceItems), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                dataSourcePath = new PropertyPath(nameof(QualityToolbarViewModel.QualityChoices)),
            });

            dropdownField.SetBinding(nameof(Dropdown.selectedIndex), new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(QualityToolbarViewModel.QualityValue)),
            });

            this.Add(dropdownField);

            this.schedule.Execute(this.viewModel.Update).Every(1); // Every frame
        }

        /// <inheritdoc/>
        object IView.ViewModel => this.viewModel;
    }
}
