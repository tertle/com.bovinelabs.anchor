// <copyright file="RenderingToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Views
{
    using System;
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Elements;
    using Unity.Properties;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public class RenderingToolbarView : VisualElement
    {
        public const string UssClassName = "bl-rendering-tab";

        [Preserve]
        public RenderingToolbarView(RenderingToolbarViewModel viewModel)
        {
            this.dataSource = viewModel;
            this.AddToClassList(UssClassName);

            TypeConverter<long, string> countConverter = (ref long value) => FormatCount(value);

            this.Add(KeyValueGroup.Create(viewModel,
                new (string, string, Action<DataBinding>)[]
                {
                    ("Triangles", nameof(RenderingToolbarViewModel.Triangles), db => db.sourceToUiConverters.AddConverter(countConverter)),
                    ("Vertices", nameof(RenderingToolbarViewModel.Vertices), db => db.sourceToUiConverters.AddConverter(countConverter)),
                    ("Draw Calls", nameof(RenderingToolbarViewModel.DrawCalls), db => db.sourceToUiConverters.AddConverter(countConverter)),
                    ("SetPass", nameof(RenderingToolbarViewModel.SetPassCalls), db => db.sourceToUiConverters.AddConverter(countConverter)),
                    ("Instances", nameof(RenderingToolbarViewModel.Instances), db => db.sourceToUiConverters.AddConverter(countConverter)),
                }));

            this.schedule.Execute(this.UpdateModel).Every(1);
        }

        private RenderingToolbarViewModel Model => (RenderingToolbarViewModel)this.dataSource;

        private static string FormatCount(long value)
        {
            if (value >= 1_000_000_000)
            {
                return $"{value / 1_000_000_000}B";
            }

            if (value >= 1_000_000)
            {
                return $"{value / 1_000_000}M";
            }

            return value >= 1_000 ? $"{value / 1_000}K" : value.ToString();
        }

        private void UpdateModel()
        {
            this.Model.Update();
        }
    }
}
