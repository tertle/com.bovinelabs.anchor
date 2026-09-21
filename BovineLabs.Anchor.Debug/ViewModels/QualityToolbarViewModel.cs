namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.Views;
    using BovineLabs.Anchor.MVVM;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    [IsService]
    [AutoToolbar("Quality")]
    public class QualityToolbarViewModel : ObservableObject, IToolbarElement
    {
        private int _qualityValue;

        [CreateProperty]
        public int QualityValue
        {
            get => _qualityValue;
            set
            {
                if (SetProperty(ref _qualityValue, value))
                {
                    QualitySettings.SetQualityLevel(_qualityValue);
                }
            }
        }

        public QualityToolbarViewModel()
        {
            QualityChoices = QualitySettings.names.ToList();
            QualityValue = QualitySettings.GetQualityLevel();
        }

        [CreateProperty(ReadOnly = true)]
        public List<string> QualityChoices { get; }

        public VisualElement CreateElement()
        {
            return new QualityToolbarView(this);
        }

        public void Update()
        {
            QualityValue = QualitySettings.GetQualityLevel();
        }
    }
}
