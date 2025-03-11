// <copyright file="QualityToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Unity.AppUI.MVVM;
    using Unity.Properties;
    using UnityEngine;

    [ObservableObject]
    public partial class QualityToolbarViewModel
    {
        [ObservableProperty]
        private int qualityValue;

        public QualityToolbarViewModel()
        {
            this.PropertyChanged += this.OnPropertyChanged;
            this.QualityChoices = QualitySettings.names.ToList();
            this.QualityValue = QualitySettings.GetQualityLevel();
        }

        [CreateProperty(ReadOnly = true)]
        public List<string> QualityChoices { get; }

        public void Update()
        {
            this.QualityValue = QualitySettings.GetQualityLevel();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.QualityValue))
            {
                QualitySettings.SetQualityLevel(this.QualityValue);
            }
        }
    }
}
