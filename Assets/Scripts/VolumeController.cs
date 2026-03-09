using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeController : MonoBehaviour
{
    [SerializeField] private Button volumeButton;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Sprite volumeOffSprite;
    [SerializeField] private Sprite volumeOnSprite;

    private bool isMuted = false;
    private float previousVolume = 1f;

    private const string VolumePrefKey = "Volume";
    private const string MutePrefKey = "IsMuted";

    void Start()
    {
        LoadVolumeSettings();

        volumeSlider.onValueChanged.AddListener(SetVolume);
        volumeButton.onClick.AddListener(ToggleMute);
        UpdateButtonSprite();
    }

    public void SetVolume(float volume)
    {
        if (volume == 0)
        {
            audioMixer.SetFloat("VolumeOfMusic", -80f);
            isMuted = true;
        }
        else
        {
            audioMixer.SetFloat("VolumeOfMusic", Mathf.Log10(volume) * 20);
            isMuted = false;
        }

        if (volume > 0 && isMuted)
        {
            isMuted = false;
        }

        // Сохранение значения громкости
        SaveVolumeSettings(volume, isMuted);

        UpdateButtonSprite();
    }

    public void ToggleMute()
    {
        if (isMuted)
        {
            // Восстановить предыдущую громкость
            SetVolume(previousVolume);
            volumeSlider.value = previousVolume;
            isMuted = false;
        }
        else
        {
            // Сохранить текущую громкость и установить её в ноль
            previousVolume = volumeSlider.value;
            SetVolume(0f);
            volumeSlider.value = 0f;
            isMuted = true;
        }

        // Сохранение состояния мутирования
        SaveVolumeSettings(volumeSlider.value, isMuted);

        UpdateButtonSprite();
    }

    private void UpdateButtonSprite()
    {
        if (isMuted)
        {
            volumeButton.image.sprite = volumeOffSprite;
        }
        else
        {
            volumeButton.image.sprite = volumeOnSprite;
        }
    }

    private void SaveVolumeSettings(float volume, bool muted)
    {
        PlayerPrefs.SetFloat(VolumePrefKey, volume);
        PlayerPrefs.SetInt(MutePrefKey, muted ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("Настройки громкости сохранены.");
    }

    private void LoadVolumeSettings()
    {
        if (PlayerPrefs.HasKey(VolumePrefKey))
        {
            float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
            bool savedMuted = PlayerPrefs.GetInt(MutePrefKey, 0) == 1;

            volumeSlider.value = savedVolume;
            SetVolume(savedMuted ? 0f : savedVolume);
            isMuted = savedMuted;
            previousVolume = savedVolume;
            Debug.Log("Настройки громкости загружены.");
        }
        else
        {
            // Установка значений по умолчанию, если настроек нет
            volumeSlider.value = 1f;
            SetVolume(1f);
            isMuted = false;
            Debug.Log("Используются настройки громкости по умолчанию.");
        }
    }
}