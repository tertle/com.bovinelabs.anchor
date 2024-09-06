// <copyright file="FPSToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.ToolbarTabs.Views
{
    using BovineLabs.Anchor.Debug.ToolbarTabs.ViewModels;
    using BovineLabs.Anchor.Toolbar;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.UIElements;
    using KeyValueElement = BovineLabs.Anchor.Toolbar.KeyValueElement;

    [AutoToolbar("FPS")]
    public class FPSToolbarView : VisualElement, IView
    {
        private readonly FPSToolbarViewModel viewModel = new();

        public FPSToolbarView()
        {
            TypeConverter<int, string> fpsConverter = (ref int value) => $"{value} fps";
            TypeConverter<float, string> timeConverter = (ref float value) => $"{value:0.0} ms";

            this.Add(KeyValueElement.Create(this.viewModel, fpsConverter, "Current", nameof(FPSToolbarViewModel.CurrentFPS)));
            this.Add(KeyValueElement.Create(this.viewModel, timeConverter, "Frame", nameof(FPSToolbarViewModel.FrameTime)));
            this.Add(KeyValueElement.Create(this.viewModel, fpsConverter, "Average", nameof(FPSToolbarViewModel.AverageFPS)));
            this.Add(KeyValueElement.Create(this.viewModel, fpsConverter, "Min", nameof(FPSToolbarViewModel.MinFPS)));
            this.Add(KeyValueElement.Create(this.viewModel, fpsConverter, "Max", nameof(FPSToolbarViewModel.MaxFPS)));

            this.schedule.Execute(this.viewModel.Update).Every(1);

            Debug.Log("Setup");
        }

        object IView.ViewModel => this.viewModel;
    }
}
