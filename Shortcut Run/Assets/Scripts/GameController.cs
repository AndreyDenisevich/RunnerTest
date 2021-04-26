using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private ParticleSystem waterSplash;
    [SerializeField] private ParticleSystem waterShlang;
    [SerializeField] private ParticleSystem smoke;
    [SerializeField] private ParticleSystem[] winParticles;

    [SerializeField] private Uimanager _uiManager;
    [SerializeField] private AudioController _audioController;

    [SerializeField] private Transform balonMid;
    [SerializeField] private Transform balonUp;
    [SerializeField] private Transform balonDown;

    [SerializeField] private GameObject tower;

    [SerializeField] private Animator playerAnimator;

    [SerializeField] private EnemyController[] enemies;
    [SerializeField] private Transform player;

    private int mnozhitel = 1;
    private Collider prevCol=null;
    private int racePosition;

    private bool mouseDrag = false;
    private bool gameStarted=false;

    private Camera mainCam;
    [SerializeField] private Transform cameraPivot;

    public bool gamestarted
    {
        set { gameStarted = value; }
    }
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStarted)
        {
            racePosition = 1;
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].transform.position.z > player.position.z)
                    racePosition++;
            }
        }
        else racePosition = 0;
        _uiManager.UpdateRacePosition(racePosition);
    }
    public void StartGame()
    {
        _uiManager.StartTimer();
        for(int i=0;i<enemies.Length;i++)
        {
            enemies[i].StartGame();
        }
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
        cameraPivot.GetComponent<CameraFollow>().enabled = false;
    }
    public void PlayerWin(float platformCount)
    {
        gamestarted = false;

        cameraPivot.GetComponent<CameraFollow>().enabled = false;
        Quaternion rot = cameraPivot.rotation;
        rot.eulerAngles = new Vector3(rot.eulerAngles.x, -90f, rot.eulerAngles.z);
        cameraPivot.rotation = rot;
        cameraPivot.position = new Vector3(17.5f, transform.position.y, 71f);

        GameObject obj = Instantiate(tower) as GameObject;
        obj.transform.position = new Vector3(1f, transform.position.y, 9f);

        if (platformCount > 0)
        { StartCoroutine(WaterShlangRotation(platformCount)); }
        else
        { _uiManager.ShowWinPanel(mnozhitel); PlayWinParticles(); playerAnimator.SetBool("win", true); }
    }
    public void PlayerFinished()
    {
        gamestarted = false;

        cameraPivot.GetComponent<CameraFollow>().enabled = false;
        Quaternion rot = cameraPivot.rotation;
        rot.eulerAngles = new Vector3(rot.eulerAngles.x, -90f, rot.eulerAngles.z);
        cameraPivot.rotation = rot;
        cameraPivot.position = new Vector3(17.5f, transform.position.y, 71f);
        _uiManager.ShowWinPanel(mnozhitel); 
    }
    private void PlayWinParticles()
    {
        for (int i = 0; i < winParticles.Length; i++)
        { winParticles[i].Play(); }
    }
    public void UpdateBalonScale(float platformCount)
    {
        if (platformCount == 0)
        { balonMid.gameObject.SetActive(false); }
        else
        if (platformCount == 1)
        { balonMid.gameObject.SetActive(true); }
        else
        { 
            balonMid.localScale = new Vector3(balonMid.localScale.x, 0.05f * platformCount, balonMid.localScale.z);
            balonMid.position = new Vector3(balonMid.position.x, 1 + platformCount * 0.03f, balonMid.position.z);
            balonUp.localScale = new Vector3(balonUp.localScale.x, 200f/platformCount, balonUp.localScale.z);
            balonDown.localScale = new Vector3(balonUp.localScale.x, 200f/platformCount, balonUp.localScale.z);
        }
    }
    public void SmokeUnderPlayer()
    {
        smoke.Play();
    }
    private IEnumerator WaterShlangRotation(float platforms)
    {
        waterShlang.Play();
        RayCheck();

        float startPlatforms = platforms;
        float endPlatforms = 0;

        float _platform;
        if (platforms <= 14)
            _platform = platforms;
        else _platform = 14;

        float progress = 0, elapsedtime = 0;
        float _duration = _platform/2;

        Quaternion startRot = waterShlang.transform.rotation;
        Quaternion endRot = startRot;
        endRot.eulerAngles = new Vector3(-5 * _platform,endRot.eulerAngles.y, endRot.eulerAngles.z);

        Vector3 camStartPos = cameraPivot.position;
        Vector3 camEndPos = camStartPos;
        for(int i =0;i<_platform;i++)
        {
            camEndPos.y += i * 0.05f;
        }
        
        while (progress<=1)
        {
            UpdateBalonScale((startPlatforms - endPlatforms) * (1 - progress));
            waterShlang.transform.rotation = Quaternion.Lerp(startRot, endRot, progress);
            cameraPivot.transform.position = Vector3.Lerp(camStartPos, camEndPos, progress);
            elapsedtime += Time.unscaledDeltaTime;
            progress = elapsedtime / _duration;
            RayCheck();
            yield return null;
        }

        UpdateBalonScale(0);

        waterShlang.transform.rotation = endRot;
        waterShlang.Stop();

        _uiManager.ShowWinPanel(mnozhitel);
        playerAnimator.SetBool("win", true);
        PlayWinParticles();
    }

    private void RayCheck()
    {
        RaycastHit hit;
        Physics.Raycast(new Vector3(waterShlang.transform.position.x, 0, waterShlang.transform.position.z), waterShlang.transform.forward, out hit);
        if (hit.collider!=null&&hit.collider!=prevCol)
        {
            mnozhitel++;
            prevCol = hit.collider;
            hit.transform.GetComponentInChildren<ParticleSystem>().Stop();
        }
    }

    private void UpdateMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
            mouseDrag = true;
        else
            if (Input.GetMouseButtonUp(0))
            mouseDrag = false;
    }
}
