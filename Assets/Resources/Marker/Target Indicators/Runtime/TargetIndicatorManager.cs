using System;
using System.Collections.Generic;
using UnityEngine;

namespace TargetIndicators
{
    /// <summary>
    /// Tracks targets and transforms their world space positions into screen space coordinates with options to clamp to
    /// the screen edges, absolute size, or a compass tape. Useful for displaying directions to targets and other
    /// positions in the environment.
    /// </summary>
    [HelpURL("https://jakemanfre.github.io/target-indicators.github.io/manual/user_guide/target-indicator-manager.html")]
    public class TargetIndicatorManager : MonoBehaviour
    {
        const int k_maxTargets = 100;

        [Header("Scene References")]
        [SerializeField, Tooltip("The camera used to calculate screen point coordinates. Defaults to the camera tagged " +
                                 "\"MainCamera\" and falls back to any camera in the scene.")]
        Camera _camera;

        [Header("Settings")]
        [SerializeField, Tooltip("The type of boundary that target indicators will clamp to.")]
        BoundaryType _boundaryType;

        [Header("Configuration")]
        [SerializeField, Tooltip("The shape of the boundary that target indicators will clamp to. Only Padded and Absolute" +
                                 "boundary types use this setting.")]
        BoundaryShape _boundaryShape;

        [SerializeField, Tooltip("The distance in pixels from the top edge of the screen to clamp to for padded boundaries.")]
        float _topPadding;

        [SerializeField, Tooltip("The distance in pixels from the bottom edge of the screen to clamp to for padded boundaries.")]
        float _bottomPadding;

        [SerializeField, Tooltip("The distance in pixels from the left edge of the screen to clamp to for padded boundaries.")]
        float _leftPadding;

        [SerializeField, Tooltip("The distance in pixels from the right edge of the screen to clamp to for padded boundaries.")]
        float _rightPadding;

        [SerializeField, Tooltip("Absolute width of the boundary. This value is ignored if boundary type is not set to Absolute or WorldSpace.")]
        float _width = 300;

        [SerializeField, Tooltip("Absolute height of the boundary. This value is ignored if boundary type is not set to Absolute or WorldSpace.")]
        float _height = 300f;

        /// <summary>
        /// Delegate that passes a `ReadOnlySpan` of target indicators that were added or updated.
        /// </summary>
        public delegate void TargetIndicatorsUpdatedDelegate(ReadOnlySpan<TargetIndicator> span);

        /// <summary>
        /// Delegate that passes a `ReadOnlySpan` of target indicators that were removed.
        /// </summary>
        public delegate void TargetIndicatorsRemovedDelegate(ReadOnlySpan<TargetIndicatorId> span);

        /// <summary>
        /// Invoked when target indicators have been added.
        /// </summary>
        public event TargetIndicatorsUpdatedDelegate TargetIndicatorsAdded;

        /// <summary>
        /// Invoked when target indicators have been updated.
        /// </summary>
        public event TargetIndicatorsUpdatedDelegate TargetIndicatorsUpdated;

        /// <summary>
        /// Invoked when target indicators have been removed.
        /// </summary>
        public event TargetIndicatorsRemovedDelegate TargetIndicatorsRemoved;

        /// <summary>
        /// The scene camera that is used to calculate screen pose of tracked targets.
        /// </summary>
        public Camera Camera
        {
            get => _camera;
            set => _camera = value;
        }

        /// <summary>
        /// The definition of the currently configured rectangle if <see cref="BoundaryShape"/> is set to
        /// <see cref="BoundaryShape.Rectangle"/>.
        /// </summary>
        public Rect Rectangle
        {
            get
            {
                return _boundaryType switch
                {
                    BoundaryType.Padded => new Rect
                    {
                        min = new Vector2(_leftPadding, _bottomPadding),
                        max = new Vector2(Screen.width - _rightPadding, Screen.height - _topPadding),
                    },
                    BoundaryType.Absolute => new Rect
                    {
                        min = new Vector2(Screen.width * 0.5f - _width * 0.5f,
                            Screen.height * 0.5f - _height * 0.5f),
                        max = new Vector2(Screen.width * 0.5f + _width * 0.5f,
                            Screen.height * 0.5f + _height * 0.5f),
                    },
                    _ => default
                };
            }
        }

        /// <summary>
        /// The definition of the currently configured ellipse if <see cref="BoundaryShape"/> is set to
        /// <see cref="BoundaryShape.Ellipse"/>.
        /// </summary>
        public Ellipse Ellipse
        {
            get
            {
                return _boundaryType switch
                {
                    BoundaryType.Padded => new Ellipse(
                        EllipseScreenPose.GetPaddedEllipseCenter(_leftPadding, _rightPadding, _topPadding, _bottomPadding),
                        (Screen.width - _leftPadding - _rightPadding) * 0.5f,
                        (Screen.height - _topPadding - _bottomPadding) * 0.5f),
                    BoundaryType.Absolute => new Ellipse(
                        EllipseScreenPose.GetAbsoluteEllipseCenter(),
                        _width * 0.5f,
                        _height * 0.5f),
                    _ => default
                };
            }
        }

        /// <summary>
        /// The type of boundary to clamp target indicators to.
        /// </summary>
        public BoundaryType BoundaryType
        {
            get => _boundaryType;
            set
            {
                var isValid = Enum.IsDefined(typeof(BoundaryType), value);
                if (!isValid)
                {
                    Debug.LogError($"Boundary type with integer value {(int)value} is not valid.");
                    return;
                }

                _boundaryType = value;
            }
        }

        /// <summary>
        /// The shape of the boundary to clamp target indicators to.
        /// </summary>
        public BoundaryShape BoundaryShape
        {
            get => _boundaryShape;
            set
            {
                var isValid = Enum.IsDefined(typeof(BoundaryShape), value);
                if (!isValid)
                {
                    Debug.LogError($"Boundary shape with integer value {(int)value} is not valid.");
                    return;
                }

                _boundaryShape = value;
            }
        }

        /// <summary>
        /// The distance in pixels from the left edge of the screen for `Padded` boundary type.
        /// </summary>
        public float LeftPadding
        {
            get => _leftPadding;
            set => _leftPadding = value;
        }

        /// <summary>
        /// The distance in pixels from the right edge of the screen for `Padded` boundary type.
        /// </summary>
        public float RightPadding
        {
            get => _rightPadding;
            set => _rightPadding = value;
        }

        /// <summary>
        /// The distance in pixels from the top edge of the screen for `Padded` boundary type.
        /// </summary>
        public float TopPadding
        {
            get => _topPadding;
            set => _topPadding = value;
        }

        /// <summary>
        /// The distance in pixels from the bottom edge of the screen for `Padded` boundary type.
        /// </summary>
        public float BottomPadding
        {
            get => _bottomPadding;
            set => _bottomPadding = value;
        }

        /// <summary>
        /// The width in pixels centered at the screen center for `Absolute` boundary type.
        /// </summary>
        public float Width
        {
            get => _width;
            set => _width = value;
        }

        /// <summary>
        /// The height in pixels centered at the screen center for `Absolute` boundary type.
        /// </summary>
        public float Height
        {
            get => _height;
            set => _height = value;
        }

        /// <summary>
        /// The max number of targets that can be tracked.
        /// </summary>
        public int MaxTargets => k_maxTargets;

        /// <summary>
        /// The current number of targets being tracked.
        /// </summary>
        public int TrackedTargetsCount => _targetDataById.Count;

        readonly Dictionary<TargetIndicatorId, Transform> _targetDataById = new();
        readonly TargetIndicator[] _addedTargetIndicators = new TargetIndicator[k_maxTargets];
        readonly TargetIndicator[] _updatedTargetIndicators = new TargetIndicator[k_maxTargets];
        readonly TargetIndicatorId[] _removedTargetIndicators = new TargetIndicatorId[k_maxTargets];

        int _addedSinceLastUpdate;
        int _removedSinceLastUpdate;

        ScreenData _screenData;
        RectangleScreenPose _rectangleScreenPose;
        EllipseScreenPose _ellipseScreenPose;
        CompassTapeScreenPose _compassTapeScreenPose;

        /// <summary>
        /// Attempts to add a new target to be tracked. This can only fail if the <see cref="TrackedTargetsCount"/>
        /// equals <see cref="MaxTargets"/>.
        /// </summary>
        /// <param name="target">The transform of the target to track.</param>
        /// <param name="targetIndicator">The created <see cref="TargetIndicator"/> for the current frame of the
        /// target's position.</param>
        /// <returns>`true` if the target was added, otherwise `false`.</returns>
        public bool TryAddTarget(Transform target, out TargetIndicator targetIndicator)
        {
            targetIndicator = TargetIndicator.Default;

            if (target == null)
                return false;

            if (_targetDataById.Count >= k_maxTargets)
                return false;

            var targetIndicatorId = new TargetIndicatorId(Guid.NewGuid());

            _targetDataById.Add(targetIndicatorId, target);

            var screenPose = GetScreenPose(target.position, out var isOutsideBoundary);
            targetIndicator = new TargetIndicator(targetIndicatorId, target, screenPose, isOutsideBoundary);

            _addedTargetIndicators[_addedSinceLastUpdate] = targetIndicator;
            _addedSinceLastUpdate += 1;

            return true;
        }

        /// <summary>
        /// Attempts to get the <see cref="TargetIndicator"/> for the corresponding `TargetIndicatorId`.
        /// </summary>
        /// <param name="targetIndicatorId">The ID of the target indicator.</param>
        /// <param name="targetIndicator">The <see cref="TargetIndicator"/> associated with
        /// <paramref name="targetIndicatorId"/>.</param>
        /// <returns>`true` if the <paramref name="targetIndicatorId"/> is valid and being tracked. Otherwise, false.</returns>
        public bool TryGetTargetIndicator(TargetIndicatorId targetIndicatorId, out TargetIndicator targetIndicator)
        {
            targetIndicator = TargetIndicator.Default;

            if (!_targetDataById.TryGetValue(targetIndicatorId, out var target))
                return false;

            var screenPose = GetScreenPose(target.position, out var isOutsideBoundary);
            targetIndicator = new TargetIndicator(targetIndicatorId, target, screenPose, isOutsideBoundary);

            return true;
        }

        /// <summary>
        /// Attempts to remove a target indicator.
        /// </summary>
        /// <param name="targetIndicatorId">The ID of the target indicator to remove.</param>
        /// <returns>`true` if the <paramref name="targetIndicatorId"/> is valid and being tracked.
        /// Otherwise `false`.</returns>
        public bool TryRemoveTarget(TargetIndicatorId targetIndicatorId)
        {
            var wasRemoved = _targetDataById.Remove(targetIndicatorId);

            if (!wasRemoved)
                return false;

            _removedTargetIndicators[_removedSinceLastUpdate] = targetIndicatorId;
            _removedSinceLastUpdate += 1;
            return true;
        }

        /// <summary>
        /// Removes all targets that are being tracked.
        /// </summary>
        public void RemoveAllTargets()
        {
            var targetsByIdCopy = new Dictionary<TargetIndicatorId, Transform>(_targetDataById);
            foreach (var id in targetsByIdCopy.Keys)
            {
                _targetDataById.Remove(id);
            }
        }

        /// <summary>
        /// Gets the screen pose of a world space position.
        /// </summary>
        /// <param name="worldSpacePosition">The world space position to convert to the screen pose.</param>
        /// <param name="isOutsideBoundary">`true` if the screen pose of <paramref name="worldSpacePosition"/> is
        /// outside of the configured boundary. Otherwise, `false`.</param>
        /// <returns>The screen space pose of <paramref name="worldSpacePosition"/>.</returns>
        public Pose GetScreenPose(in Vector3 worldSpacePosition, out bool isOutsideBoundary)
        {
            return _boundaryType switch
            {
                BoundaryType.Padded => GetPaddedScreenPose(worldSpacePosition, out isOutsideBoundary),
                BoundaryType.Absolute => GetAbsoluteScreenPose(worldSpacePosition, out isOutsideBoundary),
                BoundaryType.CompassTape => _compassTapeScreenPose.GetScreenPoseForCompassTape(worldSpacePosition, out isOutsideBoundary),
                BoundaryType.Unbounded => GetUnboundedScreenPose(worldSpacePosition, out isOutsideBoundary),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <summary>
        /// checks if the screen space position is outside the configured boundary.
        /// </summary>
        /// <param name="screenPoint">The screen point to test. Requires the point as a `Vector3` to
        /// allow for the inclusion of the depth from the camera in world space for faster calculations. If unknown, set
        /// `screenPoint.z` to 0.</param>
        /// <returns>`true` if <paramref name="screenPoint"/> is outside the configured boundary.
        /// Otherwise, `false`.</returns>
        public bool IsOutsideBoundary(in Vector3 screenPoint)
        {
            if (screenPoint.z < 0)
                return true;

            return _boundaryType switch
            {
                BoundaryType.Padded => IsOutsidePaddedBoundary(screenPoint),
                BoundaryType.Absolute => IsOutsideAbsoluteBoundary(screenPoint),
                BoundaryType.CompassTape => false,
                BoundaryType.Unbounded => false,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        void Reset()
        {
            _camera = Camera.main;

            if (_camera == null)
                _camera = FindAnyObjectByType<Camera>();
        }

        void OnValidate()
        {
            if (_leftPadding < 0)
                _leftPadding = 0;

            if (_rightPadding < 0)
                _rightPadding = 0;

            if (_topPadding < 0)
                _topPadding = 0;

            if (_bottomPadding < 0)
                _bottomPadding = 0;

            if (_width < 0)
                _width = 0;

            if (_height < 0)
                _height = 0;
        }

        void Awake()
        {
            if (_camera == null)
                _camera = Camera.main;

            if (_camera == null)
                _camera = FindAnyObjectByType<Camera>();

            if (_camera == null)
                Debug.LogException(new NullReferenceException($"{nameof(_camera)} is null."), this);

            _screenData = new(this);
            _rectangleScreenPose = new(_screenData);
            _ellipseScreenPose = new(_screenData);
            _compassTapeScreenPose = new(_screenData);
        }

        void Update()
        {
            // added
            if (_addedSinceLastUpdate > 0)
            {
                var added = new ReadOnlySpan<TargetIndicator>(_addedTargetIndicators, 0, _addedSinceLastUpdate);
                TargetIndicatorsAdded?.Invoke(added);
                _addedSinceLastUpdate = 0;
            }

            // updated
            var index = 0;
            foreach (var (id, target) in _targetDataById)
            {
                var screenPose = GetScreenPose(target.position, out var isOutsideBoundary);
                var targetIndicator = new TargetIndicator(id, target, screenPose, isOutsideBoundary);
                _updatedTargetIndicators[index] = targetIndicator;
                index += 1;
            }

            var updated = new ReadOnlySpan<TargetIndicator>(_updatedTargetIndicators, 0, index);
            TargetIndicatorsUpdated?.Invoke(updated);

            // removed
            if (_removedSinceLastUpdate > 0)
            {
                var removed = new ReadOnlySpan<TargetIndicatorId>(_removedTargetIndicators, 0, _removedSinceLastUpdate);
                TargetIndicatorsRemoved?.Invoke(removed);
                _removedSinceLastUpdate = 0;
            }
        }

        Pose GetPaddedScreenPose(in Vector3 worldSpacePosition, out bool isOutsideBoundary)
        {
            return _boundaryShape switch
            {
                BoundaryShape.Rectangle => _rectangleScreenPose.GetPaddedScreenPose(worldSpacePosition,
                    out isOutsideBoundary),
                BoundaryShape.Ellipse => _ellipseScreenPose.GetPaddedScreenPose(worldSpacePosition,
                    out isOutsideBoundary),
                _ => throw new ArgumentOutOfRangeException(
                    $"Boundary shape with integer value {(int)_boundaryShape} is not supported.")
            };
        }

        Pose GetAbsoluteScreenPose(in Vector3 worldSpacePosition, out bool isOutsideBoundary)
        {
            return _boundaryShape switch
            {
                BoundaryShape.Rectangle => _rectangleScreenPose.GetAbsoluteScreenPose(worldSpacePosition, out isOutsideBoundary),
                BoundaryShape.Ellipse => _ellipseScreenPose.GetAbsoluteScreenPose(worldSpacePosition, out isOutsideBoundary),
                _ => throw new ArgumentOutOfRangeException(
                    $"Boundary shape with integer value {(int)_boundaryShape} is not supported.")
            };
        }

        Pose GetUnboundedScreenPose(in Vector3 worldSpacePosition, out bool isOutsideBoundary)
        {
            var screenPoint = _camera.WorldToScreenPoint(worldSpacePosition);

            if (screenPoint.z < 0)
                screenPoint.x = float.MaxValue;

            isOutsideBoundary = false;

            return new Pose(screenPoint, Quaternion.identity);
        }

        bool IsOutsidePaddedBoundary(in Vector3 screenPoint)
        {
            return _boundaryShape switch
            {
                BoundaryShape.Rectangle => _rectangleScreenPose.IsOutsidePaddedBoundary(screenPoint),
                BoundaryShape.Ellipse => _ellipseScreenPose.IsOutsidePaddedBoundary(screenPoint),
                _ => throw new ArgumentOutOfRangeException(
                    $"Boundary shape with integer value {(int)_boundaryShape} is not supported.")
            };
        }

        bool IsOutsideAbsoluteBoundary(in Vector3 screenPoint)
        {
            return _boundaryShape switch
            {
                BoundaryShape.Rectangle => _rectangleScreenPose.IsOutsideAbsoluteBoundary(screenPoint),
                BoundaryShape.Ellipse => _ellipseScreenPose.IsOutsideAbsoluteBoundary(screenPoint),
                _ => throw new ArgumentOutOfRangeException(
                    $"Boundary shape with integer value {(int)_boundaryShape} is not supported.")
            };
        }
    }
}
