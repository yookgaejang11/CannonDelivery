using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

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
            if (collider.CompareTag("Box") && !GameManager.Instance.isDelivery[num])
            {
                Debug.Log("µé¾î¿È");
                GameManager.Instance.isDelivery[num] = true;
                
                collider.gameObject.GetComponent<Rigidbody>().useGravity = false;
                collider.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                collider.transform.position = boxPos.position;
                collider.transform.parent = boxPos;
            }

        }
    }

    void OnDrawGizmos()
    {
        
        Gizmos.DrawWireCube(Pos.position, BoxSize);
    }
}
