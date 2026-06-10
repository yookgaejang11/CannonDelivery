using UnityEngine;

namespace TargetIndicators.Samples
{
    public class CompassTapeTargetIndicatorsSetupExample : MonoBehaviour
    {
        [SerializeField, Tooltip("The visual indicator manager used for adding compass tape visual indicators to.")]
        CompassTapeVisualIndicatorManager _playersVisualIndicatorManager;

        [Header("Player Targets")]
        [SerializeField]
        Transform _player1Target;

        [SerializeField]
        CompassTapeVisualIndicator _player1CompassTapeIndicator;

        [Space]
        [SerializeField]
        Transform _player2Target;

        [SerializeField]
        CompassTapeVisualIndicator _player2CompassTapeIndicator;

        [Space]
        [SerializeField]
        Transform _player3Target;

        [SerializeField]
        CompassTapeVisualIndicator _player3CompassTapeIndicator;

        [Space]
        [SerializeField]
        Transform _player4Target;

        [SerializeField]
        CompassTapeVisualIndicator _player4CompassTapeIndicator;

        void Start()
        {
            // First set the `AddIndicatorMode` to manual so we can use custom prefabs for each target's visual indicator.
            _playersVisualIndicatorManager.AddIndicatorMode = AddIndicatorMode.Manual;
            _playersVisualIndicatorManager.AddTargetIndicator(_player1Target, _player1CompassTapeIndicator);
            _playersVisualIndicatorManager.AddTargetIndicator(_player2Target, _player2CompassTapeIndicator);
            _playersVisualIndicatorManager.AddTargetIndicator(_player3Target, _player3CompassTapeIndicator);
            _playersVisualIndicatorManager.AddTargetIndicator(_player4Target, _player4CompassTapeIndicator);
        }
    }
}
