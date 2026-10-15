namespace BovineLabs.Anchor.Tests.TestDoubles
{
    using UnityEngine.UIElements;

    internal sealed class TestContentContainerElement : VisualElement
    {
        public TestContentContainerElement()
        {
            hierarchy.Add(Content);
            hierarchy.Add(Decoration);
            Add(Leaf);
        }

        public VisualElement Content { get; } = new();

        public VisualElement Decoration { get; } = new();

        public VisualElement Leaf { get; } = new();

        public override VisualElement contentContainer => Content;
    }
}
