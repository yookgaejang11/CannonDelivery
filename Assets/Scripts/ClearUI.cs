using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearUI : MonoBehaviour
{
    public TextMeshProUGUI playTimeTxt;
    public TextMeshProUGUI taskTxt;
    public TextMeshProUGUI Deathcount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playTimeTxt.text ="배송시간 : "+ UiManager.Instance.FormatTime(GameManager.Instance.playTime);
        taskTxt.text = "택배 배송하기 (" + GameManager.Instance.DeliveryNum() + "/" + GameManager.Instance.isDelivery.Count + ")";
        Deathcount.text = "사고 횟수 : "+GameManager.Instance.failCount.ToString();
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Menu()
    {

    }

    public void NextStage()
    {

    }
}
