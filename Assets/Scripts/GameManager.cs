using System.Collections.Generic;
using TargetIndicators;
using UnityEngine;
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

    
    [Header("게임 상태")]
    public GameStatus status;
    public bool isFail = false;

    [Header("플레이어")]
    public GameObject playerObj;
    public Player player;

    [Header("대포")]
    public Transform cannonTransform;
    public GameObject CannonPrefab;
    public GameObject cannonObj;
    public bool canShoot = true;//플레이어 발사 후 false 처리

    [Header("택배 마커")]
    public List<Transform> targets = new List<Transform>();
    TargetIndicatorManager targetIndicatorManager;
    VisualIndicatorManager visualIndicatorManager;



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

    }

   

  


    public void ReStart()
    {
        
        Destroy(player.gameObject);
        Destroy(cannonObj);
        StageManager.Instance.checkTime = false;
        GameObject obj = Instantiate(CannonPrefab, cannonTransform.position,cannonTransform.rotation);
        GameObject[] boxes = GameObject.FindGameObjectsWithTag("Box");
        foreach (GameObject box in boxes)
        {
            Destroy(box);
        }
        for(int i = 0; i < DeliveryManager.Instance.isDelivery.Count; i++)
        {
            DeliveryManager.Instance.isDelivery[i] = false;
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
        //SoundManager.Instance.bgmSource.Stop();
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
