// <copyright file="QualityToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System.Collections.Generic;
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
            set
            {
                if (this.SetProperty(ref this.qualityValue, value))
                {
                    QualitySettings.SetQualityLevel(this.qualityValue);
                }
            }
        }

        public QualityToolbarViewModel()
        {
            this.QualityChoices = QualitySettings.names.ToList();
            this.QualityValue = QualitySettings.GetQualityLevel();
        }

        [CreateProperty(ReadOnly = true)]
        public List<string> QualityChoices { get; }

        public void Update()
        {
            this.QualityValue = QualitySettings.GetQualityLevel();
        }
    }
}
