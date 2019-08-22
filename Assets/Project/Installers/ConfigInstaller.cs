using BPC.Config;
using UnityEngine;
using Zenject;

namespace BPC.Installers
{
    [CreateAssetMenu(menuName = "BPC/ConfigInstaller", fileName = "ConfigInstaller")]
    public class ConfigInstaller : ScriptableObjectInstaller
    {
        [SerializeField]
        private GameConfig _gameConfig;

        public override void InstallBindings()
        {
            Container.BindInstance(_gameConfig.BoardSetting);
            Container.BindInstance(_gameConfig.BubbleSetting);
            Container.BindInstance(_gameConfig.InputSetting);
            Container.BindInstance(_gameConfig.SfxSetting);
        }
    }
}