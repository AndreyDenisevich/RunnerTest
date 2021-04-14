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

    [SerializeField] private Text WinCoinsText;
    [SerializeField] private Text coinsCount;
    [SerializeField] private Text timer;

    private int coins;
    void Start()
    {
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
    public void ShowWinPanel()
    {
        WinCoinsText.text = WinCoinsText.text + coins+" coins";
        winPanelAnimator.enabled = true;
    }
    public void ShowRestartButton()
    {
        restartBtnAnimator.enabled = true;
    }
    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("SampleScene");
    }

    private IEnumerator Timer()
    {
        for(int i = 0;i<3;i++)
        {
            timer.text = i + 1 + "";
            yield return new WaitForSeconds(1);
        }
        timer.enabled = false;
    }
}
