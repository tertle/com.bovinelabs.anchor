// <copyright file="ToolbarGroup.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if !BL_DISABLE_TOOLBAR && (BL_DEBUG || UNITY_EDITOR)
namespace BovineLabs.Anchor.Toolbar
{
    using System.Collections.Generic;
    using UnityEngine.UIElements;
    using Button = Unity.AppUI.UI.Button;

    internal class ToolbarGroup
    {
        public ToolbarGroup(string name, Button button, ToolbarGroupElement parent)
        {
            this.Name = name;
            this.Button = button;
            this.Parent = parent;
        }

        public string Name { get; }

        public Button Button { get; }

        public ToolbarGroupElement Parent { get; }

        public List<Tab> Groups { get; } = new();

        public class Tab
        {
            public Tab(int id, string name, ToolbarTabElement container, ToolbarGroup group, VisualElement view)
            {
                this.Name = name;
                this.ID = id;
                this.Container = container;
                this.Group = group;
                this.View = view;
            }

            /// <summary> Gets the name of the group, shown below. </summary>
            public string Name { get; }

            public int ID { get; }

            /// <summary> Gets the root visual element of the tab. </summary>
            public ToolbarTabElement Container { get; }

            public ToolbarGroup Group { get; }

            public VisualElement View { get; }
        }
    }
}
#endif
