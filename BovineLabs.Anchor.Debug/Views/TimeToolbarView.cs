// <copyright file="TimeToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Views
{
    using System;
    using System.Text;
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Toolbar;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.UIElements;
    using FloatField = Unity.AppUI.UI.FloatField;
    using KeyValueElement = BovineLabs.Anchor.Toolbar.KeyValueElement;

    [AutoToolbar("Time")]
    public class TimeToolbarView : VisualElement, IView
    {
        private readonly TimeToolbarViewModel viewModel = new();

        /// <summary> Initializes a new instance of the <see cref="TimeToolbarView"/> class. </summary>
        public TimeToolbarView()
        {
            var label = new Text("Timescale");

            var timescale = new FloatField
            {
                dataSource = this.viewModel,
                lowValue = 0f,
                value = Time.timeScale,
            };

            timescale.SetBinding(nameof(FloatField.value), new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(TimeToolbarViewModel.TimeScale)),
            });

            this.Add(label);
            this.Add(timescale);

            TypeConverter<long, string> timeConverter = (ref long value) => $"{ToFormattedString(TimeSpan.FromSeconds(value))}";
            this.Add(KeyValueElement.Create(this.viewModel, timeConverter, "Real", nameof(TimeToolbarViewModel.UnscaledSeconds), BindingUpdateTrigger.EveryUpdate));
            this.Add(KeyValueElement.Create(this.viewModel, timeConverter, "Scaled", nameof(TimeToolbarViewModel.Seconds), BindingUpdateTrigger.EveryUpdate));

            this.schedule.Execute(this.viewModel.Update).Every(1);
        }

        /// <inheritdoc/>
        object IView.ViewModel => this.viewModel;

        private static string ToFormattedString(TimeSpan ts)
        {
            var builder = new StringBuilder();

            if (ts.Days > 0)
            {
                builder.Append($"{ts.Days}.");
            }

            if (ts.Days > 0 || ts.Hours > 0)
            {
                builder.Append($"{ts.Hours}:");
            }

            builder.Append($"{ts.Minutes:00}:{ts.Seconds:00}");
            return builder.ToString();
        }
    }
}
