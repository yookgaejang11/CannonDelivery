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
    public float basicLineSize = 0.3f;

    public Player player;

    private void Awake()
    {
        impluse = GameObject.FindFirstObjectByType<CinemachineImpulseSource>();
        lineRenderer.enabled = true;
    }

    private void Start()
    {
        
    }

    void Update()
    {
        RotateCannonHead();
        DrawParabolicTrajectory();

        if (Input.GetKeyDown(KeyCode.Mouse0) && GameManager.Instance.canShoot)
        {
            GameManager.Instance.canShoot = false;
            impluse.DefaultVelocity = new Vector3(0,-2,0);  
            impluse.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion;
            impluse.GenerateImpulseWithForce(implusePower);
            lineRenderer.enabled = false;
            player.ShootPlayer(shootSpeed);
        }
    }

    void DrawParabolicTrajectory()
    {
        Vector3 startPos = shootLineDir.position;
        Vector3 dir = player.transform.up.normalized;

        float gravity = Mathf.Abs(Physics.gravity.y);
        float timeStep = 0.05f;
        int maxSteps = 30;

        lineRenderer.positionCount = maxSteps;

        Vector3 prevPos = startPos;

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
            prevPos = pos;
        }
    }

    void RotateCannonHead()
    {
        Vector3 euler = cannonHead.transform.localEulerAngles;

        // 0~360 → -180~180 변환
        if (euler.z > 180) euler.z -= 360;

        euler.z = Mathf.Clamp(euler.z, -50f, 50f);
        cannonHead.transform.localEulerAngles = euler;
        float wheel = Input.GetAxis("Mouse ScrollWheel");
        cannonHead.transform.Rotate(Vector3.forward * rotateSpeed * wheel);
       
    }
}
