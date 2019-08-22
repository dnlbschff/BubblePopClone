using System;
using BPC.Config;
using DB.Library.MVVM;
using UniRx;
using UnityEngine;
using Zenject;

namespace BPC.Bubbles
{
    public class BubbleBurstView : ViewBase<BubbleBurstViewModel>, IPoolable<int, Vector3, IMemoryPool>
    {
        [Inject]
        private GameConfig.BubbleSettings _bubbleSetting;

        [SerializeField] private ParticleSystem _particleSystem;

        private IMemoryPool _pool;
        private ParticleSystem.MainModule _particleSystemMain;
        private SerialDisposable _serialDisposable;

        protected override void SetUp()
        {
            _particleSystemMain = _particleSystem.main;
            _serialDisposable = new SerialDisposable().AddTo(Disposer);
            base.SetUp();
        }

        protected override void BindSubscriptions(CompositeDisposable disposer)
        {
            ViewModel.Exponent
                .Subscribe(OnExponentChanged)
                .AddTo(Disposer);
        }

        private void OnExponentChanged(int exponent)
        {
            _particleSystemMain.startColor = _bubbleSetting.GetBubbleColor(exponent);
        }

        public void OnDespawned()
        {
            _pool = null;
        }

        public void OnSpawned(int exponent, Vector3 position, IMemoryPool pool)
        {
            transform.position = position;
            _pool = pool;
            ViewModel.SetExponent(exponent);
            _particleSystem.Stop();
            _particleSystem.Play();

            _serialDisposable.Disposable =
                Observable.Timer(TimeSpan.FromSeconds(_particleSystemMain.duration))
                    .Subscribe(_ => _pool.Despawn(this));
        }

        public class Factory : PlaceholderFactory<int, Vector3, BubbleBurstView>
        {
        }

        public class MemoryPool : MonoPoolableMemoryPool<int, Vector3, IMemoryPool, BubbleBurstView>
        {
        }
    }
}