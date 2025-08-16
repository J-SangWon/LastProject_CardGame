using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;

    [Header("Audio Clips")]
    public List<AudioClip> bgmClips;
    public List<AudioClip> sfxClips;

    [Header("SFX Pool Settings")]
    public AudioSource sfxPrefab;  // 빈 AudioSource 프리팹
    public int sfxPoolSize = 10;

    private Dictionary<string, AudioClip> bgmDict;
    private Dictionary<string, AudioClip> sfxDict;
    private Queue<AudioSource> sfxPool;

    private float bgmVolume = 1f;
    private float sfxVolume = 1f;
    private bool bgmMute = false;
    private bool sfxMute = false;

    private const string BGM_VOL_KEY = "BGM_VOLUME";
    private const string SFX_VOL_KEY = "SFX_VOLUME";
    private const string BGM_MUTE_KEY = "BGM_MUTE";
    private const string SFX_MUTE_KEY = "SFX_MUTE";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Init()
    {
        // 사운드 딕셔너리 초기화
        bgmDict = new Dictionary<string, AudioClip>();
        foreach (var clip in bgmClips)
            if (!bgmDict.ContainsKey(clip.name))
                bgmDict.Add(clip.name, clip);

        sfxDict = new Dictionary<string, AudioClip>();
        foreach (var clip in sfxClips)
            if (!sfxDict.ContainsKey(clip.name))
                sfxDict.Add(clip.name, clip);

        // 볼륨 로드
        bgmVolume = PlayerPrefs.GetFloat(BGM_VOL_KEY, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOL_KEY, 1f);
        bgmMute = PlayerPrefs.GetInt(BGM_MUTE_KEY, 0) == 1; //1이면 true
        sfxMute = PlayerPrefs.GetInt(SFX_MUTE_KEY, 0) == 1;

        bgmSource.volume = bgmVolume;

        // SFX 풀 생성
        sfxPool = new Queue<AudioSource>();
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource newSFX = Instantiate(sfxPrefab, transform);
            newSFX.playOnAwake = false;
            sfxPool.Enqueue(newSFX);
        }
    }

    #region BGM
    public void PlayBGM(string name, float fadeTime = 0f)
    {
        int ran = 0;
        if (ran == Random.Range(0, 2)) 
            name = name + "01";
        else
            name = name + "02";

        if (!bgmDict.ContainsKey(name))
        {
            Debug.LogWarning($"BGM '{name}' not found!");
            return;
        }

        if (bgmSource.clip != null && bgmSource.clip.name == name)
            return; // 같은 곡이면 재생 안함

        StopAllCoroutines();

        if (fadeTime > 0)
            StartCoroutine(FadeBGM(bgmDict[name], fadeTime));
        else
        {
            bgmSource.clip = bgmDict[name];
            bgmSource.Play();
        }
    }

    IEnumerator FadeBGM(AudioClip newClip, float fadeTime)
    {
        float startVolume = bgmSource.volume;

        // 페이드 아웃
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0, t / fadeTime);
            yield return null;
        }
        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.Play();

        // 페이드 인
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0, startVolume, t / fadeTime);
            yield return null;
        }
        bgmSource.volume = startVolume;
    }

    public void StopBGM()
    {
        bgmSource.Stop();
        bgmSource.clip = null;
    }
    #endregion

    #region SFX with Pooling
    public void PlaySFX(string name)
    {
        PlaySFXAtPosition(name, Camera.main.transform.position);
    }

    public void PlaySFXAtPosition(string name, Vector3 position)
    {
        if (!sfxDict.ContainsKey(name))
        {
            Debug.LogWarning($"SFX '{name}' not found!");
            return;
        }

        AudioSource sfx = GetSFXFromPool();
        sfx.clip = sfxDict[name];
        sfx.volume = sfxVolume;
        sfx.transform.position = position;
        sfx.gameObject.SetActive(true);
        sfx.Play();

        StartCoroutine(ReturnSFXToPoolAfterPlay(sfx));
    }

    AudioSource GetSFXFromPool()
    {
        if (sfxPool.Count > 0)
        {
            return sfxPool.Dequeue();
        }
        else
        {
            // 풀 부족 시 추가 생성 (안정성)
            AudioSource extra = Instantiate(sfxPrefab, transform);
            extra.playOnAwake = false;
            return extra;
        }
    }

    IEnumerator ReturnSFXToPoolAfterPlay(AudioSource sfx)
    {
        yield return new WaitForSeconds(sfx.clip.length);
        sfx.Stop();
        sfx.clip = null;
        sfx.gameObject.SetActive(false);
        sfxPool.Enqueue(sfx);
    }
    #endregion

    #region Volume Control
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
        PlayerPrefs.SetFloat(BGM_VOL_KEY, bgmVolume);
        PlayerPrefs.Save();
    }
  
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_VOL_KEY, sfxVolume);
        PlayerPrefs.Save();
    }
    public void SetBGMMute(bool isOn)
    {
        bgmMute = isOn;
        bgmSource.mute = bgmMute;
        PlayerPrefs.SetInt("BGM_MUTE", bgmMute ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetSFXMute(bool isOn)
    {
        sfxMute = isOn;
        foreach (var sfx in sfxPool)
        {
            if (sfx != null)
                sfx.mute = sfxMute;
        }
        PlayerPrefs.SetInt("SFX_MUTE", sfxMute ? 1 : 0);    //true면 1, false면 0
        PlayerPrefs.Save();
    }

    public float GetBGMVolume() => bgmVolume;
    public float GetSFXVolume() => sfxVolume;
    public bool GetBGMMute() => bgmMute;
    public bool GetSFXMute() => sfxMute;
    #endregion
}
