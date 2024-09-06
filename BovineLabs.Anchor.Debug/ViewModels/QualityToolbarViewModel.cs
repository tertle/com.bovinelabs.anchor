// <copyright file="QualityToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Unity.Properties;
    using UnityEngine;

    public class QualityToolbarViewModel : BLObservableObject
    {
        private int quality;

        private List<string> choices;

        public QualityToolbarViewModel()
        {
            this.QualityValue = QualitySettings.GetQualityLevel();
        }

        [CreateProperty]
        public int QualityValue
        {
            get => this.quality;
            set
            {
                if (this.SetProperty(ref this.quality, value))
                {
                    QualitySettings.SetQualityLevel(this.QualityValue);
                }
            }
        }

        [CreateProperty]
        public List<string> QualityChoices => this.choices ??= QualitySettings.names.ToList();

        public void Update()
        {
            this.QualityValue = QualitySettings.GetQualityLevel();
        }
    }
}
