using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using JetBrains.Annotations;

public class UiManager : MonoBehaviour
{
    private static UiManager instance;

    public TextMeshProUGUI deliveryTxt;
    public TextMeshProUGUI arrivedTxt;
    public TextMeshProUGUI deathCountTxt;

    public GameObject controlUI;

    public GameObject clearObj;

    public RectTransform TaskUI;   // 아래로 내려갈 UI
    public CanvasGroup ClearBg;     // Fade될 UI
    public RectTransform ResultUI;   // 올라올 UI

    public float downY = -200f;     // A가 내려갈 위치
    public float targetCY = 100f;   // C가 올라올 목표 위치

    public bool taskActive = true;

    public TextMeshProUGUI timeText;
    public List<Slider> progressUi = new List<Slider>();

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
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        progressUi[1].maxValue = GameManager.Instance.isDelivery.Count;
        ClearBg.DOFade(0f, 0);
        clearObj.SetActive(false);
        
    }

    

    


        // Update is called once per frame
        void Update()
    {
        if(GameManager.Instance.status == GameStatus.idle)
        {
            controlUI.SetActive(true);
        }
        else
        {
            controlUI.SetActive(false) ;
        }

        if (GameManager.Instance.IsDeliveryClear())
        {
            deliveryTxt.color = new Color32(8,255,0,255);
        }
        else
        {
            deliveryTxt.color = Color.white;
        }
        deathCountTxt.text = "X" + GameManager.Instance.failCount;
        deliveryTxt.text = "택배 배송하기 (" + GameManager.Instance.DeliveryNum() + "/" + GameManager.Instance.isDelivery.Count + ")";

        timeText.text = FormatTime(GameManager.Instance.playTime);


        if(Input.GetKeyDown(KeyCode.Tab))
        {
            if (taskActive)
            {
                taskActive = false;
                Sequence seq = DOTween.Sequence();

                seq.Append(
                    TaskUI.DOAnchorPosY(-300f, 0.5f)
                           .SetEase(Ease.InQuad)
                );
            }
            else
            {

                taskActive = true;
                Sequence seq = DOTween.Sequence();

                seq.Append(
                    TaskUI.DOAnchorPosY(272, 0.5f)
                           .SetEase(Ease.InQuad)
                );
            }
        }

    }

    public string FormatTime(float time)
    {
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        return $"{minutes:00}:{seconds:00}";
    }

    public void OpenResultScreen()
    {
        clearObj.SetActive(true);
        Sequence seq = DOTween.Sequence();

        seq.Append(
            TaskUI.DOAnchorPosY(-300f, 0.5f)
                   .SetEase(Ease.InQuad)
        );

        seq.Append(
            ClearBg.DOFade(1f, 0.5f)
        );

        seq.Append(
            ResultUI.DOAnchorPosY(1.5f, 0.5f)
                   .SetEase(Ease.OutBack)
        );

        ResultUI.transform.parent.GetComponent<ClearUI>().ShowClearUI();
    }

    public static UiManager Instance
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
