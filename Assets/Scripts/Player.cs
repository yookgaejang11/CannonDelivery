using System;
using UnityEngine;

public class Player : MonoBehaviour
{

    Rigidbody rigid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Shoot(float speed)
    {
        transform.parent = null;

        rigid.isKinematic = false;
        rigid.useGravity = true;

        rigid.linearVelocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;

        rigid.AddForce(transform.up * speed, ForceMode.Impulse);
    }
}
