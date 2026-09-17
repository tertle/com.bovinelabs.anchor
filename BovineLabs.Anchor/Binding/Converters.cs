namespace BovineLabs.Anchor.Binding
{
    using Unity.Scripting.LifecycleManagement;
    using UnityEngine.UIElements;

    public static partial class Converters
    {
        [OnCodeInitializing]
        private static void RegisterConverters()
        {
            {
                var group = new ConverterGroup("DisplayStyle");
                group.AddConverter((ref bool val) => val ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None));
                group.AddConverter((ref StyleEnum<DisplayStyle> displayStyle) => displayStyle == DisplayStyle.Flex);
                ConverterGroups.RegisterConverterGroup(group);
            }

            {
                var group = new ConverterGroup("DisplayStyleInverted");
                group.AddConverter((ref bool val) => val ? new StyleEnum<DisplayStyle>(DisplayStyle.None) : new StyleEnum<DisplayStyle>(DisplayStyle.Flex));
                group.AddConverter((ref StyleEnum<DisplayStyle> displayStyle) => displayStyle != DisplayStyle.Flex);
                ConverterGroups.RegisterConverterGroup(group);
            }

            {
                var group = new ConverterGroup("Invert");
                group.AddConverter((ref bool val) => !val);
                ConverterGroups.RegisterConverterGroup(group);
            }
        }
    }
}
