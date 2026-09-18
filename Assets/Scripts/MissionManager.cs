using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class MissionManager : MonoBehaviour
{
    [Header("UI References")]
    public Text missionText;
    public GameObject rewardPanel;
    public Text rewardText;

    [Header("Missions")]
    public Mission[] missions;
    private int currentMissionIndex = 0;

    private int coinsCollected = 0;
    private CanvasGroup rewardCanvasGroup;

    void Start()
    {
        LoadMissionProgress();

        if (rewardPanel != null)
        {
            rewardCanvasGroup = rewardPanel.GetComponent<CanvasGroup>();
            if (rewardCanvasGroup == null)
                rewardCanvasGroup = rewardPanel.AddComponent<CanvasGroup>();

            rewardPanel.SetActive(false);
            rewardCanvasGroup.alpha = 0;
        }

        ShowCurrentMission();
    }

    public void OnCoinCollected()
    {
        if (currentMissionIndex >= missions.Length)
            return; // all missions completed

        coinsCollected++;
        UpdateMissionText();

        var currentMission = missions[currentMissionIndex];
        if (!currentMission.isComplete && coinsCollected >= currentMission.targetCoins)
        {
            currentMission.isComplete = true;
            SaveMissionProgress();
            StartCoroutine(ShowRewardPanel(currentMission.rewardCoins));
        }
    }

    void ShowCurrentMission()
    {
        if (currentMissionIndex < missions.Length)
        {
            var m = missions[currentMissionIndex];
            missionText.gameObject.SetActive(true);
            coinsCollected = 0;
            UpdateMissionText();
        }
        else
        {
            // ✅ Show final message when all missions complete
            missionText.gameObject.SetActive(true);
            missionText.text = "All missions complete!";
        }
    }

    void UpdateMissionText()
    {
        if (currentMissionIndex >= missions.Length)
        {
            missionText.text = "All missions complete!";
            return;
        }

        var m = missions[currentMissionIndex];
        missionText.text = $"{m.description} ({coinsCollected}/{m.targetCoins})";
    }

    IEnumerator ShowRewardPanel(int reward)
    {
        // ✅ Give reward to player immediately
        if (GameManager.instance != null)
        {
            GameManager.instance.AddCoin(reward);
        }

        // ✅ Update UI for reward
        rewardPanel.SetActive(true);
        rewardText.text = $" +{reward} Coins!";

        float duration = 1f;
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            rewardCanvasGroup.alpha = Mathf.Lerp(0, 1, t / duration);
            yield return null;
        }

        rewardCanvasGroup.alpha = 1;

        // Hide the mission text once completed
        missionText.gameObject.SetActive(false);

        StartCoroutine(HideRewardPanel());
    }



    IEnumerator HideRewardPanel()
    {
        float duration = 1f;
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            rewardCanvasGroup.alpha = Mathf.Lerp(1, 0, t / duration);
            yield return null;
        }

        rewardCanvasGroup.alpha = 0;
        rewardPanel.SetActive(false);

        NextMission();
    }

    void NextMission()
    {
        currentMissionIndex++;
        SaveMissionProgress();

        if (currentMissionIndex < missions.Length)
            ShowCurrentMission();
        else
            missionText.text = "All missions complete!";

    }

    // -------------------------------
    // 💾 SAVE / LOAD PROGRESS
    // -------------------------------
    void SaveMissionProgress()
    {
        PlayerPrefs.SetInt("CurrentMissionIndex", currentMissionIndex);

        for (int i = 0; i < missions.Length; i++)
        {
            PlayerPrefs.SetInt($"MissionComplete_{i}", missions[i].isComplete ? 1 : 0);
        }

        PlayerPrefs.Save();
    }

    void LoadMissionProgress()
    {
        currentMissionIndex = PlayerPrefs.GetInt("CurrentMissionIndex", 0);

        for (int i = 0; i < missions.Length; i++)
        {
            missions[i].isComplete = PlayerPrefs.GetInt($"MissionComplete_{i}", 0) == 1;
        }

        // If all missions complete, jump to last one
        if (currentMissionIndex >= missions.Length)
            currentMissionIndex = missions.Length;
    }

    public void ResetMissions()
    {
        PlayerPrefs.DeleteKey("CurrentMissionIndex");
        for (int i = 0; i < missions.Length; i++)
        {
            PlayerPrefs.DeleteKey($"MissionComplete_{i}");
            missions[i].isComplete = false;
        }

        currentMissionIndex = 0;
        coinsCollected = 0;
        PlayerPrefs.Save();
        ShowCurrentMission();
    }
}
