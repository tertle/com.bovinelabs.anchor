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
            dataSource = viewModel;
            AddToClassList(UssClassName);

            var dropdownField = new Dropdown
            {
                defaultMessage = string.Empty,
                bindItem = (item, i) => item.label = Model.QualityChoices[i],
            };

            dropdownField.SetBindingToUI(nameof(Dropdown.sourceItems), nameof(QualityToolbarViewModel.QualityChoices));
            dropdownField.SetBindingTwoWay(nameof(Dropdown.selectedIndex), nameof(QualityToolbarViewModel.QualityValue));

            Add(dropdownField);

            schedule.Execute(UpdateModel).Every(1); // Every frame
        }

        private QualityToolbarViewModel Model => (QualityToolbarViewModel)dataSource;

        private void UpdateModel()
        {
            Model.Update();
        }
    }
}
