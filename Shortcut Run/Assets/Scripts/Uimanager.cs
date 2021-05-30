using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Uimanager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Animator restartBtnAnimator;
    [SerializeField] private Animator winPanelAnimator;
    [SerializeField] private Animator upgradeButtonAnimator;

    [SerializeField] private GameController _gameController;

    [SerializeField] private Text WinCoinsText;
    [SerializeField] private Text WinText;

    [SerializeField] private GameObject nxtLevel;
    [SerializeField] private GameObject restartLevel;
    [SerializeField] private GameObject TapToPlayButton;

    [SerializeField] private Text coinsCount;
    [SerializeField] private Text timer;
    [SerializeField] private Text racePos;

    [SerializeField] private Text currentLevelOnUpgradeButton;
    [SerializeField] private Text currentPercentageText;
    [SerializeField] private TextMeshProUGUI upgradeCostOnButton;

    [SerializeField] private Text[] leaderboardPositions;

    [SerializeField] private string[] levels;

    private int coins;
    void Start()
    {
        string sceneName = PlayerPrefs.GetString("CurrentLevel","1");
        if (sceneName != SceneManager.GetActiveScene().name)
            SceneManager.LoadScene(sceneName);
        coinsCount.text=PlayerPrefs.GetInt("allCoins",0).ToString();
        InitializeUpgradeButton();
        timer.text = "Tap To Play";
    }

    public void InitializeUpgradeButton()
    {
        int curLvl = PlayerPrefs.GetInt("WaterLevel", 1);
        currentPercentageText.text = "(+" + ((curLvl - 1) * 10).ToString() + "%)";
        currentLevelOnUpgradeButton.text ="lvl "+curLvl.ToString();
        if (curLvl >= 10)
            upgradeCostOnButton.text = "MAX LVL";
        else upgradeCostOnButton.text = (curLvl * 1000).ToString();
    }
    public void StartTimer()
    {
        upgradeButtonAnimator.SetBool("isHidden", true);
        coinsCount.text = "0";
        _gameController.StartGame();
        TapToPlayButton.SetActive(false);
        StartCoroutine(Timer());
    }
    public void AddCoins()
    {
        coins+=5;
        coinsCount.text = coins.ToString();
    }
    public void UpdateMnozhitel(int mnozhitel)
    {
        timer.enabled = true;
        timer.text = "x" + mnozhitel;
    }
    public void DisableMnozhitel()
    {
        timer.enabled = false;
    }
    public void ShowWinPanel(int mnozhitel)
    {
        restartLevel.SetActive(false);
        WinCoinsText.text = WinCoinsText.text + coins+" coins"+" x"+mnozhitel;
        winPanelAnimator.enabled = true;
        PlayerPrefs.SetInt("allCoins", PlayerPrefs.GetInt("allCoins", 0) + coins*mnozhitel);
    }
    public void ShowLosePanel()
    {
        nxtLevel.SetActive(false);
        WinText.text = "You Lose!";
        WinCoinsText.text = WinCoinsText.text + coins + " coins";
        winPanelAnimator.enabled = true;
        PlayerPrefs.SetInt("allCoins", PlayerPrefs.GetInt("allCoins", 0) + coins);
    }
    public void ShowRestartButton()
    {
        restartBtnAnimator.enabled = true;
    }
    public void NextLevel()
    {
        
        if (PlayerPrefs.GetInt("Randomize") == 0)
        {
            if (SceneManager.GetActiveScene().buildIndex != SceneManager.sceneCountInBuildSettings - 1)
            {
                int level = SceneManager.GetActiveScene().buildIndex + 2;
                PlayerPrefs.SetString("CurrentLevel", level.ToString());
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
                PlayerPrefs.SetInt("Randomize", 1);
                string level;
                do
                {
                    level = Random.Range(1, SceneManager.sceneCountInBuildSettings).ToString();
                }
                while (level == SceneManager.GetActiveScene().name);
                PlayerPrefs.SetString("CurrentLevel", level);
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        else
        {
            string level;
            do
            {
                level = Random.Range(1, SceneManager.sceneCountInBuildSettings).ToString();
            }
            while (level == SceneManager.GetActiveScene().name);
            PlayerPrefs.SetString("CurrentLevel", level);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void UpgradeWater()
    {
        int currentLevel = PlayerPrefs.GetInt("WaterLevel", 1);
        int upgradeCost = currentLevel * 1000;
        int allCoins = PlayerPrefs.GetInt("allCoins", 0);
        
        if (allCoins>=upgradeCost&&currentLevel<10)
        {
            PlayerPrefs.SetInt("WaterLevel", currentLevel + 1);
            allCoins -= upgradeCost;
            PlayerPrefs.SetInt("allCoins", allCoins);
            coinsCount.text = allCoins.ToString();
        }
        InitializeUpgradeButton();
    }
    public void UpdateRacePosition(int pos)
    {
        switch(pos)
        {
            case 1: racePos.enabled = true; racePos.text = "1st";break;
            case 2: racePos.enabled = true; racePos.text = "2nd";break;
            case 3: racePos.enabled = true; racePos.text = "3rd";break;
            case 4: racePos.enabled = true; racePos.text = "4th";break;
            case 5: racePos.enabled = true; racePos.text = "5th";break;
            case 0: racePos.enabled = false;break;
        }
    }

    public void UpdateLeaderBoard(int pos,string _name)
    {
        switch (pos)
        {
            case 1: leaderboardPositions[0].text = "1."+_name; break;
            case 2: leaderboardPositions[1].text = "2." + _name; break;
            case 3: leaderboardPositions[2].text = "3." + _name; break;
            case 4: leaderboardPositions[3].text = "4." + _name; break;
            case 5: leaderboardPositions[4].text = "5." + _name; break;
        }
    }
    private IEnumerator Timer()
    {
        for(int i = 2;i>=0;i--)
        {
            timer.text = i + 1 + "";
            yield return new WaitForSeconds(1);
        }
        timer.enabled = false;
        _gameController.gamestarted = true;
    }
    private void OnApplicationQuit()
    {
        PlayerPrefs.SetString("CurrentLevel", SceneManager.GetActiveScene().name);
    }
}
