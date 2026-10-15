namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Elements;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public class FPSToolbarView : VisualElement
    {
        public const string UssClassName = "bl-fps-tab";

        [Preserve]
        public FPSToolbarView(FPSToolbarViewModel viewModel)
        {
            dataSource = viewModel;
            AddToClassList(UssClassName);

            Add(KeyValueGroup.CreateNumeric(viewModel,
                new (string, string, AnchorNumericText)[]
                {
                    ("FPS", nameof(FPSToolbarViewModel.CurrentFPS), new AnchorIntegerText {
                        suffix = " fps"
                    }),
                    ("Frame", nameof(FPSToolbarViewModel.FrameTime), new AnchorNumberText {
                        format = "0.0",
                        suffix = " ms"
                    }),
                    ("Average", nameof(FPSToolbarViewModel.AverageFPS), new AnchorIntegerText {
                        suffix = " fps"
                    }),
                    ("Min", nameof(FPSToolbarViewModel.MinFPS), new AnchorIntegerText {
                        suffix = " fps"
                    }),
                    ("Max", nameof(FPSToolbarViewModel.MaxFPS), new AnchorIntegerText {
                        suffix = " fps"
                    }),
                }));

            schedule.Execute(UpdateModel).Every(1);
        }

        private FPSToolbarViewModel Model => (FPSToolbarViewModel)dataSource;

        private void UpdateModel()
        {
            Model.Update();
        }
    }
}
