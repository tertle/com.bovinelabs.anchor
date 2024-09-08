// <copyright file="FPSToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Views
{
    using System;
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Elements;
    using BovineLabs.Anchor.Toolbar;
    using Unity.Properties;
    using UnityEngine.UIElements;

    [AutoToolbar("FPS")]
    public class FPSToolbarView : VisualElement, IView
    {
        public const string UssClassName = "bl-fps-tab";

        private readonly FPSToolbarViewModel viewModel = new();

        public FPSToolbarView()
        {
            this.AddToClassList(UssClassName);

            TypeConverter<int, string> fpsConverter = (ref int value) => $"{value} fps";
            TypeConverter<float, string> timeConverter = (ref float value) => $"{value:0.0} ms";

            this.Add(KeyValueGroup.Create(this.viewModel, new (string, string, Action<DataBinding>)[]
            {
                ("FPS", nameof(FPSToolbarViewModel.CurrentFPS), db => db.sourceToUiConverters.AddConverter(fpsConverter)),
                ("Frame", nameof(FPSToolbarViewModel.FrameTime), db => db.sourceToUiConverters.AddConverter(timeConverter)),
                ("Average", nameof(FPSToolbarViewModel.AverageFPS), db => db.sourceToUiConverters.AddConverter(fpsConverter)),
                ("Min", nameof(FPSToolbarViewModel.MinFPS), db => db.sourceToUiConverters.AddConverter(fpsConverter)),
                ("Max", nameof(FPSToolbarViewModel.MaxFPS), db => db.sourceToUiConverters.AddConverter(fpsConverter)),
            }));

            this.schedule.Execute(this.viewModel.Update).Every(1);
        }

        object IView.ViewModel => this.viewModel;
    }
}
