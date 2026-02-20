using System;
using System.Collections;
using System.Runtime.InteropServices;
using Unity.Cinemachine;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region 총 발사 코드(만약을 위한 보류)
    /*public GameObject weapon;
    public bool aiming = false;
    float slowTime = 0;
    public float maxSlowTime = 1.0f;
    [Header("무기")]
    public float aimingTime;
    public float maxAimingTime = 0.3f;
    ineRenderer lineRenderer;
    public float coolTime;
    public float maxCoolTime = 0.15f;
    public bool canShoot = true;
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public LineRenderer aimLine;
    public float aimLineLength = 10f;
    public GameObject shootStartPos;
    private Camera mainCam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        //lineRenderer = GetComponent<LineRenderer>();
        //mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        aimingTime += Time.deltaTime;
        coolTime += Time.deltaTime;
        if(aimingTime > maxAimingTime)
        {
            Time.timeScale = 1;
        }

        if(coolTime > maxCoolTime)
        {
            canShoot = true;
        }

        if (Input.GetMouseButtonDown(1) && canShoot)
        {
            StartAiming();
        }

        if (Input.GetMouseButton(1) && canShoot)
        {
            UpdateAimLine();
        }

        if (Input.GetMouseButtonUp(1) && canShoot)
        {
            Shoot();
            StopAiming();
        }
    }
    void StartAiming()
    {
        Time.timeScale = 0.3f;
        aimLine.enabled = true;
        aimingTime = 0f;

    }

    void UpdateAimLine()
    {
        float camDistance = Vector3.Distance(mainCam.transform.position, transform.position);

        Vector3 mousePos = mainCam.ScreenToWorldPoint(new Vector3(
            Input.mousePosition.x,
            Input.mousePosition.y,
            camDistance
        ));
        mousePos.z = shootStartPos.transform.position.z; // shootStartPos와 같은 z

        // 길이 제한 없이 마우스까지
        aimLine.SetPosition(0, shootStartPos.transform.position);
        aimLine.SetPosition(1, mousePos);

        Debug.DrawLine(shootStartPos.transform.position, mousePos, Color.red);
    }

    void Shoot()
    {
        canShoot = false;
        Vector3 mousePos = mainCam.ScreenToWorldPoint(new Vector3(
            Input.mousePosition.x,
            Input.mousePosition.y,
            Mathf.Abs(mainCam.transform.position.z)
        ));
        mousePos.z = transform.position.z;

        Vector3 direction = (mousePos - transform.position).normalized;

        GameObject projectile = Instantiate(projectilePrefab, shootStartPos.transform.position, Quaternion.identity);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(direction * projectileSpeed,ForceMode.Impulse);
        }


    }

    void StopAiming()
    {
        Time.timeScale = 1f;
        aimLine.enabled = false;
        coolTime = 0f;
    }
    */
    #endregion
    Rigidbody rigid;
    Rigidbody[] rbs;

    public Rigidbody hipsRb;
    Collider[] cols;
    Animator anim;
    public CinemachineImpulseSource impluse;
    public CinemachineCamera cam;

    public float implusePower;
    public float KnockbackPow = 5f;

    private void Awake()
    {
        FirstSetting();
    }

    void FirstSetting()
    {
        Debug.Log("첫 설정");
        rigid = GetComponent<Rigidbody>();
        rbs = GetComponentsInChildren<Rigidbody>();
        cols = GetComponentsInChildren<Collider>();
        anim = GetComponent<Animator>();
        cam = GameObject.FindFirstObjectByType<CinemachineCamera>();
        impluse = cam.gameObject.GetComponent<CinemachineImpulseSource>();

        cam.Target.TrackingTarget = this.gameObject.transform;

       

        foreach (Rigidbody rb in rbs)
        {
            if (rb.gameObject.name == "Player")
                continue;
            rb.isKinematic = true;
        }


        foreach (Collider col in cols)
        {
            if (col.gameObject.name == "Player")
                continue;
            col.enabled = false;
        }


        anim.enabled = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EnableRagdoll();
            Rigidbody mainRb = GetComponent<Rigidbody>();
            mainRb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        }
    }

    void EnableRagdoll()
    {
        anim.enabled = false;

        foreach (Rigidbody rb in rbs)
        {
            if (rb == rigid) continue;   // 루트 제외
            rb.isKinematic = false;
        }

        foreach (Collider col in cols)
        {
            /*if (col.gameObject == this.gameObject)
            {
                Debug.Log("asdf");
                continue;
            }*/
            col.enabled = true;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("test");  
        //rigid.isKinematic = true;
        rigid.linearVelocity = Vector3.zero;
        this.gameObject.GetComponent<CapsuleCollider>().enabled = false;
        if (!GameManager.Instance.isFail)
        {
            GameManager.Instance.isFail = true;
            impluse.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
            impluse.DefaultVelocity = new Vector3(-2, 0, 0);
            impluse.GenerateImpulseWithForce(KnockbackPow);
        }
        foreach (Rigidbody rb in rbs)
        {
            if (rb.gameObject.name == "Player")
                continue;
            //rb.GetComponent<Rigidbody>().isKinematic = true;
            //rb.GetComponent<Rigidbody>().detectCollisions = false;
        }

        EnableRagdoll();
        hipsRb.AddExplosionForce(KnockbackPow, transform.position, 3f);
        StartCoroutine(Fail());
        
       
    }

    IEnumerator Fail()
    {
        GameManager.Instance.failCount++;
        cam.Target.TrackingTarget = null;
        yield return new WaitForSeconds(1.5f);
        GameManager.Instance.ReStart();

    }


    

    public void ShootPlayer(float speed)
    {

        transform.parent = null;
        
        rigid.isKinematic = false;
        rigid.useGravity = true;

        rigid.linearVelocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;

        foreach (Rigidbody rb in rbs)
        {
            if (rb.gameObject.CompareTag("ragdoll"))
                rb.isKinematic = false;

        }


        foreach (Collider col in cols)
        {
            if (col.gameObject.CompareTag("ragdoll"))
                col.enabled = true;

        }

        rigid.AddForce(transform.up * speed, ForceMode.Impulse);
        this.gameObject.GetComponent<CapsuleCollider>().isTrigger = false;
    }

    
}
