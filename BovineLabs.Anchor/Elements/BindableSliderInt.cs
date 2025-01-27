// <copyright file="BindableSliderInt.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
    using UnityEngine.UIElements;
    using SliderInt = Unity.AppUI.UI.SliderInt;

    [UxmlElement]
    public partial class BindableSliderInt : SliderInt
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
