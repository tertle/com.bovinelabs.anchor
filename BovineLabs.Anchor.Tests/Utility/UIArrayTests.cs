namespace BovineLabs.Anchor.Tests.Utility
{
    using NUnit.Framework;
    using Unity.Collections;

    public class UIArrayTests
    {
        [Test]
        public void CopyTo_ObjectArray_BoxesEveryElement()
        {
            var nativeArray = new NativeArray<int>(new[] { 4, 5 }, Allocator.Temp);
            var uiArray = (UIArray<int>)(MultiContainer<int>)nativeArray;
            var destination = new object[2];

            uiArray.CopyTo(destination, 0);

            CollectionAssert.AreEqual(new object[] { 4, 5 }, destination);
        }
    }
}
