using UnityEngine;

public class Cannon : MonoBehaviour
{
    [Header("대포")]
    public float shootSpeed = 10f;
    public float rotateSpeed = 60f;
    public GameObject cannonHead;
    
    [Header("라인")]
    public Transform shootLineDir;
    public LineRenderer lineRenderer;
    public float maxDistance = 15;
    public float basicLineSize = 0.3f;

    public Player player;
    private void Start()
    {
        lineRenderer.startWidth = basicLineSize;
        lineRenderer.endWidth = basicLineSize;
    }

    void Update()
    {
        RotateCannonHead();
        DrawParabolicTrajectory();

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            lineRenderer.startWidth = 0;
            lineRenderer.endWidth = 0;// 발사하면 라인 제거
            player.Shoot(shootSpeed);
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

        if (Input.GetKey(KeyCode.E))
            cannonHead.transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.Q))
            cannonHead.transform.Rotate(Vector3.back * rotateSpeed * Time.deltaTime);
    }
}
