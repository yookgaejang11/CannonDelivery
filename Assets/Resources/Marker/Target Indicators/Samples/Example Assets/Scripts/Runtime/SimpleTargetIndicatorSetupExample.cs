using System.Collections.Generic;
using UnityEngine;

namespace TargetIndicators.Samples
{
    public class SimpleTargetIndicatorSetupExample : MonoBehaviour
    {
        [SerializeField, Tooltip("The target indicator manager to add targets to track.")]
        TargetIndicatorManager _targetIndicatorManager;

        [SerializeField, Tooltip("The targets to track and create visual indicators for.")]
        List<Transform> _targets = new();

        void Start()
        {
            // Add targets directly to the `TargetIndicatorManager` and let the `VisualIndicatorManager`
            // automatically create visual indicators with its default prefab.
            foreach (var target in _targets)
            {
                _targetIndicatorManager.TryAddTarget(target, out var targetIndicator);
            }
        }
    }
}
