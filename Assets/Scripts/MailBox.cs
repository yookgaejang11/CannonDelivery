using DG.Tweening;
using UnityEngine;
public class MailBox : MonoBehaviour
{
    public int num;
    public Transform Pos;
    public Transform boxPos;
    public Vector3 BoxSize;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        

        Collider[] colliders = Physics.OverlapBox(Pos.position, BoxSize);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Box") && !DeliveryManager.Instance.isDelivery[num])
            {
                Debug.Log("µé¾î¿È");
                DeliveryManager.Instance.isDelivery[num] = true;
                collider.gameObject.GetComponent<Rigidbody>().useGravity = false;
                collider.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                collider.transform.position = boxPos.position;
                collider.transform.parent = boxPos;
                UiManager.Instance.progressUi[1].DOValue(DeliveryManager.Instance.DeliveryNum(), 0.25f);
            }

        }
    }

    void OnDrawGizmos()
    {
        
        Gizmos.DrawWireCube(Pos.position, BoxSize);
    }
}
