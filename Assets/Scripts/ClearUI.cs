using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearUI : MonoBehaviour
{
    public TextMeshProUGUI playTimeTxt;
    public TextMeshProUGUI deliveryTxt;
    public TextMeshProUGUI taskTxt;
    public TextMeshProUGUI deathCountTxt;

    public int stars;
    public GameObject starObj;
    public GameObject starParent;

    public void ShowClearUI()
    {
        StartCoroutine(ClearSequence());
    }

    IEnumerator ClearSequence()
    {   
        stars = StageManager.Instance.CalculateStars();
        yield return new WaitForSeconds(0.8f);
        SoundManager.Instance.PlaySFX(SFXType.Clear);
        yield return new WaitForSeconds(0.8f);
        // 1 택배 배송 결과
        deliveryTxt.gameObject.SetActive(true);
        deliveryTxt.text = "택배 배송하기 (" + DeliveryManager.Instance.DeliveryNum() + "/" + DeliveryManager.Instance.isDelivery.Count + ")";
        yield return new WaitForSeconds(0.3f);

        // 2 목표 도달
        taskTxt.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.3f);

        // 3 시간 카운트
        float targetTime = StageManager.Instance.playTime;
        float curTime = 0;
        playTimeTxt.gameObject.SetActive(true);
        while (curTime < targetTime)
        {
            curTime += Time.deltaTime * 20f;

            playTimeTxt.text = "배송시간 : "
                + UiManager.Instance.FormatTime(curTime);
            
            yield return null;
        }
        
        playTimeTxt.text = "배송시간 : "
            + UiManager.Instance.FormatTime(targetTime);

        yield return new WaitForSeconds(0.01f);

        deathCountTxt.gameObject.SetActive(true);
        // 4 사고 횟수 카운트
        int targetDeath = StageManager.Instance.failCount;

        for (int i = 0; i <= targetDeath; i++)
        {
            deathCountTxt.text = "사고 횟수 : " + i;
            
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.2f);

        

        for (int i = 0; i < stars; i++)
        {
            GameObject obj = Instantiate(starObj, starParent.transform);
            yield return new WaitForSeconds(0.2f);
        }
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
