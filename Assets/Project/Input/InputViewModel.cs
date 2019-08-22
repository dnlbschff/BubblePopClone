using System;
using System.Collections.Generic;
using BPC.Board;
using BPC.Bubbles;
using BPC.Config;
using BPC.Utils;
using DB.Library.MVVM;
using Extensions;
using UniRx;
using UnityEngine;

namespace BPC.Input
{
    public class InputViewModel : ViewModelBase
    {
        private readonly ReactiveProperty<bool> _isAiming;
        public IReadOnlyReactiveProperty<bool> IsAiming => _isAiming;

        private readonly ReactiveProperty<Vector2> _aimPosition;
        public IReadOnlyReactiveProperty<Vector2> AimPosition => _aimPosition;

        private readonly ReactiveProperty<IReadOnlyList<Vector3>> _trajectoryPoints;
        public IReadOnlyReactiveProperty<IReadOnlyList<Vector3>> TrajectoryPoints => _trajectoryPoints;

        private readonly ReactiveProperty<bool> _hasValidTarget;
        public IReadOnlyReactiveProperty<bool> HasValidTarget => _hasValidTarget;

        private readonly ReactiveProperty<Vector2Int> _targetGridPos;
        public IReadOnlyReactiveProperty<Vector2Int> TargetGridPos => _targetGridPos;

        private Subject<Unit> _shootSubject;
        public IObservable<Unit> ShootObservable => _shootSubject;

        private readonly Camera _camera;
        private readonly BoardViewModel _boardViewModel;
        private readonly BoardView _boardView;
        private Transform _transform;
        private readonly List<Vector3> _linePositions = new List<Vector3>(3);
        private readonly RaycastHit2D[] _raycastResults = new RaycastHit2D[1];
        private bool _canShoot = true;
        private SerialDisposable _canShootTimerDisposable;
        private float _shootInterval;

        public InputViewModel(Camera camera, BoardViewModel boardViewModel, BoardView boardView, GameConfig.InputSettings inputSetting)
        {
            _shootInterval = inputSetting.ShootInterval;
            _camera = camera;
            _boardViewModel = boardViewModel;
            _boardView = boardView;
            _isAiming = new ReactiveProperty<bool>().AddTo(Disposer);
            _aimPosition = new ReactiveProperty<Vector2>().AddTo(Disposer);
            _trajectoryPoints = new ReactiveProperty<IReadOnlyList<Vector3>>(_linePositions);

            _hasValidTarget = new ReactiveProperty<bool>().AddTo(Disposer);
            _targetGridPos = new ReactiveProperty<Vector2Int>().AddTo(Disposer);
            
            _shootSubject = new Subject<Unit>().AddTo(Disposer);
            _canShootTimerDisposable = new SerialDisposable().AddTo(Disposer);

            Observable.EveryUpdate()
                .Subscribe(_ => UpdateInput()).AddTo(Disposer);

            AimPosition
                .SkipLatestValueOnSubscribe()
                .Subscribe(OnAimPositionChanged).AddTo(Disposer);

            IsAiming
                .SkipLatestValueOnSubscribe()
                .IfFalse()
                .Do(_ => Shoot())
                .DelayFrame(1)
                .Subscribe(_ => _hasValidTarget.Value = false).AddTo(Disposer);
        }

        private void Shoot()
        {
            if (_canShoot)
            {
                _shootSubject.OnNext(Unit.Default);
                _canShoot = false;
                _canShootTimerDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(_shootInterval))
                    .Subscribe(_ => _canShoot = true);
            }
        }

        public void SetTransform(Transform transform)
        {
            _transform = transform;
        }

        private void UpdateInput()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                _isAiming.Value = true;
            }

            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                _isAiming.Value = false;
            }

            if (_isAiming.Value)
            {
                _aimPosition.SetValueAndForceNotify(_camera.ScreenToWorldPoint(UnityEngine.Input.mousePosition));
            }
        }

        private void OnAimPositionChanged(Vector2 aimPosition)
        {
            var position2d = _transform.position.ToVector2();
            var direction = aimPosition - position2d;

            _linePositions.Clear();
            _linePositions.Add(position2d);

            Physics2D.RaycastNonAlloc(position2d, direction, _raycastResults);
            var raycastHit = _raycastResults[0];
            _linePositions.Add(raycastHit.point);

            if (raycastHit.collider.gameObject.CompareTag("Walls"))
            {
                var wallHitPosition =
                    raycastHit.point + 0.0001f * raycastHit.normal; // * float.Epsilon is not safe enough 
                var reflectedDirection = Vector3.Reflect(direction, raycastHit.normal);

                Physics2D.RaycastNonAlloc(wallHitPosition, reflectedDirection, _raycastResults);
                raycastHit = _raycastResults[0];
                _linePositions.Add(raycastHit.point);
            }

            _trajectoryPoints.SetValueAndForceNotify(_linePositions);

            UpdateTargetPosition(raycastHit);
        }

        private void UpdateTargetPosition(RaycastHit2D raycastHit)
        {
            var isHittingBubble = raycastHit.collider.gameObject.CompareTag("Bubble");
            var isHittingTopBoundary = raycastHit.collider.gameObject.CompareTag("TopBoundary");
            _hasValidTarget.Value = isHittingBubble || isHittingTopBoundary;


            if (isHittingBubble)
            {
                var targetGridPos = GetTargetGridPositionFromBubbleHit(raycastHit);
                if (!_boardViewModel.IsFree(targetGridPos) || !_boardViewModel.IsWithinBounds(targetGridPos)) return;

                _targetGridPos.Value = targetGridPos;
            }

            if (isHittingTopBoundary)
            {
                var localPos = _boardView.BubblesParent.InverseTransformPoint(raycastHit.point);
                var row = Mathf.RoundToInt(-localPos.y / 0.88f);
                var col = Mathf.RoundToInt(localPos.x + 0.5f *(Mathf.Abs(row) % 2)) * 2 - (Mathf.Abs(row) % 2);

                var targetGridPos = new Vector2Int(col, row);
                if (!_boardViewModel.IsFree(targetGridPos) || !_boardViewModel.IsWithinBounds(targetGridPos)) return;

                _targetGridPos.Value = targetGridPos;
            }
        }

        private Vector2Int GetTargetGridPositionFromBubbleHit(RaycastHit2D raycastHit)
        {
            var popViewModel = raycastHit.collider.gameObject.GetComponent<BubbleView>().ViewModel;

            var normal = raycastHit.normal;
            var angle = Vector2.SignedAngle(Vector2.right, normal);

            var neighborLocal = HexGridUtils.AngleToHexGridNeighborGridPos(angle);
            var neighbor = neighborLocal + popViewModel.GridPosition.Value;
            return neighbor;
        }
    }
}