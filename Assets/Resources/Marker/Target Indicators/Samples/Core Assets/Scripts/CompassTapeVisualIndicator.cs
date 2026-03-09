using System;
using UnityEngine;

namespace TargetIndicators.Samples
{
    /// <summary>
    /// Control the instance of a compass tape visual indicator.
    /// </summary>
    public class CompassTapeVisualIndicator : VisualIndicator
    {
        /// <summary>
        /// The length of the visible tape.
        /// </summary>
        public float Length { get; set; }

        /// <summary>
        /// The length ratio between the full tape and visible tape. If the full tape is twice as long as the visible tape
        /// this value should be 2.
        /// </summary>
        public float FullTapeToVisibleTapeRatio { get; set; }

        /// <inheritdoc/>
        public override void UpdateVisualIndicator(TargetIndicator targetIndicator)
        {
            UpdateVisualIndicator(targetIndicator.ScreenPose, targetIndicator.IsOutsideBoundary);
        }

        /// <inheritdoc/>
        public override void UpdateVisualIndicator(Pose screenPose, bool isOutsideBoundary)
        {
            screenPose.position.x %= 1f;

            var positionOnFullTape = screenPose.position.x * (FullTapeToVisibleTapeRatio * Length);
            var middleOfFullTape = (FullTapeToVisibleTapeRatio * Length) * 0.5f;
            var middleToPointOnFullTape = positionOnFullTape - middleOfFullTape;
            var middleOfVisibleTape = Length * 0.5f;
            var pointOnVisibleTape = middleOfVisibleTape + middleToPointOnFullTape;

            screenPose.position.x = pointOnVisibleTape;
            screenPose.position.y = 0;

            isOutsideBoundary = pointOnVisibleTape < 0 || pointOnVisibleTape > Length;

            _rectTransform.anchoredPosition = screenPose.position;

            switch (_coreContentVisibility)
            {
                case IndicatorVisibility.Never:
                    _contentGO.SetActive(false);
                    break;
                case IndicatorVisibility.Always:
                    screenPose.position.x = Mathf.Clamp(pointOnVisibleTape, 0, Length);
                    _rectTransform.anchoredPosition = screenPose.position;
                    _contentGO.SetActive(true);
                    break;
                case IndicatorVisibility.OutsideBoundary:
                    screenPose.position.x = Mathf.Clamp(pointOnVisibleTape, 0, Length);
                    _rectTransform.anchoredPosition = screenPose.position;
                    _contentGO.SetActive(isOutsideBoundary);
                    break;
                case IndicatorVisibility.InsideBoundary:
                    _contentGO.SetActive(!isOutsideBoundary);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _rotationContentGO.SetActive(false);
        }

        /// <inheritdoc/>
        protected override void SetAnchorsAndPivot()
        {
            _rectTransform.anchorMin = new Vector2(0f, 0.5f);
            _rectTransform.anchorMax = new Vector2(0f, 0.5f);
            _rectTransform.pivot = Vector2.one * 0.5f;
        }
    }
}
