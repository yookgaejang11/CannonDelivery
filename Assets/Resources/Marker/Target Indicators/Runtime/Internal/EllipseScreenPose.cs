using System;
using UnityEngine;

namespace TargetIndicators
{
    class EllipseScreenPose
    {
        const float k_halfPi = (float)Math.PI * 0.5f;

        readonly ScreenData _screenData;

        float _ellipseSemiMajorAxisLength;
        float _eEllipseSemiMinorAxisLength;
        Vector2 _ellipseCenter;

        public EllipseScreenPose(ScreenData screenData)
        {
            _screenData = screenData;
        }

        public static Vector2 GetPaddedEllipseCenter(float leftPadding, float rightPadding, float topPadding, float bottomPadding)
        {
            return new Vector2
            {
                x = leftPadding + (Screen.width - leftPadding - rightPadding) * 0.5f,
                y = bottomPadding + (Screen.height - bottomPadding - topPadding) * 0.5f
            };
        }

        public static Vector2 GetAbsoluteEllipseCenter()
        {
            return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }

        public bool IsOutsidePaddedBoundary(in Vector3 screenPoint)
        {
            if (screenPoint.z < 0)
                return true;

            UpdatePaddedSizeData();

            var xMinusCenterX = screenPoint.x - _ellipseCenter.x;
            var yMinusCenterY = screenPoint.y - _ellipseCenter.y;

            var majorAxisLengthSquared = _ellipseSemiMajorAxisLength * _ellipseSemiMajorAxisLength;
            var minorAxisLengthSquared = _eEllipseSemiMinorAxisLength * _eEllipseSemiMinorAxisLength;

            var part1 = xMinusCenterX * xMinusCenterX / majorAxisLengthSquared;
            var part2 = yMinusCenterY * yMinusCenterY / minorAxisLengthSquared;
            return part1 + part2 > 1.0f;
        }

        public bool IsOutsideAbsoluteBoundary(in Vector3 screenPoint)
        {
            if (screenPoint.z < 0)
                return true;

            UpdateAbsoluteSizeData();

            var xMinusCenterX = screenPoint.x - _ellipseCenter.x;
            var yMinusCenterY = screenPoint.y - _ellipseCenter.y;

            var majorAxisLengthSquared = _ellipseSemiMajorAxisLength * _ellipseSemiMajorAxisLength;
            var minorAxisLengthSquared = _eEllipseSemiMinorAxisLength * _eEllipseSemiMinorAxisLength;

            var part1 = xMinusCenterX * xMinusCenterX / majorAxisLengthSquared;
            var part2 = yMinusCenterY * yMinusCenterY / minorAxisLengthSquared;
            return part1 + part2 > 1.0f;
        }

        public Pose GetPaddedScreenPose(in Vector3 worldSpacePosition, out bool isOutsideBoundary)
        {
            UpdatePaddedSizeData();

            var screenPoint = _screenData.Camera.WorldToScreenPoint(worldSpacePosition);
            if (screenPoint.z < 0)
            {
                screenPoint.x = Screen.width - screenPoint.x;
                screenPoint.y = Screen.height - screenPoint.y;
            }

            isOutsideBoundary = IsOutsidePaddedBoundary(screenPoint);
            if (isOutsideBoundary)
                screenPoint = ProjectOnEllipse(screenPoint);

            var screenPoint2D = new Vector2(screenPoint.x, screenPoint.y);
            var vectorToScreenPoint = (screenPoint2D - ScreenData.ScreenCenter).normalized;
            var angle = Mathf.Atan2(vectorToScreenPoint.y, vectorToScreenPoint.x) * Mathf.Rad2Deg;
            var rotation = Quaternion.Euler(0, 0, angle);

            return new Pose(screenPoint, rotation);
        }

        public Pose GetAbsoluteScreenPose(in Vector3 worldSpacePosition, out bool isOutsideBoundary)
        {
            UpdateAbsoluteSizeData();

            var screenPoint = _screenData.Camera.WorldToScreenPoint(worldSpacePosition);
            if (screenPoint.z < 0)
            {
                screenPoint.x = Screen.width - screenPoint.x;
                screenPoint.y = Screen.height - screenPoint.y;
            }

            isOutsideBoundary = IsOutsideAbsoluteBoundary(screenPoint);
            if (isOutsideBoundary)
                screenPoint = ProjectOnEllipse(screenPoint);

            var screenPoint2D = new Vector2(screenPoint.x, screenPoint.y);
            var vectorToScreenPoint = (screenPoint2D - (ScreenData.ScreenCenter)).normalized;
            var angle = Mathf.Atan2(vectorToScreenPoint.y, vectorToScreenPoint.x) * Mathf.Rad2Deg;
            var rotation = Quaternion.Euler(0, 0, angle);

            return new Pose(screenPoint, rotation);
        }

        void UpdatePaddedSizeData()
        {
            _ellipseSemiMajorAxisLength = (Screen.width - _screenData.LeftPadding - _screenData.RightPadding) * 0.5f;
            _eEllipseSemiMinorAxisLength = (Screen.height - _screenData.TopPadding - _screenData.BottomPadding) * 0.5f;

            _ellipseCenter = new Vector2
            {
                x = _screenData.LeftPadding + _ellipseSemiMajorAxisLength,
                y = _screenData.BottomPadding + _eEllipseSemiMinorAxisLength
            };
        }

        void UpdateAbsoluteSizeData()
        {
            _ellipseSemiMajorAxisLength = _screenData.Width * 0.5f;
            _eEllipseSemiMinorAxisLength = _screenData.Height * 0.5f;

            _ellipseCenter = GetAbsoluteEllipseCenter();
        }

        Vector2 ProjectOnEllipse(in Vector2 screenPoint)
        {
            // Equation to calculate coordinates on an ellipse given angle θ:
            // a = semiMajorAxisLength
            // b = semiMinorAxisLength
            // x = +- a * b / sqrt(b * b + a * a * tan(θ) * tan(θ)) where the sign is + if −π/2 < θ < π/2
            // y = x * tan(θ)
            var direction = screenPoint - _ellipseCenter;
            var angleRadians = Mathf.Atan2(direction.y, direction.x);
            var tanTheta = Mathf.Tan(angleRadians);
            var tanThetaSquared = tanTheta * tanTheta;

            var minorAxisLengthSquared = _eEllipseSemiMinorAxisLength * _eEllipseSemiMinorAxisLength;
            var majorAxisLengthSquared = _ellipseSemiMajorAxisLength * _ellipseSemiMajorAxisLength;

            var numerator = _ellipseSemiMajorAxisLength * _eEllipseSemiMinorAxisLength;
            var denominator = Mathf.Sqrt(minorAxisLengthSquared + majorAxisLengthSquared * tanThetaSquared);
            var x = numerator / denominator;
            x *= angleRadians is > -k_halfPi and < k_halfPi ? 1 : -1;
            var y = x * tanTheta;

            return _ellipseCenter + new Vector2(x, y);
        }
    }
}
