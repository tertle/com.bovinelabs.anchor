namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.ViewModels;
    using Unity.AppUI.UI;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public class QualityToolbarView : VisualElement
    {
        public const string UssClassName = "bl-quality-tab";

        [Preserve]
        public QualityToolbarView(QualityToolbarViewModel viewModel)
        {
            this.dataSource = viewModel;
            this.AddToClassList(UssClassName);

            var dropdownField = new Dropdown
            {
                defaultMessage = string.Empty,
                bindItem = (item, i) => item.label = this.Model.QualityChoices[i],
            };

            dropdownField.SetBindingToUI(nameof(Dropdown.sourceItems), nameof(QualityToolbarViewModel.QualityChoices));
            dropdownField.SetBindingTwoWay(nameof(Dropdown.selectedIndex), nameof(QualityToolbarViewModel.QualityValue));

            this.Add(dropdownField);

            this.schedule.Execute(this.UpdateModel).Every(1); // Every frame
        }

        private QualityToolbarViewModel Model => (QualityToolbarViewModel)this.dataSource;

        private void UpdateModel()
        {
            this.Model.Update();
        }
    }
}
