// <copyright file="Converts.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Binding
{
    using UnityEngine.UIElements;
#if UNITY_EDITOR
  using UnityEditor;
#else
    using UnityEngine;
#endif

    public static class Converters
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        public static void RegisterConverters()
        {
            var group = new ConverterGroup("DisplayStyle");
            group.AddConverter((ref bool val) => val ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None));
            group.AddConverter((ref StyleEnum<DisplayStyle> displayStyle) => displayStyle == DisplayStyle.Flex);
            ConverterGroups.RegisterConverterGroup(group);
        }
    }
}