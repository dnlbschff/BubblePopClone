using System.Collections.Generic;
using System.Linq;
using DB.Library.MVVM;
using UniRx;
using UnityEngine;

namespace BPC.Input
{
    public class InputView : ViewBase<InputViewModel>
    {
        [SerializeField] private LineRenderer _lineRenderer;

        protected override void BindSubscriptions(CompositeDisposable disposer)
        {
            ViewModel.SetTransform(transform);

            ViewModel.TrajectoryPoints
                .Subscribe(OnTrajectoryChanged).AddTo(disposer);

            ViewModel.HasValidTarget
                .Subscribe(hasValidTarget => _lineRenderer.enabled = hasValidTarget).AddTo(Disposer);
        }

        private void OnTrajectoryChanged(IReadOnlyList<Vector3> trajectoryPoints)
        {
            _lineRenderer.positionCount = trajectoryPoints.Count;
            _lineRenderer.SetPositions(trajectoryPoints.ToArray());
        }
    }
}