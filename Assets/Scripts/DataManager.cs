using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class StageRecord
{
    public int stageIndex;
    public bool isCleared;
    public int stars;
    public float bestTime;
    public int bestDeathCount;
    public float currentTime;
    public int currentDeathCount;
}

[System.Serializable]
public class GameData
{
    public List<StageRecord> stages = new List<StageRecord>();
}

public class DataManager : MonoBehaviour
{
    private static DataManager instance;
    public static DataManager Instance => instance;

    private GameData gameData = new GameData();
    private string savePath;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        savePath = Application.persistentDataPath + "/savedata.json";
        LoadData();
    }

    // =====================
    // 저장 / 불러오기
    // =====================

    public void SaveData()
    {
        string json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"저장 완료: {savePath}");
    }

    public void LoadData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            gameData = JsonUtility.FromJson<GameData>(json);
            Debug.Log("불러오기 완료");
        }
        else
        {
            Debug.Log("저장 파일 없음 → 새로 생성");
            gameData = new GameData();
        }
    }

    // =====================
    // 스테이지 데이터
    // =====================

    // 스테이지 기록 가져오기
    public StageRecord GetStageRecord(int stageIndex)
    {
        StageRecord record = gameData.stages.Find(s => s.stageIndex == stageIndex);

        // 없으면 새로 만들기
        if (record == null)
        {
            record = new StageRecord { stageIndex = stageIndex };
            gameData.stages.Add(record);
        }

        return record;
    }

    // 현재 플탐/죽은횟수 저장 (실패할 때마다)
    public void SaveCurrentRecord(int stageIndex, float currentTime, int currentDeathCount)
    {
        StageRecord record = GetStageRecord(stageIndex);
        record.currentTime = currentTime;
        record.currentDeathCount = currentDeathCount;
        SaveData();
    }

    // 클리어 시 저장
    public void SaveClearRecord(int stageIndex, int stars, float time, int deathCount)
    {
        StageRecord record = GetStageRecord(stageIndex);

        record.isCleared = true;
        record.currentTime = time;
        record.currentDeathCount = deathCount;

        // 최고 기록 갱신
        if (stars > record.stars)
            record.stars = stars;

        if (record.bestTime == 0 || time < record.bestTime)
            record.bestTime = time;

        if (record.bestDeathCount == 0 || deathCount < record.bestDeathCount)
            record.bestDeathCount = deathCount;

        SaveData();
    }

    // =====================
    // 설정 (PlayerPrefs)
    // =====================

    public void SaveBGMVolume(float volume)
    {
        PlayerPrefs.SetFloat("BGMVolume", volume);
        PlayerPrefs.Save();
    }

    public void SaveSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    public float LoadBGMVolume()
    {
        return PlayerPrefs.GetFloat("BGMVolume", 0.7f); // 기본값 0.7
    }

    public float LoadSFXVolume()
    {
        return PlayerPrefs.GetFloat("SFXVolume", 0.8f); // 기본값 0.8
    }

    // =====================
    // 앱 종료 시 저장
    // =====================

    void OnApplicationQuit()
    {
        SaveData();
    }
}