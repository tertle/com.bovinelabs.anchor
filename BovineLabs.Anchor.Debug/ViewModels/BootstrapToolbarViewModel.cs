// <copyright file="BootstrapToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE
namespace BovineLabs.Anchor.Debug.ViewModels
{
    using BovineLabs.Core;
    using Unity.AppUI.MVVM;
    using Unity.Properties;

    public class BootstrapToolbarViewModel : ObservableObject
    {
#if !UNITY_SERVER
        [CreateProperty]
        public bool Local
        {
            get => BovineLabsBootstrap.GameWorld != null;
            set => this.SetProperty(this.Local, value, value =>
            {
                if (value)
                {
                    BovineLabsBootstrap.Instance.CreateGameWorld();
                }
                else
                {
                    BovineLabsBootstrap.Instance.DestroyGameWorld();
                }
            });
        }

#if UNITY_NETCODE
        [CreateProperty]
        public bool Client
        {
            get => Unity.NetCode.ClientServerBootstrap.ClientWorld != null;
            set
            {
                this.SetProperty(this.Client, value, value =>
                {
                    if (value)
                    {
                        BovineLabsBootstrap.Instance.CreateClientWorld();
                    }
                    else
                    {
                        BovineLabsBootstrap.Instance.DestroyClientWorld();
                    }
                });
            }
        }
#endif
#endif

#if !UNITY_CLIENT
#if UNITY_NETCODE
        [CreateProperty]
        public bool Server
        {
            get => Unity.NetCode.ClientServerBootstrap.ServerWorld != null;
            set => this.SetProperty(this.Server, value, value =>
            {
                if (value)
                {
                    BovineLabsBootstrap.Instance.CreateServerWorld();
                }
                else
                {
                    BovineLabsBootstrap.Instance.DestroyServerWorld();
                }
            });
        }
#endif
#endif
    }
}
#endif
