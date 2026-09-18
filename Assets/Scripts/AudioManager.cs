using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public Button soundButton;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;
    public GameObject bgMusic;

    private bool isSoundOn = true;

    private void Start()
    {
        // Load saved state (optional)
        isSoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;

        UpdateSoundState();

        if (soundButton != null)
            soundButton.onClick.AddListener(ToggleSound);
    }

    private void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        PlayerPrefs.SetInt("SoundOn", isSoundOn ? 1 : 0);

        UpdateSoundState();
    }

    private void UpdateSoundState()
    {
        if (bgMusic != null)
            bgMusic.SetActive(isSoundOn);

        if (soundButton != null && soundButton.image != null)
        {
            soundButton.image.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
        }
    }
}
