// <copyright file="BoostrapToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE_1_3_3_OR_NEWER && BL_CORE_EXTENSIONS
namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Toolbar;
    using Unity.Properties;
    using UnityEngine.UIElements;
    using Toggle = Unity.AppUI.UI.Toggle;

    [AutoToolbar("Bootstrap")]
    public class BoostrapToolbarView : VisualElement, IView<BootstrapToolbarViewModel>
    {
        public const string UssClassName = "bl-net-bootstrap-tab";

        public BoostrapToolbarView()
        {
            this.AddToClassList(UssClassName);

#if !UNITY_CLIENT
#if UNITY_NETCODE
            var server = new Toggle
            {
                label = "Server",
                dataSource = this.ViewModel,
            };

            server.SetBinding(nameof(Toggle.value), new DataBinding { dataSourcePath = new PropertyPath(nameof(BootstrapToolbarViewModel.Server)) });
            this.Add(server);
#endif
#endif

#if !UNITY_SERVER
#if UNITY_NETCODE
            var client = new Toggle
            {
                label = "Client",
                dataSource = this.ViewModel,
            };

            client.SetBinding(nameof(Toggle.value), new DataBinding { dataSourcePath = new PropertyPath(nameof(BootstrapToolbarViewModel.Client)) });
            this.Add(client);
#endif

            var game = new Toggle
            {
                label = "Local",
                dataSource = this.ViewModel,
            };

            game.SetBinding(nameof(Toggle.value), new DataBinding { dataSourcePath = new PropertyPath(nameof(BootstrapToolbarViewModel.Local)) });
            this.Add(game);
#endif
        }

        public BootstrapToolbarViewModel ViewModel { get; } = new();
    }
}
#endif
