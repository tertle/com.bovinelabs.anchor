// <copyright file="BindableSliderFloat.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
    using Unity.AppUI.UI;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class BindableSliderFloat : SliderFloat
    {
        // protected override void SetValueFromDrag(float newPos)
        // {
        //     var sliderRect = this.GetSliderRect();
        //     var newValue = this.ComputeValueFromHandlePosition(sliderRect.width, newPos - sliderRect.x);
        //
        //     this.value = newValue;
        // }
    }
}
