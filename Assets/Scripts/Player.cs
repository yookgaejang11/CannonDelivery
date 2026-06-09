using DG.Tweening;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

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

    CinemachinePositionComposer composer;

    public Vector2 screenPos;

    public Transform trackingTarget;
    private void Start()
    {
        FirstSetting();
    }

    void FirstSetting()
    {
        Time.timeScale = 1;
        curBullet = maxBullet;
        rigid = GetComponent<Rigidbody>();
        rigid.isKinematic = true;
        rigid.useGravity = false;
        lineRenderer = GetComponent<LineRenderer>();
        mainCam = Camera.main;
        rbs = GetComponentsInChildren<Rigidbody>();
        cols = GetComponentsInChildren<Collider>();
        anim = GetComponent<Animator>();
        cam = GameObject.FindFirstObjectByType<CinemachineCamera>();
        composer = GameObject.FindFirstObjectByType<CinemachinePositionComposer>();
        impluse = cam.gameObject.GetComponent<CinemachineImpulseSource>();
        cam.transform.rotation = Quaternion.Euler(18, 0, 0);
        cam.Target.TrackingTarget = trackingTarget;
        cam.Lens.FieldOfView = 70;
        GameManager.Instance.status = GameStatus.idle;

        composer.Composition.ScreenPosition = screenPos;
        composer.Lookahead.Enabled = true;
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
        if (isCheckingLanding)
            return;
        if (GameManager.Instance.isFail)
            return;
        
        if(GameManager.Instance.status != GameStatus.fail && GameManager.Instance.status != GameStatus.goal && collision.gameObject.CompareTag("FailObj"))
        {
            StartCoroutine(Fail());
        }

    }


    public void KnockBack()
    {
        int ran = UnityEngine.Random.Range(1,100);
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
            impluse.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion;
            impluse.DefaultVelocity = new Vector3(0, -2, 0);
            impluse.GenerateImpulseWithForce(KnockbackPow);
        }
        foreach (Rigidbody rb in rbs)
        {
            if (rb.gameObject.name == "Player")
                continue;
        }

        EnableRagdoll();
        hipsRb.AddExplosionForce(KnockbackPow, transform.position, 3f);
    }

    IEnumerator Fail()
    {
        composer.Lookahead.Enabled = false;
        GameManager.Instance.status = GameStatus.fail;
        StageManager.Instance.failCount++;
        StageManager.Instance.checkTime  = false;
        KnockBack();
        Time.timeScale = 0.3f;
        

        cam.Lens.FieldOfView = 40;
        composer.Composition.ScreenPosition = Vector2.zero;

        rigid.linearVelocity *= 0.2f;

        yield return new WaitForSeconds(0.6f);
        
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
        rotY = transform.eulerAngles.y;
        rotZ = transform.eulerAngles.z;

        UiManager.Instance.progressUi[0].DOValue(UiManager.Instance.progressUi[0].maxValue,0.25f);
    }


}
