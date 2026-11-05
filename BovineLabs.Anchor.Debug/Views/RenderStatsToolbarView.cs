namespace BovineLabs.Anchor.Debug.Views
{
    using System;
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Elements;
    using Unity.Properties;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public class RenderStatsToolbarView : VisualElement
    {
        public const string UssClassName = "bl-render-stats-tab";

        [Preserve]
        public RenderStatsToolbarView(RenderStatsToolbarViewModel viewModel)
        {
            dataSource = viewModel;
            AddToClassList(UssClassName);

            TypeConverter<long, string> countConverter = (ref long value) => FormatCount(value);

            Add(KeyValueGroup.Create(viewModel,
                new (string, string, Action<DataBinding>)[]
                {
                    ("Triangles", nameof(RenderStatsToolbarViewModel.Triangles), db => db.sourceToUiConverters.AddConverter(countConverter)),
                    ("Vertices", nameof(RenderStatsToolbarViewModel.Vertices), db => db.sourceToUiConverters.AddConverter(countConverter)),
                    ("Draw Calls", nameof(RenderStatsToolbarViewModel.DrawCalls), db => db.sourceToUiConverters.AddConverter(countConverter)),
                    ("SetPass", nameof(RenderStatsToolbarViewModel.SetPassCalls), db => db.sourceToUiConverters.AddConverter(countConverter)),
                    ("Instances", nameof(RenderStatsToolbarViewModel.Instances), db => db.sourceToUiConverters.AddConverter(countConverter)),
                }));

            schedule.Execute(UpdateModel).Every(1);
        }

        private RenderStatsToolbarViewModel Model => (RenderStatsToolbarViewModel)dataSource;

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
            Model.Update();
        }
    }
}
