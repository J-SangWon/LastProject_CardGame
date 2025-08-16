using UnityEngine;
using UnityEngine.UI;

public class SoundUIController : MonoBehaviour
{
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Text bgmValueText;
    public Text sfxValueText;
    public Toggle bgmToggle;
    public Toggle sfxToggle;
    public Text bgmToggleText;
    public Text sfxToggleText;
    public Button backButton;

    void Start()
    {
        // 저장된 볼륨 불러와서 슬라이더 초기화
        if (SoundManager.Instance != null)
        {
            bgmSlider.value = SoundManager.Instance.GetBGMVolume();
            sfxSlider.value = SoundManager.Instance.GetSFXVolume();
            bgmValueText.text = $"{bgmSlider.value * 100:F0}%";
            sfxValueText.text = $"{sfxSlider.value * 100:F0}%";
            bgmToggle.isOn = SoundManager.Instance.GetBGMMute();    //true면 켜짐
            sfxToggle.isOn = SoundManager.Instance.GetSFXMute();
            bgmToggleText.text = bgmToggle.isOn ? "OFF" : "ON"; // 체크가 on이면 소리off
            sfxToggleText.text = sfxToggle.isOn ? "OFF" : "ON"; // 토글 상태에 따라 텍스트 변경
        }

        // 슬라이더 값 변경 시 사운드 매니저에 적용
        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        // 토글 상태 변경 시 사운드 매니저에 적용
        bgmToggle.onValueChanged.AddListener((isOn) =>
        {
            SoundManager.Instance.PlaySFX("MENUSELECT_03"); // 토글 변경 시 효과음 재생
            SoundManager.Instance.SetBGMMute(isOn);
            bgmToggleText.text = isOn ? "OFF" : "ON"; // 체크가 on이면 소리off
        });

        sfxToggle.onValueChanged.AddListener((isOn) =>
        {
            SoundManager.Instance.PlaySFX("MENUSELECT_03"); // 토글 변경 시 효과음 재생
            SoundManager.Instance.SetSFXMute(isOn);
            sfxToggleText.text = isOn ? "OFF" : "ON"; // 체크가 on이면 소리off
        });

        // 뒤로가기 버튼 클릭 시 메인 메뉴로 돌아가기
        if (backButton != null)
        {
            backButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX("MENUSELECT_03");
                Destroy(gameObject); // 현재 UI 오브젝트 제거
            });
        }
    }

    void OnBGMVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetBGMVolume(value);

        // BGM 볼륨 텍스트 업데이트
        if (bgmValueText != null)
            bgmValueText.text = $"{value * 100:F0}%";
    }

    void OnSFXVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetSFXVolume(value);

        // SFX 볼륨 텍스트 업데이트
        if (sfxValueText != null)
            sfxValueText.text = $"{value * 100:F0}%";
    }

}
