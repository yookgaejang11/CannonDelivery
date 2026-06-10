using System;
using System.Collections.Generic;
using TargetIndicators.Samples;
using TargetIndicators;
using UnityEngine;

/// <summary>
/// Manages target indicators created by a <see cref="TargetIndicatorManager"/> by instantiating visual indicators
/// and updating their position in the UI. Supports `Padded`, `Absolute`, and `Unbounded` boundary types.
/// </summary>
public class VisualIndicatorManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField, Tooltip("The default prefab for the visual indicator.")]
    protected VisualIndicator _defaultVisualIndicatorPrefab;

    [Header("Scene References")]
    [SerializeField, Tooltip("The manager this visual indicator manager interacts with and responds to.")]
    protected TargetIndicatorManager _targetIndicatorManager;

    [SerializeField, Tooltip("The Canvas the visual indicator belongs to. This is used for handling changes in canvas scale.")]
    protected Canvas _canvas;

    [SerializeField, Tooltip("The RectTransform that visual indicators get parented to.")]
    protected RectTransform _content;

    [Header("Settings")]
    [SerializeField, Tooltip("How this class should add visual indicators. Auto will instantiate the default visual" +
                             "indicator prefab when the target indicator manager adds targets. Manual will only " +
                             "instantiate prefabs when user code calls the AddTargetIndicator API in this class.")]
    protected AddIndicatorMode _addIndicatorMode;

    /// <summary>
    /// Get and set the default indicator prefab.
    /// </summary>
    public VisualIndicator DefaultVisualIndicatorPrefab
    {
        get => _defaultVisualIndicatorPrefab;
        set => _defaultVisualIndicatorPrefab = value;
    }

    /// <summary>
    /// Get and set the add indicator mode.
    /// </summary>
    public AddIndicatorMode AddIndicatorMode
    {
        get => _addIndicatorMode;
        set => _addIndicatorMode = value;
    }

    /// <summary>
    /// The set of targets that is being managed by the target indicator ID.
    /// </summary>
    protected readonly Dictionary<TargetIndicatorId, VisualIndicator> _trackedUITargetIndicators = new();

    /// <summary>
    /// The state if a warning log has been logged since the last `OnTargetIndicatorsAdded` or `OnTargetIndicatorsUpdated`
    /// was called to prevent spamming every frame.
    /// </summary>
    protected bool _warningLogged;

    /// <summary>
    /// Adds a target to be tracked to the target indicator manager and will instantiate the default visual indicator
    /// prefab. Requires AddIndicatorMode is set to Auto.
    /// </summary>
    /// <param name="target">The target transform to track.</param>
    public virtual void AddTargetIndicator(Transform target)
    {
        AddTargetIndicator(target, _defaultVisualIndicatorPrefab);
    }

    /// <summary>
    /// Adds a target to be tracked to the target indicator manager and will instantiate a custom visual indicator
    /// prefab. Requires AddIndicatorMode is set to Auto.
    /// </summary>
    /// <param name="target">The target transform to track.</param>
    /// <param name="indicatorPrefab">The visual indicator prefab to instantiate for the target.</param>
    public virtual void AddTargetIndicator(Transform target, VisualIndicator indicatorPrefab)
    {
        if (indicatorPrefab == null)
            throw new ArgumentNullException(nameof(indicatorPrefab));

        if (_addIndicatorMode == AddIndicatorMode.Auto)
            return;

        var wasAdded = _targetIndicatorManager.TryAddTarget(target.transform, out var targetIndicator);
        if (!wasAdded)
            return;

        CreateUITargetIndicator(indicatorPrefab, targetIndicator);
    }

    /// <summary>
    /// Shows the visual indicator by its ID.
    /// </summary>
    /// <param name="id">The ID of the visual indicator to show.</param>
    public virtual void ShowVisualIndicator(TargetIndicatorId id)
    {
        if (!_trackedUITargetIndicators.TryGetValue(id, out var uiTargetIndicator))
            return;

        if (uiTargetIndicator != null)
            uiTargetIndicator.Show();
    }

    /// <summary>
    /// Hides the visual indicator by its ID.
    /// </summary>
    /// <param name="id">The ID of the visual indicator to hide.</param>
    public virtual void HideVisualIndicator(TargetIndicatorId id)
    {
        if (!_trackedUITargetIndicators.TryGetValue(id, out var uiTargetIndicator))
            return;

        if (uiTargetIndicator != null)
            uiTargetIndicator.Hide();
    }

    /// <summary>
    /// Removes a target from the target indicator manager by ID.
    /// </summary>
    /// <param name="id">The target indicator ID of the target to remove.</param>
    public virtual void RemoveTargetIndicator(TargetIndicatorId id)
    {
        if (!_trackedUITargetIndicators.TryGetValue(id, out var uiTargetIndicator))
            return;

        if (uiTargetIndicator != null)
            Destroy(uiTargetIndicator.gameObject);

        _trackedUITargetIndicators.Remove(id);
    }

    protected virtual void Reset()
    {
        _targetIndicatorManager = FindAnyObjectByType<TargetIndicatorManager>();
    }

    protected virtual void Awake()
    {
        DefaultVisualIndicatorPrefab = _defaultVisualIndicatorPrefab;

        if (_targetIndicatorManager == null)
            _targetIndicatorManager = FindAnyObjectByType<TargetIndicatorManager>();

        if (_targetIndicatorManager == null)
            Debug.LogException(new NullReferenceException($"{nameof(_targetIndicatorManager)} is null."), this);

        if (_content == null)
            Debug.LogException(new NullReferenceException($"{nameof(_content)} is null."), this);

        if (_canvas == null)
            Debug.LogException(new NullReferenceException($"{nameof(_canvas)} is null."), this);

    }

    protected virtual void OnEnable()
    {
        _targetIndicatorManager.TargetIndicatorsAdded += OnTargetIndicatorsAdded;
        _targetIndicatorManager.TargetIndicatorsUpdated += OnTargetIndicatorsUpdated;
        _targetIndicatorManager.TargetIndicatorsRemoved += OnTargetIndicatorsRemoved;

        foreach (var uiTargetIndicator in _trackedUITargetIndicators.Values)
        {
            uiTargetIndicator.gameObject.SetActive(true);
        }
    }

    protected virtual void OnDisable()
    {
        _targetIndicatorManager.TargetIndicatorsAdded -= OnTargetIndicatorsAdded;
        _targetIndicatorManager.TargetIndicatorsUpdated -= OnTargetIndicatorsUpdated;
        _targetIndicatorManager.TargetIndicatorsRemoved -= OnTargetIndicatorsRemoved;

        foreach (var uiTargetIndicator in _trackedUITargetIndicators.Values)
        {
            if (uiTargetIndicator == null)
                continue;

            uiTargetIndicator.gameObject.SetActive(false);
        }
    }

    protected virtual void OnDestroy()
    {
        var trackedUITargetIndicatorsCopy = new Dictionary<TargetIndicatorId, VisualIndicator>(_trackedUITargetIndicators);
        foreach (var id in trackedUITargetIndicatorsCopy.Keys)
        {
            if (!_trackedUITargetIndicators.TryGetValue(id, out var uiTargetIndicator))
                return;

            if (uiTargetIndicator != null)
                Destroy(uiTargetIndicator.gameObject);

            _trackedUITargetIndicators.Remove(id);
        }
    }

    /// <summary>
    /// This method subscribes to the <see cref="TargetIndicatorManager.TargetIndicatorsAdded"/> event and instantiates
    /// the default visual indicator prefab for the target indicator.
    /// </summary>
    /// <param name="addedTargetIndicators">The `ReadOnlySpan` of target indicators that were added.</param>
    protected virtual void OnTargetIndicatorsAdded(ReadOnlySpan<TargetIndicator> addedTargetIndicators)
    {
        if (_addIndicatorMode == AddIndicatorMode.Manual)
            return;

        if (_targetIndicatorManager.BoundaryType == BoundaryType.CompassTape)
        {
            if (_warningLogged)
                return;

            _warningLogged = true;
            Debug.LogWarning(
                $"{nameof(VisualIndicatorManager)} cannot display {nameof(BoundaryType.CompassTape)} " +
                $"target indicators. Use the {nameof(CompassTapeVisualIndicatorManager)} with the " +
                $"{nameof(CompassTapeVisualIndicator)} or create your own system for displaying target indicator " +
                $"pose updates when {nameof(_targetIndicatorManager.BoundaryShape)} is set to " +
                $"{nameof(BoundaryType.CompassTape)}.)", this);

            return;
        }

        _warningLogged = false;

        foreach (var targetIndicator in addedTargetIndicators)
        {
            CreateUITargetIndicator(_defaultVisualIndicatorPrefab, targetIndicator);
        }
    }

    /// <summary>
    /// This method subscribes to the <see cref="TargetIndicatorManager.TargetIndicatorsUpdated"/> event and updates
    /// the visual indicator associated with each target indicator.
    /// </summary>
    protected virtual void OnTargetIndicatorsUpdated(ReadOnlySpan<TargetIndicator> updatedTargetIndicators)
    {
        if (_targetIndicatorManager.BoundaryType == BoundaryType.CompassTape)
        {
            if (_warningLogged)
                return;

            _warningLogged = true;
            Debug.LogWarning(
                $"{nameof(VisualIndicatorManager)} cannot display {nameof(BoundaryType.CompassTape)} " +
                $"target indicators. Use the {nameof(CompassTapeVisualIndicatorManager)} with the " +
                $"{nameof(CompassTapeVisualIndicator)} or create your own system for displaying target indicator " +
                $"pose updates when {nameof(_targetIndicatorManager.BoundaryShape)} is set to " +
                $"{nameof(BoundaryType.CompassTape)}.)", this);

            return;
        }

        _warningLogged = false;

        foreach (var targetIndicator in updatedTargetIndicators)
        {
            if (!_trackedUITargetIndicators.TryGetValue(targetIndicator.Id, out var uiTargetIndicator))
                return;

            uiTargetIndicator.CanvasScale = _canvas.transform.localScale.x;
            uiTargetIndicator.UpdateVisualIndicator(targetIndicator);
        }
    }

    /// <summary>
    /// This method subscribes to the <see cref="TargetIndicatorManager.TargetIndicatorsRemoved"/> event and destroys
    /// the associated visual indicator for each target indicator ID and removes it from the target indicator manager.
    /// </summary>
    /// <param name="removedTargetIndicators"></param>
    protected virtual void OnTargetIndicatorsRemoved(ReadOnlySpan<TargetIndicatorId> removedTargetIndicators)
    {
        foreach (var id in removedTargetIndicators)
        {
            if (!_trackedUITargetIndicators.TryGetValue(id, out var uiTargetIndicator))
                return;

            if (uiTargetIndicator != null)
                Destroy(uiTargetIndicator.gameObject);

            _trackedUITargetIndicators.Remove(id);
        }
    }

    /// <summary>
    /// Instantiates a visual indicator and sets it's pose.
    /// </summary>
    /// <param name="indicatorPrefab">The prefab to instantiate.</param>
    /// <param name="targetIndicator">The target indicator data to apply to the visual indicator.</param>
    protected virtual void CreateUITargetIndicator(VisualIndicator indicatorPrefab, TargetIndicator targetIndicator)
    {
        if (indicatorPrefab == null)
            throw new ArgumentNullException(nameof(indicatorPrefab));

        var uiIndicator = Instantiate(indicatorPrefab, _content);
        uiIndicator.TargetIndicatorId = targetIndicator.Id;
        uiIndicator.CanvasScale = _canvas.transform.localScale.x;
        uiIndicator.UpdateVisualIndicator(targetIndicator);

        _trackedUITargetIndicators.Add(targetIndicator.Id, uiIndicator);
    }
}
