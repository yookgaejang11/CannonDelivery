using UnityEngine;

public class StageManager : MonoBehaviour
{
    private static StageManager instance;

    public float playTime = 0f;
    public int failCount = 0;
    public int playerStar;
    public float star3Time;
    public float star2Time;

    public bool checkTime;


    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void Update()
    {
        if ((Input.anyKeyDown || Input.mouseScrollDelta.y != 0) && (GameManager.Instance.status != GameStatus.goal &&GameManager.Instance.status != GameStatus.fail))
        {
            checkTime = true;
        }


        if (GameManager.Instance.status != GameStatus.goal && !UiManager.Instance.isPause && checkTime)
        {
            playTime += Time.unscaledDeltaTime;
        }
    }

    public int CalculateStars()
    {
        float time = playTime;

        if (time <= star3Time)
            return 3;

        if (time <= star2Time)
            return 2;

        return 1;
    }

    public static StageManager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }
}
