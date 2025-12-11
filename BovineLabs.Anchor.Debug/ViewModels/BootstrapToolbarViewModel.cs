// <copyright file="BootstrapToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE_1_3_3_OR_NEWER && BL_CORE_EXTENSIONS
namespace BovineLabs.Anchor.Debug.ViewModels
{
    using BovineLabs.Core;
    using Unity.AppUI.MVVM;
#if UNITY_NETCODE
    using Unity.NetCode;
#endif
    using Unity.Properties;

    public class BootstrapToolbarViewModel : ObservableObject
    {
#if UNITY_NETCODE
#if !UNITY_CLIENT && !UNITY_SERVER
        private bool host;
        private bool hostEnabled;
#endif

#if !UNITY_SERVER
        private bool client;
        private bool clientEnabled;
#endif

#if !UNITY_CLIENT
        private bool server;
        private bool serverEnabled;
#endif
#else
        private bool local;
#endif

#if UNITY_NETCODE
#if !UNITY_CLIENT && !UNITY_SERVER
        [CreateProperty]
        public bool Host
        {
            get => this.host;
            set
            {
                if (this.SetProperty(ref this.host, value))
                {
                    if (value)
                    {
                        BovineLabsBootstrap.Instance.CreateClientServerWorlds(false);
                    }
                    else
                    {
                        BovineLabsBootstrap.Instance.DestroyClientServerWorlds();
                    }
                }
            }
        }

        [CreateProperty(ReadOnly = true)]
        public bool HostEnabled
        {
            get => this.hostEnabled;
            set => this.SetProperty(ref this.hostEnabled, value);
        }
#endif

#if !UNITY_SERVER
        [CreateProperty]
        public bool Client
        {
            get => this.client;
            set
            {
                if (this.SetProperty(ref this.client, value))
                {
                    if (value)
                    {
                        if (ClientServerBootstrap.ClientWorld == null)
                        {
                            BovineLabsBootstrap.Instance.CreateClientWorld();
                        }
                    }
                    else
                    {
                        if (ClientServerBootstrap.ClientWorld != null)
                        {
                            BovineLabsBootstrap.Instance.DestroyClientWorld();
                        }
                    }
                }
            }
        }

        [CreateProperty(ReadOnly = true)]
        public bool ClientEnabled
        {
            get => this.clientEnabled;
            set => this.SetProperty(ref this.clientEnabled, value);
        }

#endif // !UNITY_SERVER

#if !UNITY_CLIENT
        [CreateProperty]
        public bool Server
        {
            get => this.server;
            set
            {
                if (this.SetProperty(ref this.server, value))
                {
                    if (value)
                    {
                        if (ClientServerBootstrap.ServerWorld == null)
                        {
                            BovineLabsBootstrap.Instance.CreateServerWorld();
                        }
                    }
                    else
                    {
                        if (ClientServerBootstrap.ServerWorld == null)
                        {
                            BovineLabsBootstrap.Instance.DestroyServerWorld();
                        }
                    }
                }
            }
        }

        [CreateProperty(ReadOnly = true)]
        public bool ServerEnabled
        {
            get => this.serverEnabled;
            set => this.SetProperty(ref this.serverEnabled, value);
        }
#endif
#else
        [CreateProperty]
        public bool Local
        {
            get => this.local;
            set
            {
                if (this.SetProperty(ref this.local, value))
                {
                    if (value)
                    {
                        BovineLabsBootstrap.Instance.CreateGameWorld();
                    }
                    else
                    {
                        BovineLabsBootstrap.Instance.DestroyGameWorld();
                    }
                }
            }
        }
#endif

        public void Update()
        {
#if UNITY_NETCODE
            var clientWorldExists = ClientServerBootstrap.ClientWorld != null;
            var serverWorldExists = ClientServerBootstrap.ServerWorld != null;


#if UNITY_NETCODE && !UNITY_CLIENT && !UNITY_SERVER
            this.Host = clientWorldExists && serverWorldExists;
            this.HostEnabled = clientWorldExists == serverWorldExists;
#endif

#if UNITY_NETCODE && !UNITY_SERVER
            this.Client = clientWorldExists;
            this.ClientEnabled = !serverWorldExists;
#endif

#if UNITY_NETCODE && !UNITY_CLIENT
            this.Server = serverWorldExists;
#if !UNITY_SERVER
            this.ServerEnabled = !clientWorldExists;
#else
            this.ServerEnabled = true;
#endif
#endif
#else
            this.Local = ClientServerBootstrap.GameWorld != null;
#endif
        }
    }
}
#endif
