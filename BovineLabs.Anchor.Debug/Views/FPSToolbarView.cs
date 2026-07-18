// <copyright file="FPSToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Views
{
    using System;
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Elements;
    using Unity.Properties;
    using UnityEngine.UIElements;
    using UnityEngine.Scripting;

    [Preserve]
    public class FPSToolbarView : VisualElement
    {
        public const string UssClassName = "bl-fps-tab";

        [Preserve]
        public FPSToolbarView(FPSToolbarViewModel viewModel)
        {
            this.dataSource = viewModel;
            this.AddToClassList(UssClassName);

            TypeConverter<int, string> fpsConverter = (ref int value) => $"{value} fps";
            TypeConverter<float, string> timeConverter = (ref float value) => $"{value:0.0} ms";

            this.Add(KeyValueGroup.Create(viewModel,
                new (string, string, Action<DataBinding>)[]
                {
                    ("FPS", nameof(FPSToolbarViewModel.CurrentFPS), db => db.sourceToUiConverters.AddConverter(fpsConverter)),
                    ("Frame", nameof(FPSToolbarViewModel.FrameTime), db => db.sourceToUiConverters.AddConverter(timeConverter)),
                    ("Average", nameof(FPSToolbarViewModel.AverageFPS), db => db.sourceToUiConverters.AddConverter(fpsConverter)),
                    ("Min", nameof(FPSToolbarViewModel.MinFPS), db => db.sourceToUiConverters.AddConverter(fpsConverter)),
                    ("Max", nameof(FPSToolbarViewModel.MaxFPS), db => db.sourceToUiConverters.AddConverter(fpsConverter)),
                }));

            this.schedule.Execute(this.UpdateModel).Every(1);
        }

        private FPSToolbarViewModel Model => (FPSToolbarViewModel)this.dataSource;

        private void UpdateModel()
        {
            this.Model.Update();
        }
    }
}
