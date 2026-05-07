using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class ClearPoint : MonoBehaviour
{
    float time;


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

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player") && GameManager.Instance.IsDeliveryClear())
        {
            other.GetComponent<Player>().isCheckingLanding = true;
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            time  += Time.deltaTime;
            if (time > 0.5f && GameManager.Instance.IsDeliveryClear())
            {
                ClearStage();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<Player>().isCheckingLanding = false;
        }
    }

}