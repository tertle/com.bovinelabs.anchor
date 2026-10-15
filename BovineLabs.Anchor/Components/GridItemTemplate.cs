namespace BovineLabs.Anchor.Components
{
    using System;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.UIElements;

    [VisualElementComponent]
    [RequiresElementOfType(typeof(GridView))]
    public partial struct GridItemTemplate : IVisualElementComponent
    {
        [CreateProperty]
        [UxmlAttribute("item-template")]
        public VisualTreeAsset ItemTemplate;

        private TemplateBinding _binding;

        [OnComponentAdded]
        private static void OnAdded(ref GridItemTemplate self, VisualElement owner)
        {
            var binding = new TemplateBinding((GridView)owner, self.ItemTemplate);
            self._binding = binding;
            binding.Install();
        }

        [OnComponentChanged]
        private static void OnChanged(ref GridItemTemplate self, VisualElement owner)
        {
            self._binding.SetTemplate(self.ItemTemplate);
        }

        [OnComponentRemoved]
        private static void OnRemoved(ref GridItemTemplate self, VisualElement owner)
        {
            self._binding.Restore();
        }

        private sealed class TemplateBinding
        {
            private readonly GridView _grid;
            private readonly Func<VisualElement> _previousMakeItem;
            private readonly Action<VisualElement, int> _previousBindItem;
            private readonly Func<VisualElement> _makeItem;
            private readonly Action<VisualElement, int> _bindItem;
            private VisualTreeAsset _template;
            private bool _removed;

            public TemplateBinding(GridView grid, VisualTreeAsset template)
            {
                _grid = grid;
                _template = template;
                _previousMakeItem = grid.makeItem;
                _previousBindItem = grid.bindItem;
                _makeItem = MakeItem;
                _bindItem = BindItem;
            }

            public void Install()
            {
                // The generic data-source binder also accepts any rows made by the previous factory during this refresh.
                _grid.bindItem = _bindItem;
                if (!_removed)
                {
                    _grid.makeItem = _makeItem;
                }
            }

            public void SetTemplate(VisualTreeAsset template)
            {
                _template = template;
                if (_grid.makeItem == _makeItem)
                {
                    _grid.Refresh();
                }
            }

            public void Restore()
            {
                if (_removed)
                {
                    return;
                }

                _removed = true;
                var restoreMakeItem = _grid.makeItem == _makeItem;
                var restoreBindItem = _grid.bindItem == _bindItem;

                if (restoreMakeItem)
                {
                    var bindItem = restoreBindItem ? _previousBindItem : _grid.bindItem;
                    // Avoid invoking the previous binder on rows from our template while the callbacks are exchanged.
                    _grid.bindItem = null;
                    _grid.makeItem = _previousMakeItem;
                    _grid.bindItem = bindItem;
                }
                else if (restoreBindItem)
                {
                    _grid.bindItem = _previousBindItem;
                }
            }

            private VisualElement MakeItem()
            {
                return _template != null ? _template.Instantiate() : new Label("Template Not Found");
            }

            private void BindItem(VisualElement element, int index)
            {
                element.dataSource = _grid.itemsSource[index];
            }
        }
    }
}
