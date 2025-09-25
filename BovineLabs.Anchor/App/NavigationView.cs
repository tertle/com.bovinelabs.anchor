// // <copyright file="NavigationView.cs" company="BovineLabs">
// //     Copyright (c) BovineLabs. All rights reserved.
// // </copyright>
//
// namespace BovineLabs.Anchor
// {
//     using BovineLabs.Anchor.Nav;
//     using Unity.AppUI.Navigation;
//     using UnityEngine.UIElements;
//
//     [IsService]
//     public class NavigationView : VisualElement
//     {
//         public NavigationView(AnchorNavHost navHost)
//         {
//             this.pickingMode = PickingMode.Ignore;
//
//             this.Add(navHost);
//
//             navHost.navController.SetGraph(AnchorApp.current.GraphViewAsset);
//             navHost.visualController = AnchorApp.current.NavVisualController;
//             this.Controller = navHost.navController;
//
//             if (!AnchorApp.current.GraphViewAsset)
//             {
//                 this.style.display = DisplayStyle.None;
//                 return;
//             }
//
//             this.style.flexGrow = 1;
//             navHost.StretchToParentSize();
//         }
//
//         public NavController Controller { get; }
//     }
// }
