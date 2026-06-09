using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    private static DeliveryManager instance;
    [Header("배송 성공 확인")]
    public List<bool> isDelivery = new List<bool>();


    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsDeliveryClear()
    {
        for (int i = 0; i < isDelivery.Count; i++)
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
        for (int i = 0; i < isDelivery.Count; i++)
        {
            if (isDelivery[i])
            {
                num++;
            }
        }

        return num;
    }



    public static DeliveryManager Instance
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
