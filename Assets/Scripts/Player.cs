using DG.Tweening;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float rotZ;
    public float rotY;
    public GameObject weapon;
    public bool aiming = false;
    public float aimingTime;
    public float slowValue;
    public float maxAimingTime = 0.3f;
    LineRenderer lineRenderer;
    public float coolTime;
    public float maxCoolTime = 0.15f;
    public bool canShoot = true;
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public LineRenderer aimLine;
    public float aimLineLength = 10f;
    public GameObject shootStartPos;
    private Camera mainCam;
    
    public int curBullet;
    public int maxBullet = 1;
    
   
    Rigidbody rigid;
    Rigidbody[] rbs;

    public Rigidbody hipsRb;
    Collider[] cols;
    Animator anim;
    public CinemachineImpulseSource impluse;
    public CinemachineCamera cam;

    public float implusePower;
    public float KnockbackPow = 5f;

    public float extraFallForce = 20f;   // 스페이스 누를 때 추가 낙하 힘
    public float rotationSpeed = 10f;    // 회전 보간 속도
    public float maxFallSpeed = -25f;   // 최대 낙하 속도 제한

    private void Start()
    {
        FirstSetting();
    }

    void FirstSetting()
    {
        rigid = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
        mainCam = Camera.main;
        rbs = GetComponentsInChildren<Rigidbody>();
        cols = GetComponentsInChildren<Collider>();
        anim = GetComponent<Animator>();
        cam = GameObject.FindFirstObjectByType<CinemachineCamera>();
        impluse = cam.gameObject.GetComponent<CinemachineImpulseSource>();
        
        cam.Target.TrackingTarget = this.gameObject.transform;

        Debug.Log(GameManager.Instance.status);

        GameManager.Instance.status = GameStatus.idle;


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
        

        Bazooka();

    }

    /// <summary>
    /// 바주카 코드
    /// </summary>
    void Bazooka()
    {
        aimingTime += Time.deltaTime;
        coolTime += Time.deltaTime;
        if (aimingTime > maxAimingTime)
        {
            Time.timeScale = 1;
        }

        if (coolTime > maxCoolTime)
        {
            canShoot = true;
        }

        if(GameManager.Instance.status == GameStatus.aiming || GameManager.Instance.status == GameStatus.shooting)
        {
            if (Input.GetMouseButtonDown(1) && canShoot)
            {
                StartAiming();
            }

            if (Input.GetMouseButton(1) && canShoot)
            {
                GameManager.Instance.status = GameStatus.aiming;
                UpdateAimLine();
            }

            if (Input.GetMouseButtonUp(1) && canShoot)
            {
                GameManager.Instance.status = GameStatus.shooting;
                Shoot();
                StopAiming();
            }
        }
    }

    void StartAiming()
    {
        //cam.Lens.FieldOfView = 50;
        Time.timeScale = slowValue;
        aimLine.enabled = true;
        aimingTime = 0f;
       
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        // 1순위 : 실제 충돌 지점
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return hit.point;
        }

        // 2순위 : 아무것도 안 맞으면 shootStartPos 기준 평면
        Plane plane = new Plane(Vector3.forward, shootStartPos.transform.position);

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return shootStartPos.transform.position + transform.forward * 10f;
    }

    void UpdateAimLine()
    {
        Vector3 targetPoint = GetMouseWorldPosition();
        Vector3 directionToMouse = (targetPoint - shootStartPos.transform.position).normalized;

        float angleX = -Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg + 90f;

        Quaternion targetRotation = Quaternion.Euler(angleX, rotY, rotZ);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 600 * Time.unscaledDeltaTime);

        aimLine.SetPosition(0, shootStartPos.transform.position);
        aimLine.SetPosition(1, targetPoint);
    }

    void Shoot()
    {
        canShoot = false;


        //cam.Lens.FieldOfView = Mathf.Lerp(50, 70f, Time.deltaTime * 100f);

        Vector3 targetPoint = GetMouseWorldPosition();

        Vector3 direction = (targetPoint - shootStartPos.transform.position).normalized;

        GameObject projectile = Instantiate(
            projectilePrefab,
            shootStartPos.transform.position,
            Quaternion.identity
        );

        Physics.IgnoreCollision(
            projectile.GetComponent<Collider>(),
            GetComponent<Collider>()
        );

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(direction * projectileSpeed, ForceMode.Impulse);
        }
    }

    void StopAiming()
    {
        Time.timeScale = 1f;
        aimLine.enabled = false;
        coolTime = 0f;
    }



    void FixedUpdate()
    {
        // 스페이스 누르면 추가 낙하 가속
        if (Input.GetKey(KeyCode.Space) &&( GameManager.Instance.status == GameStatus.shooting || GameManager.Instance.status == GameStatus.aiming))
        {
            if (rigid.linearVelocity.y < 0)
            {
                float targetX = 90f + rigid.linearVelocity.y;

                Quaternion targetRot = Quaternion.Euler(targetX, transform.eulerAngles.y, transform.eulerAngles.z);

                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
            if (rigid.linearVelocity.y > maxFallSpeed)
            {
                rigid.AddForce(Vector3.down * extraFallForce, ForceMode.Acceleration);
            }
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
        if (!GameManager.Instance.IsDeliveryClear() &&!collision.gameObject.CompareTag("Goal") && !collision.gameObject.CompareTag("Box") && GameManager.Instance.status != GameStatus.goal && GameManager.Instance.status != GameStatus.fail)
        {

            KnockBack();
        }

    }

    public void KnockBack()
    {
        this.gameObject.GetComponent<CapsuleCollider>().enabled = false;
        if (!GameManager.Instance.isFail)
        {
            GameManager.Instance.isFail = true;
            impluse.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
            impluse.DefaultVelocity = new Vector3(-2, 0, 0);
            impluse.GenerateImpulseWithForce(KnockbackPow * rigid.linearVelocity.magnitude);
        }
        foreach (Rigidbody rb in rbs)
        {
            if (rb.gameObject.name == "Player")
                continue;
        }

        EnableRagdoll();
        hipsRb.AddExplosionForce(KnockbackPow, transform.position, 3f);
        StartCoroutine(Fail());
    }

    IEnumerator Fail()
    {
        GameManager.Instance.status = GameStatus.fail;
        GameManager.Instance.failCount++;
        cam.Target.TrackingTarget = null;
        yield return new WaitForSeconds(1.5f);
        cam.Lens.FieldOfView = 70;
        GameManager.Instance.ReStart();

    }




    public void ShootPlayer(float speed)
    {

        GameManager.Instance.status = GameStatus.shooting;
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
        Debug.Log(this.transform.rotation.z);
        rotY = transform.eulerAngles.y;
        rotZ = transform.eulerAngles.z;

        UiManager.Instance.progressUi[0].DOValue(UiManager.Instance.progressUi[0].maxValue,0.25f);
    }


}
