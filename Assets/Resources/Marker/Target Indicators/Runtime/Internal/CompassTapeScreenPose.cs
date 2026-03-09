using UnityEngine;

namespace TargetIndicators
{
    class CompassTapeScreenPose
    {
        readonly ScreenData _screenData;

        public CompassTapeScreenPose(ScreenData screenData)
        {
            _screenData = screenData;
        }

        public Pose GetScreenPoseForCompassTape(in Vector3 worldSpacePosition, out bool isOutsideBoundary)
        {
            var cameraForward = Vector3.ProjectOnPlane(_screenData.Camera.transform.forward, Vector3.up);
            cameraForward.Normalize();

            var directionToTarget = worldSpacePosition - _screenData.Camera.transform.position;
            directionToTarget = Vector3.ProjectOnPlane(directionToTarget, Vector3.up);
            directionToTarget.Normalize();

            var angle = Vector3.SignedAngle(directionToTarget, cameraForward, Vector3.up);
            // Add 180 to offset value by half the width. We want 0 to be to the left, 0.5 straight forward,
            // and 1 to be to the right.
            angle += 180;

            if (angle < 0)
                angle = 360 + angle;

            var x = angle / 360;

            isOutsideBoundary = false;
            return new Pose(new Vector3(1 - x, 0, 0), Quaternion.identity);
        }
    }
}
