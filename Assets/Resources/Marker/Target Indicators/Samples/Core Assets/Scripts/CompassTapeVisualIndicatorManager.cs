using System;
using UnityEngine;

namespace TargetIndicators.Samples
{
    /// <summary>
    /// Manages target indicators created by a <see cref="TargetIndicatorManager"/> for compass tape indicators by
    /// instantiating visual indicators and updating their position in the UI. Supports only `CompassTape` boundary types.
    /// </summary>
    public class CompassTapeVisualIndicatorManager : VisualIndicatorManager
    {
        [SerializeField, Tooltip("The size of the full tape relative to the visible tape. For example, if the full tape " +
                                 "is twice the size of the visible tape then this value should be 2.")]
        float _fullTapeToVisibleTapeRatio;

        /// <summary>
        /// The length ratio between the full tape and visible tape. If the full tape is twice as long as the visible tape
        /// this value should be 2.
        /// </summary>
        public float FullTapeToVisibleTapeRatio
        {
            get => _fullTapeToVisibleTapeRatio;
            set => _fullTapeToVisibleTapeRatio = value;
        }

        /// <inheritdoc/>
        protected override void OnTargetIndicatorsAdded(ReadOnlySpan<TargetIndicator> addedTargetIndicators)
        {
            if (_addIndicatorMode == AddIndicatorMode.Manual)
                return;

            if (_targetIndicatorManager.BoundaryType != BoundaryType.CompassTape)
            {
                if (_warningLogged)
                    return;

                Debug.LogWarning(
                    $"{nameof(CompassTapeVisualIndicatorManager)} can only display {nameof(BoundaryType.CompassTape)} " +
                    $"target indicators. Use the {nameof(VisualIndicatorManager)} with a {nameof(VisualIndicator)} or create " +
                    $"your own system for displaying target indicator pose updates when " +
                    $"{nameof(_targetIndicatorManager.BoundaryShape)} is not set to {nameof(BoundaryType.CompassTape)}.)",
                    this);

                return;
            }

            _warningLogged = false;

            foreach (var targetIndicator in addedTargetIndicators)
            {
                var uiTargetIndicator = Instantiate(DefaultVisualIndicatorPrefab, _content);
                var uiCompassTapeTargetIndicator = uiTargetIndicator as CompassTapeVisualIndicator;
                if (uiCompassTapeTargetIndicator == null)
                    continue;

                uiTargetIndicator.TargetIndicatorId = targetIndicator.Id;
                uiCompassTapeTargetIndicator.Length = _content.rect.width;
                uiCompassTapeTargetIndicator.FullTapeToVisibleTapeRatio = FullTapeToVisibleTapeRatio;
                uiCompassTapeTargetIndicator.UpdateVisualIndicator(targetIndicator);

                _trackedUITargetIndicators.Add(targetIndicator.Id, uiCompassTapeTargetIndicator);
            }
        }

        /// <inheritdoc/>
        protected override void OnTargetIndicatorsUpdated(ReadOnlySpan<TargetIndicator> updatedTargetIndicators)
        {
            if (_targetIndicatorManager.BoundaryType != BoundaryType.CompassTape)
            {
                if (_warningLogged)
                    return;

                Debug.LogWarning(
                    $"{nameof(CompassTapeVisualIndicatorManager)} can only display {nameof(BoundaryType.CompassTape)} " +
                    $"target indicators. Use the {nameof(VisualIndicatorManager)} with a {nameof(VisualIndicator)} or create " +
                    $"your own system for displaying target indicator pose updates when " +
                    $"{nameof(_targetIndicatorManager.BoundaryShape)} is not set to {nameof(BoundaryType.CompassTape)}.)",
                    this);

                return;
            }

            _warningLogged = false;

            foreach (var targetIndicator in updatedTargetIndicators)
            {
                if (!_trackedUITargetIndicators.TryGetValue(targetIndicator.Id, out var uiTargetIndicator))
                    continue;

                var uiCompassTapeTargetIndicator = uiTargetIndicator as CompassTapeVisualIndicator;
                if (uiCompassTapeTargetIndicator == null)
                    continue;

                uiCompassTapeTargetIndicator.Length = _content.rect.width;
                uiCompassTapeTargetIndicator.FullTapeToVisibleTapeRatio = FullTapeToVisibleTapeRatio;
                uiCompassTapeTargetIndicator.UpdateVisualIndicator(targetIndicator);
            }
        }
    }
}
