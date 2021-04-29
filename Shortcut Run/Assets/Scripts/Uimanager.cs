using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Uimanager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Animator restartBtnAnimator;
    [SerializeField] private Animator winPanelAnimator;

    [SerializeField] private GameController _gameController;

    [SerializeField] private Text WinCoinsText;
    [SerializeField] private Text WinText;
    [SerializeField] private Button nxtLevel;
    [SerializeField] private Button restartLevel;

    [SerializeField] private Text coinsCount;
    [SerializeField] private Text timer;
    [SerializeField] private Text racePos;

    [SerializeField] private Text[] leaderboardPositions;

    [SerializeField] private string[] levels;

    private int coins;
    void Start()
    {
        //racePos.enabled = false;
        timer.text = "Tap To Play";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartTimer()
    {
        StartCoroutine(Timer());
    }
    public void AddCoin()
    {
        coins++;
        coinsCount.text = coins + "";
    }
    public void ShowWinPanel(int mnozhitel)
    {
        WinCoinsText.text = WinCoinsText.text + coins+" coins"+" x"+mnozhitel;
        winPanelAnimator.enabled = true;
    }
    public void ShowLosePanel()
    {
        WinText.text = "You Lose!";
        WinCoinsText.text = WinCoinsText.text + coins + " coins";
        winPanelAnimator.enabled = true;
    }
    public void ShowRestartButton()
    {
        restartBtnAnimator.enabled = true;
    }
    public void NextLevel()
    {
        restartLevel.enabled = false;
        for (int i = 0; i < levels.Length; i++)
        {
            if(levels[i]== SceneManager.GetActiveScene().name&&i+1<levels.Length)
            {
                SceneManager.LoadScene(levels[i + 1]);
            }
        }
    }
    public void RestartGame()
    {
        nxtLevel.enabled = false;
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
        for(int i = 0;i<3;i++)
        {
            timer.text = i + 1 + "";
            yield return new WaitForSeconds(1);
        }
        timer.enabled = false;
        _gameController.gamestarted = true;
    }
}
