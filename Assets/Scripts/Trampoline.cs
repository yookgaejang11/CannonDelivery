using UnityEngine;

public class Trampoline : MonoBehaviour
{
    public float jumpPower = 8;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Rigidbody>().AddForce(transform.up * jumpPower, ForceMode.Impulse);
        }
    }
}
