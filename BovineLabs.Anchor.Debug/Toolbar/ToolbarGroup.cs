namespace BovineLabs.Anchor.Debug.Toolbar
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

            public string Name { get; }

            public int ID { get; }

            public ToolbarTabElement Container { get; }

            public ToolbarGroup Group { get; }

            public VisualElement View { get; }
        }
    }
}
