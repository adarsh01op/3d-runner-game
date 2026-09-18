using Microsoft.CodeAnalysis.FlowAnalysis;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static event System.Action<Transform> OnPlayerChanged;

    [Header("UI Panels")]
    public GameObject menuUI;
    public GameObject gameUI;   // HUD during game
    public GameObject deathUI;
    public GameObject characterSelectionUI;
    public GameObject resetConfirmationPanel;


    [Header("HUD UI Elements")]
    public Text heartsText;
    public Text scoreText;
    public Text highScoreText;
    public Text coinText;
    public Text diamondText;

    [Header("Gameplay")]
    public int startingHearts = 3;
    public int maxHearts = 5;

    [Header("References")]
    public GameObject player;


    public Transform currentPlayer;


    public Camera mainCamera;
    public Transform frontCamPos;
    public Transform runCamPos;
    public float cameraTransitionSpeed = 2f;

    private int currentHearts;
    private int currentScore;
    private int currentCoins;
    private int currentDiamonds;
    private bool isGameStarted = false;
    private bool isGameOver = false;

    private Animator playerAnim;
    private bool isTransitioning = false;
    private Vector3 camStartPos;
    private Quaternion camStartRot;

    public ChaserFollow chaser;

    [Header("Distance Rewards")]
    public float rewardInterval = 250f; // 250 meters per reward
    public int rewardCoins = 20;
    private float nextRewardDistance = 250f;

    



    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (instance == null) instance = this;
        else Destroy(gameObject);
    }


    public void SetCurrentPlayer(Transform newPlayer)
    {
        currentPlayer = newPlayer;
        OnPlayerChanged?.Invoke(newPlayer);
    }
    private void Start()
    {
        Time.timeScale = 0f; // freeze game
        currentHearts = startingHearts;
        currentScore = 0;
        currentCoins = 0;
        currentDiamonds = 0;

        playerAnim = player != null ? player.GetComponent<Animator>() : null;

        menuUI.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false);
        if (deathUI != null) deathUI.SetActive(false);

        UpdateHUD();

        // show high score on main menu
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
            highScoreText.text = ": " + highScore;


        // Start camera and idle animation
        if (mainCamera != null && frontCamPos != null)
        {
            mainCamera.transform.position = frontCamPos.position;
            mainCamera.transform.rotation = frontCamPos.rotation;
        }

        if (playerAnim != null)
            playerAnim.Play("idle");
    }

    public bool IsGameStarted() => isGameStarted;
    public bool IsGameOver() => isGameOver;

    public void StartGame()
    {


        if (isGameStarted || isTransitioning) return;

        StartCoroutine(StartGameCinematic());
    }

    private IEnumerator StartGameCinematic()
    {
        isTransitioning = true;
        Time.timeScale = 1f;

        if (mainCamera != null && runCamPos != null)
        {
            camStartPos = mainCamera.transform.position;
            camStartRot = mainCamera.transform.rotation;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * cameraTransitionSpeed;
                mainCamera.transform.position = Vector3.Lerp(camStartPos, runCamPos.position, t);
                mainCamera.transform.rotation = Quaternion.Slerp(camStartRot, runCamPos.rotation, t);
                yield return null;
            }
        }

        // Player starts running
        if (playerAnim != null)
            playerAnim.Play("Running");

        isTransitioning = false;
        isGameStarted = true;

        // after isGameStarted = true;
        if (chaser != null)
        {
            chaser.StartFollowing();

        }



        isGameOver = false;



        // UI toggles
        menuUI.SetActive(false);
        if (gameUI != null) gameUI.SetActive(true);
        if (deathUI != null) deathUI.SetActive(false);
        if (characterSelectionUI != null) characterSelectionUI.SetActive(false);
    }

    // Called when Reset button is clicked
    public void OnResetButtonClicked()
    {
        if (resetConfirmationPanel != null)
            resetConfirmationPanel.SetActive(true);
    }

    // Called when user presses "No"
    public void OnCancelReset()
    {
        if (resetConfirmationPanel != null)
            resetConfirmationPanel.SetActive(false);
    }

    // Called when user presses "Yes"
    public void OnConfirmReset()
    {
        // ✅ Reset all data
        PlayerPrefs.SetInt("Coins", 0);
        PlayerPrefs.SetInt("Character0_Unlocked", 1);
        PlayerPrefs.SetInt("Character1_Unlocked", 0);
        PlayerPrefs.SetInt("Character2_Unlocked", 0);
        PlayerPrefs.SetInt("SelectedPlayer", 0);
        PlayerPrefs.SetInt("HighScore", 0);

        if (missionManager != null)
            missionManager.ResetMissions();

        PlayerPrefs.Save();

        Debug.Log("All progress reset!");

        // Close panel
        if (resetConfirmationPanel != null)
            resetConfirmationPanel.SetActive(false);

        // Reload scene to refresh everything
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    public void UpdateDistance(float currentDistance)
    {
        // Check if player reached milestone
        if (currentDistance >= nextRewardDistance)
        {
            GiveDistanceReward();
            nextRewardDistance += rewardInterval;
        }

    }
    private void GiveDistanceReward()
    {
        currentCoins += rewardCoins;
        currentScore += rewardCoins * 10;

        // ? Use same key as everywhere else
        int totalCoins = PlayerPrefs.GetInt("Coins", 0) + rewardCoins;
        PlayerPrefs.SetInt("Coins", totalCoins);
        PlayerPrefs.Save();

        UpdateHUD();
        ShowRewardPopup($"You ran {nextRewardDistance}m!\n+{rewardCoins} Coins!");
    }



    [Header("UI - Reward Popup")]
    public GameObject distanceRewardPanel;
    // public Text distanceCoveredText;

    public void ShowRewardPopup(string message)
    {
        if (distanceRewardPanel == null) return;
        StopAllCoroutines(); // stop any previous fade
        StartCoroutine(FadeRewardPopup(message));
    }

    private IEnumerator FadeRewardPopup(string message)
    {
        distanceRewardPanel.SetActive(true);


        CanvasGroup group = distanceRewardPanel.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = distanceRewardPanel.AddComponent<CanvasGroup>();
        }

        // Fade In
        for (float t = 0; t < 1f; t += Time.deltaTime * 2)
        {
            group.alpha = t;
            yield return null;
        }

        group.alpha = 1f;
        yield return new WaitForSeconds(2f);

        // Fade Out
        for (float t = 1f; t > 0f; t -= Time.deltaTime * 2)
        {
            group.alpha = t;
            yield return null;
        }

        group.alpha = 0f;
        distanceRewardPanel.SetActive(false);
    }




    // ------------------ HEARTS ------------------
    public void LoseHeart()
    {
        if (isGameOver) return;

        currentHearts--;
        UpdateHUD();

        if (currentHearts <= 0)
        {
            GameOver(3f);
        }
    }

    public void GainHeart()
    {
        currentHearts = Mathf.Clamp(currentHearts + 1, 0, maxHearts);
        UpdateHUD();
    }

    // ------------------ SCORE + COINS ------------------
    public MissionManager missionManager; // add at top of class

    public void AddCoin(int amount)
    {
        currentCoins += amount;
        currentScore += amount * 10;

        // ? Add to total saved coins too
        int totalCoins = PlayerPrefs.GetInt("Coins", 0);
        totalCoins += amount;
        PlayerPrefs.SetInt("Coins", totalCoins);
        PlayerPrefs.Save();

        UpdateHUD();

        if (missionManager != null)
            missionManager.OnCoinCollected();
    }



    public void AddDiamonds(int amount)
    {
        currentDiamonds += amount;
        currentScore += amount * 20; // 1 diamond = 20 score
        UpdateHUD();
    }



    private void UpdateHUD()
    {
        if (heartsText != null) heartsText.text = "Hearts: " + currentHearts;
        if (scoreText != null) scoreText.text = "Score: " + currentScore;
        if (coinText != null) coinText.text = "Coins: " + currentCoins;
        if (diamondText != null) diamondText.text = "Gems: " + currentDiamonds;
    }

    // ------------------ GAME OVER ------------------
    public void GameOver(float delay = 3f)
    {
        if (!isGameOver)
            StartCoroutine(GameOverRoutine(delay));
    }

    private IEnumerator GameOverRoutine(float delay)
    {
        isGameOver = true;

        // Tell chaser to catch
        if (chaser != null)
            chaser.CatchPlayer();

        // Wait until the chaser finishes catching the player
        yield return new WaitForSeconds(2f); // give time for catch-up animation

        // Now freeze time AFTER chaser reached player
        yield return new WaitForSecondsRealtime(delay);

        // Save high score
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (currentScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
            highScore = currentScore;
        }

        // ? Keep total coins intact, don’t overwrite
        int totalCoins = PlayerPrefs.GetInt("Coins", 0) + currentCoins;
        PlayerPrefs.SetInt("Coins", totalCoins);
        PlayerPrefs.Save();


        // Freeze after catch is done
        Time.timeScale = 0f;

        if (deathUI != null) deathUI.SetActive(true);
        if (highScoreText != null) highScoreText.text = "High Score: " + highScore;
    }


    // ------------------ RESTART / QUIT ------------------
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetPlayerAnimator(Animator newAnim)
    {
        playerAnim = newAnim;
    }
}
