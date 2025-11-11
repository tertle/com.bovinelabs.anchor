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
    using BovineLabs.Anchor.Toolbar;
    using Unity.AppUI.UI;
    using Unity.Mathematics;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.UIElements;
    using FloatField = Unity.AppUI.UI.FloatField;
    using SliderInt = Unity.AppUI.UI.SliderInt;

    [AutoToolbar("Time")]
    public class TimeToolbarView : View<TimeToolbarViewModel>
    {
        public const string UssClassName = "bl-time-tab";

        /// <summary> Initializes a new instance of the <see cref="TimeToolbarView" /> class. </summary>
        public TimeToolbarView()
            : base(new TimeToolbarViewModel())
        {
            this.AddToClassList(UssClassName);

            var label = new Text("Timescale") { dataSource = this.ViewModel };
            label.SetBindingToUI(nameof(Text.text), nameof(TimeToolbarViewModel.TimeScale), static (ref float value) => $"Timescale: {value:0.00}x");

            var timescale = new SliderFloat
            {
                dataSource = this.ViewModel,
                lowValue = 0f,
                highValue = 2f,
                formatString = "0.0",
                step = 0.25f,
                track = TrackDisplayType.On,
                showMarks = true,
                restrictedValues = RestrictedValuesPolicy.Step,
                showInputField = false,
                scale = TimeToolbarViewModel.UIToTimeScale,
                formatFunction = value => $"{value:0.00}x",
                displayValueLabel = ValueDisplayMode.Auto,
            };

            timescale.SetBindingTwoWay(nameof(FloatField.value), nameof(TimeToolbarViewModel.TimeScale),
                static (ref float value) => TimeToolbarViewModel.TimescaleToUI(value), static (ref float value) => TimeToolbarViewModel.UIToTimeScale(value));

            this.Add(label);

            TypeConverter<long, string> timeConverter = (ref long value) => $"{ToFormattedString(TimeSpan.FromSeconds(value))}";
            this.Add(KeyValueGroup.Create(this.ViewModel,
                new (string, string, Action<DataBinding>)[]
                {
                    ("Real", nameof(TimeToolbarViewModel.UnscaledSeconds), db => db.sourceToUiConverters.AddConverter(timeConverter)),
                    ("Scale", nameof(TimeToolbarViewModel.Seconds), db => db.sourceToUiConverters.AddConverter(timeConverter)),
                }, BindingUpdateTrigger.EveryUpdate));

            this.Add(timescale);

            this.schedule.Execute(this.ViewModel.Update).Every(1);
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
