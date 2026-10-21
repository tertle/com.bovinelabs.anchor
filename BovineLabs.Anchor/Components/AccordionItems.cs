namespace BovineLabs.Anchor.Components
{
    using System.Collections;
    using System.Collections.Generic;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.UIElements;

    [VisualElementComponent]
    [RequiresElementOfType(typeof(Accordion))]
    public partial struct AccordionItems
    {
        [CreateProperty]
        [UxmlAttribute("item-template")]
        public VisualTreeAsset ItemTemplate;

        [CreateProperty]
        public IList ItemsSource;

        private TemplateItems _items;

        [OnComponentAdded]
        private static void OnAdded(ref AccordionItems self, VisualElement owner)
        {
            var items = new TemplateItems((Accordion)owner);
            self._items = items;
            items.Rebuild(self.ItemTemplate, self.ItemsSource);
        }

        [OnComponentChanged]
        private static void OnChanged(ref AccordionItems self, VisualElement owner)
        {
            self._items.Rebuild(self.ItemTemplate, self.ItemsSource);
        }

        [OnComponentRemoved]
        private static void OnRemoved(ref AccordionItems self, VisualElement owner)
        {
            self._items.Clear();
        }

        private sealed class TemplateItems
        {
            private readonly Accordion _accordion;
            private readonly List<VisualElement> _roots = new();

            public TemplateItems(Accordion accordion)
            {
                _accordion = accordion;
            }

            public void Rebuild(VisualTreeAsset template, IList source)
            {
                Clear();
                if (template == null || source == null)
                {
                    return;
                }

                for (var index = 0; index < source.Count; index++)
                {
                    var firstChild = _accordion.childCount;
                    template.CloneTree(_accordion);

                    // Clone directly into the Accordion so its exclusive-open behavior still sees direct AccordionItem children.
                    for (var childIndex = firstChild; childIndex < _accordion.childCount; childIndex++)
                    {
                        var root = _accordion[childIndex];
                        root.dataSource = source[index];
                        _roots.Add(root);
                    }
                }
            }

            public void Clear()
            {
                foreach (var root in _roots)
                {
                    if (root.resourcesReleased || root.parent != _accordion)
                    {
                        continue;
                    }

                    root.RemoveFromHierarchy();
                    // Removal may run inside a generated row's activation callback; App UI still uses that row afterward.
                }

                _roots.Clear();
            }
        }
    }
}
