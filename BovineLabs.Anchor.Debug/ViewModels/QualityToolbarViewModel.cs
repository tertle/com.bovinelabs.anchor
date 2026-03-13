// <copyright file="QualityToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using BovineLabs.Anchor.MVVM;
    using Unity.Properties;
    using UnityEngine;

    public class QualityToolbarViewModel : ObservableObject
    {
        private int qualityValue;

        [CreateProperty]
        public int QualityValue
        {
            get => this.qualityValue;
            set => this.SetProperty(ref this.qualityValue, value);
        }

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
