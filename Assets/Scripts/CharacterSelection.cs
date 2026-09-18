using UnityEngine;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour
{
    [Header("Players")]
    public GameObject[] players; // 0 = default, 1 = second, 2 = third

    [Header("UI Buttons")]
    public Button[] characterButtons; // buttons for each character
    public Text[] buttonTexts;

    [Header("Settings")]
    public Text coinText; // Still keeps your coin UI working on this screen

    void Start()
    {
        SetActivePlayer(PlayerPrefs.GetInt("SelectedPlayer", 0));
        UpdateButtonStates();
    }

    void Update()
    {
        // Continuously update the coin UI, but stop checking for unlocks
        if (coinText != null)
        {
            int coins = PlayerPrefs.GetInt("Coins", 0);
            coinText.text = coins.ToString();
        }
    }

    // Called when you press a character button
    public void OnCharacterButtonPressed(int index)
    {
        // Because all characters are unlocked, we just set the active player directly
        SetActivePlayer(index);
    }

    private void SetActivePlayer(int index)
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].SetActive(i == index);
        }

        PlayerPrefs.SetInt("SelectedPlayer", index);
        PlayerPrefs.Save();

        // Update GameManager & Camera to follow new active player
        GameObject newPlayer = players[index];

        // Update GameManager player reference
        if (GameManager.instance != null)
        {
            GameManager.instance.player = newPlayer;
        }

        // Update CameraFollow target
        CameraFollow camFollow = FindObjectOfType<CameraFollow>();
        if (camFollow != null)
        {
            camFollow.target = newPlayer.transform;
        }

        // Reassign Animator reference in GameManager
        if (GameManager.instance != null)
        {
            Animator newAnim = newPlayer.GetComponent<Animator>();
            GameManager.instance.SendMessage("SetPlayerAnimator", newAnim, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void UpdateButtonStates()
    {
        for (int i = 0; i < characterButtons.Length; i++)
        {
            // Make every button permanently clickable
            characterButtons[i].interactable = true;

            if (buttonTexts != null && buttonTexts.Length > i)
            {
                // Force every button to just say "Select"
                buttonTexts[i].text = "Select";
            }
        }
    }
}