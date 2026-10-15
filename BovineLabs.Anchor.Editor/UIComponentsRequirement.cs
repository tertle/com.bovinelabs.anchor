namespace BovineLabs.Anchor.Editor
{
    using System;
    using System.Reflection;
    using Unity.Scripting.LifecycleManagement;
    using UnityEditor;

    [InitializeOnLoad]
    internal static class UIComponentsRequirement
    {
        // Metadata from Unity's permanent Editor assembly remains valid across play-mode and code reload scopes.
        [NoAutoStaticsCleanup]
        private static readonly PropertyInfo EnabledProperty;

        static UIComponentsRequirement()
        {
            var settingsType = Type.GetType("UnityEditor.UIElements.UIToolkitProjectSettings, UnityEditor.UIElementsModule", true);
            EnabledProperty = settingsType.GetProperty("enableUIComponents", BindingFlags.Static | BindingFlags.NonPublic);

            // Unity keeps this switch internal. Its setter saves settings and updates open authoring interfaces.
            // Unity clears this notification's subscriptions on code reload; this initializer reinstalls ours.
            var changedField = settingsType.GetField("onEnableUIComponentsChanged", BindingFlags.Static | BindingFlags.NonPublic);
            changedField.SetValue(null, (Action)changedField.GetValue(null) + EnsureEnabled);
            EnsureEnabled();
        }

        private static void EnsureEnabled()
        {
            if (!(bool)EnabledProperty.GetValue(null))
            {
                EnabledProperty.SetValue(null, true);
            }
        }
    }
}
