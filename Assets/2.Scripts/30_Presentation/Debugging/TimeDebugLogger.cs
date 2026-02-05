using UnityEngine;
using Hourbound.Domain.Time;
using Hourbound.Presentation.Time;

namespace Hourbound.Presentation.Debugging
{
    public sealed class TimeDebugLogger : MonoBehaviour
    {
        [SerializeField] private TimeResourcePresenter presenter;

        private ITimeResource _time;

        private void Awake()
        {
            if (presenter == null) presenter = FindFirstObjectByType<TimeResourcePresenter>();
            _time = presenter != null ? presenter.Time : null;
        }

        private void OnEnable()
        {
            if (_time == null) return;
            _time.Changed += OnChanged;
        }

        private void OnDisable()
        {
            if (_time == null) return;
            _time.Changed -= OnChanged;
        }

        private void OnChanged(TimeChangedArgs e)
        {
            Debug.Log($"[TIME] {e.ChangeType} {e.Before:F2}->{e.After:F2} (delta={e.Delta:F2}) ctx={e.Context}", this);
        }
    }
}