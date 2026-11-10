namespace BovineLabs.Anchor.Debug.Views
{
    using System;
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Elements;
    using Unity.AppUI.UI;
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

            var counters = KeyValueGroup.Create(viewModel,
                new (string, string, Action<DataBinding>)[]
                {
                    ("Triangles", nameof(RenderStatsToolbarViewModel.Triangles), db => db.sourceToUiConverters.AddConverter(countConverter)),
                    ("Vertices", nameof(RenderStatsToolbarViewModel.Vertices), db => db.sourceToUiConverters.AddConverter(countConverter)),
                    ("Draw Calls", nameof(RenderStatsToolbarViewModel.DrawCalls), db => db.sourceToUiConverters.AddConverter(countConverter)),
                    ("SetPass", nameof(RenderStatsToolbarViewModel.SetPassCalls), db => db.sourceToUiConverters.AddConverter(countConverter)),
                    ("Instances", nameof(RenderStatsToolbarViewModel.Instances), db => db.sourceToUiConverters.AddConverter(countConverter)),
                });

            SetBreakdownTooltip(counters.Elements[2], viewModel, nameof(RenderStatsToolbarViewModel.DrawCallsBreakdown));
            SetBreakdownTooltip(counters.Elements[4], viewModel, nameof(RenderStatsToolbarViewModel.InstancesBreakdown));
            Add(counters);

            schedule.Execute(UpdateModel).Every(1);
        }

        private RenderStatsToolbarViewModel Model => (RenderStatsToolbarViewModel)dataSource;

        private static void SetBreakdownTooltip(KeyValueElement row, RenderStatsToolbarViewModel viewModel, string property)
        {
            var tooltip = new Text { dataSource = viewModel, pickingMode = PickingMode.Ignore };
            tooltip.SetBindingToUI(nameof(Text.text), property);
            row.KeyLabel.SetTooltipTemplate(tooltip);
            row.ValueLabel.SetTooltipTemplate(tooltip);
        }

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
