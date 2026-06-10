using UnityEngine;
using DG.Tweening;

public class ClearPoint : MonoBehaviour
{
    float time;


    public void ClearStage()
    {
        if (DeliveryManager.Instance.IsDeliveryClear() &&
            GameManager.Instance.status != GameStatus.goal)
        {
            GameManager.Instance.status = GameStatus.goal;

            UiManager.Instance.progressUi[2].DOValue(1, 0.25f);

            GameManager.Instance.Clear();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player") && DeliveryManager.Instance.IsDeliveryClear())
        {
            other.GetComponent<Player>().isCheckingLanding = true;
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            time  += Time.deltaTime;
            if (time > 0.5f && DeliveryManager.Instance.IsDeliveryClear())
            {
                other.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                other.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                ClearStage();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<Player>().isCheckingLanding = false;
            time = 0;
        }
    }

}