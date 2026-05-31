using Unity.Cinemachine;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    [Header("대포")]
    public float shootSpeed = 10f;
    public float rotateSpeed = 60f;
    public GameObject cannonHead;
    public CinemachineImpulseSource impluse;
    public float implusePower;

    [Header("라인")]
    public Transform shootLineDir;
    public LineRenderer lineRenderer;
    public float maxDistance = 15;
    //public float basicLineSize = 0.3f;

    public Player player;

    private float currentAngle = 0f; // 각도 캐싱

    private void Awake()
    {
        impluse = GameObject.FindFirstObjectByType<CinemachineImpulseSource>();
        lineRenderer.enabled = true;

        // 초기 각도 저장
        currentAngle = cannonHead.transform.localEulerAngles.z;
        if (currentAngle > 180) currentAngle -= 360;
    }

    void Update()
    {
        RotateCannonHead();
        DrawParabolicTrajectory();
        

        

        if (Input.GetKeyDown(KeyCode.F) && GameManager.Instance.canShoot)
        {
            SoundManager.Instance.PlaySFX(SFXType.cannon_shot);
            Shoot();

        }
    }

    void LateUpdate()
    {
        if (GameManager.Instance.status == GameStatus.idle)
        {
            player.transform.localRotation = Quaternion.Euler(72f, -90f, 0f);
        }
    }

    void RotateCannonHead()
    {
        if (GameManager.Instance.status != GameStatus.idle)
            return;

        float wheel = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(wheel) < 0.001f) return; // 입력 없으면 스킵 ⭐

        // 각도 업데이트
        currentAngle += rotateSpeed * wheel;
        currentAngle = Mathf.Clamp(currentAngle, -50f, 50f);

        if(wheel > 0)
        {
            SoundManager.Instance.PlaySFX(SFXType.anglePlus);
        }
        else if(wheel < 0)
        {
            SoundManager.Instance.PlaySFX(SFXType.angleMinus);
        }

        // Quaternion으로 설정 (더 안정적) ⭐
        cannonHead.transform.localRotation = Quaternion.Euler(0, 0, currentAngle);
    }

    void Shoot()
    {
        GameManager.Instance.canShoot = false;
        impluse.DefaultVelocity = new Vector3(0, -2, 0);
        impluse.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion;
        impluse.GenerateImpulseWithForce(implusePower);
        lineRenderer.enabled = false;
        player.ShootPlayer(shootSpeed);
    }

    void DrawParabolicTrajectory()
    {
        Vector3 startPos = shootLineDir.position;
        Vector3 dir = player.transform.up.normalized;
        float gravity = Mathf.Abs(Physics.gravity.y);
        float timeStep = 0.05f;
        int maxSteps = 30;

        lineRenderer.positionCount = maxSteps;

        for (int i = 0; i < maxSteps; i++)
        {
            float t = i * timeStep;
            Vector3 pos =
                startPos +
                dir * shootSpeed * t +
                Vector3.down * 0.5f * gravity * t * t;

            if (Vector3.Distance(startPos, pos) > maxDistance)
            {
                lineRenderer.positionCount = i;
                break;
            }

            lineRenderer.SetPosition(i, pos);
        }
    }
}