using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private ParticleSystem waterSplash;
    [SerializeField] private ParticleSystem[] winParticles;

    [SerializeField] private Uimanager _uiManager;
    [SerializeField] private AudioController _audioController;

    private Camera mainCam;
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartGame()
    {
        _uiManager.StartTimer();
    }
    public void CoinCollected()
    {
        _uiManager.AddCoin();
    }
    public void PlayerDead()
    {
        ParticleSystem splash = Instantiate(waterSplash) as ParticleSystem;
        splash.transform.position=mainCam.transform.parent.position;
        splash.Play();
        _audioController.PlayWaterSplash();
        Time.timeScale = 0.2f;
        _uiManager.ShowRestartButton();
    }
    public void PlayerWin()
    {
        for(int i =0;i<winParticles.Length;i++)
        { winParticles[i].Play(); }
        _uiManager.ShowWinPanel();
        StartCoroutine(camMoveAndRotate(mainCam.transform.position, mainCam.transform.rotation));
    }
    private IEnumerator camMoveAndRotate(Vector3 startPos, Quaternion startRotation)
    {
        float progress = 0;
        float elapsedTime = 0;
        float duration = 1f;
        Vector3 endPos = new Vector3(startPos.x, startPos.y, startPos.z + 20);
        Quaternion endRotation = startRotation;
        endRotation.eulerAngles = new Vector3(endRotation.eulerAngles.x, 180, endRotation.eulerAngles.z);
        while (progress <= 1)
        {
            mainCam.transform.position = Vector3.Lerp(startPos, endPos, progress);
            mainCam.transform.rotation = Quaternion.Lerp(startRotation, endRotation, progress);
            elapsedTime += Time.unscaledDeltaTime;
            progress = elapsedTime / duration;
            yield return null;
        }
        mainCam.transform.position = endPos;
        mainCam.transform.rotation = endRotation;
    }
}
