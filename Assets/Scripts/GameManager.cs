using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public Transform cannonTransform;

    public int failCount = 0;

    public GameObject playerObj;
    public Player player;

    public GameObject CannonPrefab;

    public GameObject cannonObj;

    public bool isFail = false;
    public bool canShoot = true;
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
    }

    public void ReStart()
    {
        Destroy(player);
        Destroy(cannonObj);
        GameObject obj = Instantiate(CannonPrefab, cannonTransform.position,cannonTransform.rotation);
        player = GameObject.FindFirstObjectByType<Player>();
        playerObj = player.gameObject;
        cannonObj = GameObject.FindFirstObjectByType<Cannon>().gameObject;
        cannonTransform = cannonObj.transform;
        canShoot = true;
        isFail = false;
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
