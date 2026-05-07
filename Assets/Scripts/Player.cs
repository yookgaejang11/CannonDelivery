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
    [Header("플레이어")]
    public float implusePower;
    public float KnockbackPow = 5f;

    public bool isCheckingLanding;

    public float extraFallForce = 20f;   // 스페이스 누를 때 추가 낙하 힘
    public float rotationSpeed = 10f;    // 회전 보간 속도
    public float maxFallSpeed = -30f;   // 최대 낙하 속도 제한

    public float rotZ;
    public float rotY;

    [Header("무기")]
    public GameObject weapon;
    public ParticleSystem shootParticle;
    public bool aiming = false;
    public GameObject shootStartPos;
    public float aimingTime;
    public LineRenderer aimLine;
    public float aimLineLength = 10f;
    public float slowValue;
    public float maxAimingTime = 0.3f;
    LineRenderer lineRenderer;
    public float coolTime;
    public float maxCoolTime = 0.15f;
    public bool canShoot = true;

    [Header("택배")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public int curBullet;
    public int maxBullet = 1;

    Rigidbody rigid;
    Rigidbody[] rbs;
    public Rigidbody hipsRb;
    Collider[] cols;
    Animator anim;
    public CinemachineImpulseSource impluse;
    public CinemachineCamera cam;
    private Camera mainCam;


    private void Start()
    {
        FirstSetting();
    }

    void FirstSetting()
    {
        curBullet = maxBullet;
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
            col.isTrigger = true;
        }


        anim.enabled = true;
    }

    private void Update()
    {
        Bazooka();
    }

    void Bazooka()
    {
        aimingTime += Time.deltaTime;
        coolTime += Time.deltaTime;
        if (aimingTime > maxAimingTime && GameManager.Instance.status == GameStatus.aiming && !UiManager.Instance.isPause)
        {
            Time.timeScale = 1;
        }

        if (coolTime > maxCoolTime)
        {
            canShoot = true;
        }

        if(GameManager.Instance.status == GameStatus.aiming || GameManager.Instance.status == GameStatus.shooting && curBullet > 0)
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
                curBullet -= 1;
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

        
        shootParticle.Play();

        SoundManager.Instance.PlaySFX(SFXType.shootBox);

        //cam.Lens.FieldOfView = Mathf.Lerp(50, 70f, Time.deltaTime * 100f);

        Vector3 targetPoint = GetMouseWorldPosition();

        Vector3 direction = (targetPoint - shootStartPos.transform.position).normalized;

        GameObject projectile = Instantiate(
            projectilePrefab,
            shootStartPos.transform.position,
            Quaternion.identity
        );

        //오브젝트 충돌 무시
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
            //velocityY 값이 - 일때 몸 기울이기
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
            else
            {
                rigid.AddForce(Vector3.down * 10, ForceMode.Acceleration);
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
            col.isTrigger = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {

        Debug.Log("asdf");
        if (isCheckingLanding)
            return;
        if (GameManager.Instance.isFail)
            return;
        
        KnockBack();
    }


    public void KnockBack()
    {
        int ran = UnityEngine.Random.Range(1,100);
        //Debug.Log(ran);
        if (ran <= 90)
        {
            SoundManager.Instance.PlaySFX(SFXType.bonk);
        }
        else
        {
            
            SoundManager.Instance.PlaySFX(SFXType.fail);
        }
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
            col.isTrigger = true;
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
