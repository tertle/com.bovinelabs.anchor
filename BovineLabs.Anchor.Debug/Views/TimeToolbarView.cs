// <copyright file="TimeToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Views
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Elements;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.UIElements;
    using FloatField = Unity.AppUI.UI.FloatField;
    using UnityEngine.Scripting;

    [Preserve]
    public class TimeToolbarView : VisualElement
    {
        public const string UssClassName = "bl-time-tab";

        /// <summary> Initializes a new instance of the <see cref="TimeToolbarView" /> class. </summary>
        [Preserve]
        public TimeToolbarView(TimeToolbarViewModel viewModel)
        {
            this.dataSource = viewModel;
            this.AddToClassList(UssClassName);

            TypeConverter<long, string> timeConverter = static (ref long value) => $"{ToFormattedString(TimeSpan.FromSeconds(value))}";
            TypeConverter<float, string> timescaleConverter = static (ref float value) => $"{value:0.00}x";

            this.Add(KeyValueGroup.Create(viewModel,
                new (string, string, Action<DataBinding>)[]
                {
                    ("Real", nameof(TimeToolbarViewModel.UnscaledSeconds), db => db.sourceToUiConverters.AddConverter(timeConverter)),
                    ("Scale", nameof(TimeToolbarViewModel.Seconds), db => db.sourceToUiConverters.AddConverter(timeConverter)),
                    ("Timescale", nameof(TimeToolbarViewModel.TimeScale), db => db.sourceToUiConverters.AddConverter(timescaleConverter)),
                }));

            var timescale = new SliderFloat
            {
                lowValue = 0f,
                highValue = 3f,
                formatString = "0.0",
                step = 0.25f,
                track = TrackDisplayType.On,
                showMarks = true,
                restrictedValues = RestrictedValuesPolicy.Step,
                showInputField = false,
                scale = TimeToolbarViewModel.UIToTimeScale,
                formatFunction = value => $"{value:0.00}x",
                displayValueLabel = ValueDisplayMode.Auto,
                customMarks = new List<SliderMark<float>>
                {
                    new()
                    {
                        value = 1f,
                        label = "1x",
                    },
                    new()
                    {
                        value = 2f,
                        label = "2x",
                    },
                },
            };

            timescale.SetBindingTwoWay(nameof(FloatField.value), nameof(TimeToolbarViewModel.TimeScale),
                static (ref float value) => TimeToolbarViewModel.TimescaleToUI(value), static (ref float value) => TimeToolbarViewModel.UIToTimeScale(value));

            this.Add(timescale);

            this.schedule.Execute(this.UpdateModel).Every(1);
        }

        private TimeToolbarViewModel Model => (TimeToolbarViewModel)this.dataSource;

        private void UpdateModel()
        {
            this.Model.Update();
        }

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
