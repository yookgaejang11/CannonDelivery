using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    public AudioClip bgmClip;

    [Header("UI Sliders (자동 연결 가능)")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("SFX Clips")]
    public List<SFXData> sfxList = new List<SFXData>();
    private Dictionary<SFXType, AudioClip> sfxDict = new Dictionary<SFXType, AudioClip>();

    private float bgmVolume = 1f;
    private float sfxVolume = 1f;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // 씬 로드 시 호출될 함수 등록
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // SFX Dictionary 설정
        foreach (var sfx in sfxList)
        {
            if (!sfxDict.ContainsKey(sfx.type))
                sfxDict.Add(sfx.type, sfx.clip);
        }

        // 저장된 볼륨 불러오기
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        ApplyVolume();
        bgmSource.clip = bgmClip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    private void ApplyVolume()
    {
        if (bgmSource != null) bgmSource.volume = bgmVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    public void SetBGMVolume(float value)
    {
        bgmVolume = value;
        if (bgmSource != null) bgmSource.volume = value;
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        if (sfxSource != null) sfxSource.volume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void PlaySFX(SFXType type)
    {
        if (sfxDict.ContainsKey(type) && sfxDict[type] != null)
        {
            sfxSource.PlayOneShot(sfxDict[type], sfxVolume);
        }
        else
        {
            Debug.LogWarning($"[SoundManager] '{type}' SFX가 등록되지 않았습니다.");
        }
    }
    private void Start()
    {
        TryFindSliders();
    }
    // 🔄 씬이 바뀔 때마다 자동으로 새 슬라이더 찾아 연결
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryFindSliders();
        #region 예전 코드의 페해
        /*if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            bgmSource.Stop();
            bgmSource.clip = bgm1;
            bgmSource.loop = true;
            bgmSource.Play();
        }
        else if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            bgmSource.Stop();
            bgmSource.clip = bgm2;
            bgmSource.loop = true;
            bgmSource.Play();
        }*/
        #endregion
    }



    private void TryFindSliders()
    {
        // 활성/비활성 오브젝트 포함해서 전부 탐색
        var allSliders = Resources.FindObjectsOfTypeAll<Slider>();

        foreach (var slider in allSliders)
        {
            if (slider.CompareTag("bgmSlider"))
                bgmSlider = slider;
            else if (slider.CompareTag("sfxSlider"))
                sfxSlider = slider;
        }

        if (bgmSlider != null)
        {
            bgmSlider.value = bgmVolume;
            bgmSlider.onValueChanged.RemoveAllListeners();
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVolume;
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }
}

[System.Serializable]
public class SFXData
{
    public SFXType type;
    public AudioClip clip;
}

public enum SFXType
{
    fail,
    bonk,
    cannon_shot,
    boxDrop,
    shootBox,
    beep,
    ringing,
    Clear,

}