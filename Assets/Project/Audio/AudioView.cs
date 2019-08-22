using System;
using DB.Library.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

namespace BPC.Audio
{
    public class AudioView : ViewBase<AudioViewModel>, IPoolable<AudioClip, AudioMixerGroup, float, float, IMemoryPool>
    {
        [SerializeField]
        private AudioSource _audioSource;

        private IMemoryPool _pool;
        private SerialDisposable _serialDisposable;

        protected override void SetUp()
        {
            _serialDisposable = new SerialDisposable().AddTo(Disposer);
            base.SetUp();
        }

        protected override void BindSubscriptions(CompositeDisposable disposer)
        {
        }

        public void OnDespawned()
        {
            _pool = null;
            _audioSource.Stop();
        }

        public void OnSpawned(AudioClip clip, AudioMixerGroup mixerGroup, float pitch, float delay, IMemoryPool pool)
        {
            _pool = pool;
            _audioSource.clip = clip;
            _audioSource.pitch = pitch;
            _audioSource.outputAudioMixerGroup = mixerGroup;
            _serialDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(delay)).Subscribe(_ =>
            {
                _audioSource.Play();
                _serialDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(clip.length + 0.1f))
                    .Subscribe(__ => _pool.Despawn(this));
            });
        }

        public class Factory : PlaceholderFactory<AudioClip, AudioMixerGroup, float, float, AudioView>
        {
        }

        public class MemoryPool : MonoPoolableMemoryPool<AudioClip, AudioMixerGroup, float, float, IMemoryPool,
            AudioView>
        {
        }
    }
}