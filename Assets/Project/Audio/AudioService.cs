using System;
using BPC.Config;
using Zenject;
using Random = UnityEngine.Random;

namespace BPC.Audio
{
    public enum SfxId
    {
        Shot,
        Pop,
        Explosion
    }

    public class AudioService
    {
        [Inject]
        private GameConfig.SfxSettings _sfxSettings;

        [Inject]
        private AudioView.Factory _audioViewFactory;

        public void PlaySfx(SfxId sfxId)
        {
            var sfxInfo = _sfxSettings.GetSfxInfo(sfxId);

            switch (sfxId)
            {
                case SfxId.Shot:
                    _audioViewFactory.Create(sfxInfo.AudioClip, sfxInfo.MixerGroup, Random.Range(0.8f, 1.1f), 0f);
                    break;
                case SfxId.Pop:
                    _audioViewFactory.Create(sfxInfo.AudioClip, sfxInfo.MixerGroup, Random.Range(0.9f, 1.1f),
                        Random.Range(0f, 0.2f));
                    break;
                case SfxId.Explosion:
                    _audioViewFactory.Create(sfxInfo.AudioClip, sfxInfo.MixerGroup, 1f, 0f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sfxId), sfxId, null);
            }
        }
    }
}