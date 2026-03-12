using NUnit.Framework;
using System;
using System.Collections.Generic;
using TargetIndicators;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public enum GameStatus
{
    idle,
    shooting,
    aiming,
    goal,
    fail
}


public class GameManager : MonoBehaviour
{

    private static GameManager instance;

    public ClearPoint clearPoint;

    public float playTime = 0f;

    public Transform cannonTransform;

    public int failCount = 0;

    public GameObject playerObj;
    public Player player;

    public GameObject CannonPrefab;

    public GameObject cannonObj;

    public GameStatus status;

    public bool isFail = false;
    public bool canShoot = true;

    public List<Transform> targets = new List<Transform>();
    public Transform goal;

    TargetIndicatorManager targetIndicatorManager;
    VisualIndicatorManager visualIndicatorManager;

    public int playerStar;

    public float star3Time;
    public float star2Time;

    public List<bool> isDelivery = new List<bool>();
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        player = GameObject.FindFirstObjectByType<Player>();
        playerObj = player.gameObject;
        cannonObj = GameObject.FindFirstObjectByType<Cannon>().gameObject;
        cannonTransform = cannonObj.transform;
        targetIndicatorManager = GetComponent<TargetIndicatorManager>();
        visualIndicatorManager = GetComponent<VisualIndicatorManager>();
        
       
    }
    private void Start()
    {
        if (targets.Count > 0)
        {
            foreach (Transform t in targets)
            {
                if (targetIndicatorManager != null)
                {
                    targetIndicatorManager.TryAddTarget(t, out var targetIndicator);
                }
                else
                {
                    Debug.Log("sdf");
                }
            }
        }

        //goal = GameObject.FindGameObjectWithTag("Goal").transform;

        Debug.Log(targets.Count);
        if (goal != null)
        {
            targetIndicatorManager.TryAddTarget(goal, out var targetIndicator);
        }
        else
        {
            Debug.LogError("does not exsisted GoalPoint");
        }

    }

    private void Update()
    {
        if(status != GameStatus.goal )
        {
            playTime += Time.unscaledDeltaTime;
        }
    }

    public int CalculateStars()
    {
        float time = GameManager.Instance.playTime;

        if (time <= star3Time)
            return 3;

        if (time <= star2Time)
            return 2;

        return 1;
    }

    public bool IsDeliveryClear()
    {
        for(int i = 0; i < isDelivery.Count; i++)
        {
            if (!isDelivery[i])
            {
                return false;
            }
        }
        return true;
    }

    public int DeliveryNum()
    {
        int num = 0;
        for(int i = 0; i < isDelivery.Count;i++)
        {
            if (isDelivery[i])
            {
                num++;
            }
        }

        return num;
    }
   

    public void ReStart()
    {
        Destroy(player.gameObject);
        Destroy(cannonObj);
        GameObject obj = Instantiate(CannonPrefab, cannonTransform.position,cannonTransform.rotation);
        GameObject[] boxes = GameObject.FindGameObjectsWithTag("Box");
        foreach (GameObject box in boxes)
        {
            Destroy(box);
        }
        for(int i = 0; i < isDelivery.Count; i++)
        {
            isDelivery[i] = false;
        }
        player = GameObject.FindFirstObjectByType<Player>();
        playerObj = player.gameObject;
        cannonObj = GameObject.FindFirstObjectByType<Cannon>().gameObject;
        cannonTransform = cannonObj.transform;
        canShoot = true;
        isFail = false;
        status = GameStatus.idle;
        foreach(Slider ui in UiManager.Instance.progressUi)
        {
            ui.value = 0;
        }

        player.curBullet = player.maxBullet;
        
    }

    public void Clear()
    {
        
        UiManager.Instance.OpenResultScreen();
    }

    public static GameManager Instance
    {
        get
        {
            if(instance == null)
            {
                return null;
            }
            return instance;
        }
    }
}
