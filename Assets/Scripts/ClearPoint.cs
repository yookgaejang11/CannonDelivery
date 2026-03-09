using UnityEngine;

public class ClearPoint : MonoBehaviour
{
    public Transform Pos;
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
            if (collider.CompareTag("Player"))
            {
                if (GameManager.Instance.IsDeliveryClear())
                {
                    GameManager.Instance.Clear();
                }
                else
                {
                    GameManager.Instance.Fail();
                }
            }

        }
    }

    void OnDrawGizmos()
    {

        Gizmos.DrawWireCube(Pos.position, BoxSize);
    }
}
