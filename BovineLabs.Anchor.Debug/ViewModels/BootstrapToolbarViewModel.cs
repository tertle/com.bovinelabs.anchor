// <copyright file="BootstrapToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE_1_3_3_OR_NEWER && BL_CORE_EXTENSIONS
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
            set =>
                this.SetProperty(this.Local, value, value =>
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

        [CreateProperty]
        public bool LocalEnabled
        {
            get
            {
#if UNITY_NETCODE
                return !this.Client && !this.Server;
#else
                return true;
#endif
            }
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

        [CreateProperty]
        public bool ClientEnabled => !this.Local;

#endif // UNITY_NETCODE
#endif // !UNITY_SERVER

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

        [CreateProperty]
        public bool ServerEnabled
        {
            get
            {
#if !UNITY_SERVER
                return !this.Local;
#else
                return true;
#endif
            }
        }
#endif
#endif
    }
}
#endif
