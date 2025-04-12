// <copyright file="TimeToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Views
{
    using System;
    using System.Text;
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Elements;
    using BovineLabs.Anchor.Toolbar;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.UIElements;
    using FloatField = Unity.AppUI.UI.FloatField;

    [AutoToolbar("Time")]
    public class TimeToolbarView : View<TimeToolbarViewModel>
    {
        public const string UssClassName = "bl-time-tab";

        /// <summary> Initializes a new instance of the <see cref="TimeToolbarView" /> class. </summary>
        public TimeToolbarView()
            : base(new TimeToolbarViewModel())
        {
            this.AddToClassList(UssClassName);

            var label = new Text("Timescale");

            var timescale = new FloatField
            {
                dataSource = this.ViewModel,
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
            this.Add(KeyValueGroup.Create(this.ViewModel,
                new (string, string, Action<DataBinding>)[]
                {
                    ("Real", nameof(TimeToolbarViewModel.UnscaledSeconds), db => db.sourceToUiConverters.AddConverter(timeConverter)),
                    ("Scale", nameof(TimeToolbarViewModel.Seconds), db => db.sourceToUiConverters.AddConverter(timeConverter)),
                }, BindingUpdateTrigger.EveryUpdate));

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
