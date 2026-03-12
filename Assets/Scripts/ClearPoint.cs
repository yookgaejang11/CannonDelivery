using UnityEngine;
using DG.Tweening;

public class ClearPoint : MonoBehaviour
{
    public Transform Pos;
    public Vector3 BoxSize;

    public bool IsPlayerInside()
    {
        Collider[] colliders = Physics.OverlapBox(Pos.position, BoxSize);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    public void ClearStage()
    {
        if (GameManager.Instance.IsDeliveryClear() &&
            GameManager.Instance.status != GameStatus.goal)
        {
            GameManager.Instance.status = GameStatus.goal;

            UiManager.Instance.progressUi[2].DOValue(1, 0.25f);

            GameManager.Instance.Clear();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(Pos.position, BoxSize * 2);
    }
}