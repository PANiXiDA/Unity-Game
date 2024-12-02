﻿using Zenject;

namespace Assets.Scripts.UI.MultiPlayerLobby.Extensions
{
    public class MultiPlayerLobbyInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ProfileManager>().FromComponentInHierarchy().AsSingle();
        }
    }
}
