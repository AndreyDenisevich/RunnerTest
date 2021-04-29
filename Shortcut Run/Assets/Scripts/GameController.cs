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
    [SerializeField] private Transform finish;

    private int mnozhitel = 1;

    private Collider prevCol=null;
    private int kol=31;

    private int racePosition;

    private bool mouseDrag = false;
    private Vector3 prevMousePos;

    private float StartBalonScale;

    private bool gameStarted=false;

    private Camera mainCam;
    [SerializeField] private Transform cameraPivot;

    [SerializeField] LayerMask fire;

    public bool gamestarted
    {
        set { gameStarted = value; }
    }
    void Start()
    {
        StartBalonScale = balonMid.localScale.y;
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
                if (enemies[i]!=null&&enemies[i].transform.position.z > player.position.z)
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
        //player.position = new Vector3(finish.position.x, cameraPivot.position.y, finish.position.z + 3f);
        StartCoroutine(MovePlayer(new Vector3(finish.position.x, cameraPivot.position.y, finish.position.z + 3f)));
        cameraPivot.rotation = rot;
        cameraPivot.position = new Vector3(finish.position.x, cameraPivot.position.y, finish.position.z+2f);

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
        //player.position = new Vector3(finish.position.x, cameraPivot.position.y, finish.position.z + 2f);
        StartCoroutine(MovePlayer(new Vector3(finish.position.x, cameraPivot.position.y, finish.position.z + 2f)));
        cameraPivot.position = new Vector3(finish.position.x, cameraPivot.position.y, finish.position.z+2f);
        _uiManager.ShowLosePanel(); 
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
            balonMid.localScale = new Vector3(balonMid.localScale.x, StartBalonScale * platformCount, balonMid.localScale.z);
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
        Quaternion rot = waterShlang.transform.rotation;

        waterShlang.Play();

        float leftRightRot = 40f;
        float downPosCam = cameraPivot.position.y;

        float rotationY = 0f;
        float rotationX = 0f;

        float startPlatforms = platforms;
        float endPlatforms = 0;

        float _platform;
        if (platforms <= 13)
            _platform = platforms;
        else _platform = 13;

        float progress = 0, elapsedtime = 0;
        float _duration = platforms/1.5f;

        Quaternion startRot = waterShlang.transform.rotation;
        Quaternion endRot = startRot;
        endRot.eulerAngles = new Vector3(-5 * _platform, endRot.eulerAngles.y, endRot.eulerAngles.z);

        Vector3 camStartPos = cameraPivot.position;
        Vector3 camEndPos = camStartPos;
        float camPosY = cameraPivot.position.y;

        prevMousePos = Input.mousePosition;
        while (progress<=1)
        {
            UpdateMouseDrag();
            UpdateBalonScale((startPlatforms - endPlatforms) * (1 - progress));

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved)
                {
                    Vector2 pos = touch.deltaPosition;
                    rotationX += -0.5f * pos.y * touch.deltaTime;
                    rotationY += 0.25f * pos.x * touch.deltaTime;
                    if(rotationX>-30f)
                    {
                        camPosY += 0.05f * pos.y * touch.deltaTime;
                    }
                    if(rotationX<=-30f&& rotationX>-45f)
                    {
                        camPosY += 0.065f * pos.y * touch.deltaTime;
                    }
                    if (rotationX <= -30f && rotationX > -60f)
                    {
                        camPosY += 0.085f * pos.y * touch.deltaTime;
                    }
                    if (rotationX <= -60f)
                    {
                        camPosY += 0.1f * pos.y * touch.deltaTime;
                    }
                    rotationY = Mathf.Clamp(rotationY, -leftRightRot, leftRightRot);
                    rotationX = Mathf.Clamp(rotationX, -65f, 0f);
                    camPosY = Mathf.Clamp(camPosY, downPosCam, 11f);
                }
            }
            if (mouseDrag)
            {
                rotationY += 4f * (Input.mousePosition.x - prevMousePos.x) * Time.unscaledDeltaTime;
                rotationX += -2f * (Input.mousePosition.y - prevMousePos.y) * Time.unscaledDeltaTime;
                if (rotationX > -30f)
                {
                    camPosY += 4*0.05f * (Input.mousePosition.y - prevMousePos.y) * Time.unscaledDeltaTime;
                }
                if (rotationX <= -30f && rotationX > -45f)
                {
                    camPosY += 4*0.065f * (Input.mousePosition.y - prevMousePos.y) * Time.unscaledDeltaTime;
                }
                if (rotationX <= -30f && rotationX > -60f)
                {
                    camPosY += 4*0.085f * (Input.mousePosition.y - prevMousePos.y) * Time.unscaledDeltaTime;
                }
                if (rotationX <= -60f)
                {
                    camPosY += 4*0.1f * (Input.mousePosition.y - prevMousePos.y) * Time.unscaledDeltaTime;
                }
                camPosY = Mathf.Clamp(camPosY, downPosCam, 11f);
                rotationY = Mathf.Clamp(rotationY, -leftRightRot, leftRightRot);
                rotationX = Mathf.Clamp(rotationX, -65f, 0f);
            }

            prevMousePos = Input.mousePosition;
            //rot = Quaternion.Lerp(startRot, endRot, progress);
            rot.eulerAngles = new Vector3(rotationX,rotationY, rot.eulerAngles.z); 
            waterShlang.transform.rotation = rot;

            cameraPivot.position = new Vector3(cameraPivot.position.x, camPosY, cameraPivot.position.z);
            
            elapsedtime += Time.unscaledDeltaTime;
            progress = elapsedtime / _duration;
            RayCheck();
            yield return null;
        }

        UpdateBalonScale(0);

        for(int i=0;i<31;i++)
        {
            RayCheck();
            yield return null;
        }

       // waterShlang.transform.rotation = endRot;
        waterShlang.Stop();
        _uiManager.ShowWinPanel(mnozhitel);
        playerAnimator.SetBool("win", true);
        PlayWinParticles();
    }

    private void RayCheck()
    {
        RaycastHit hit;
        Physics.Raycast(new Vector3(waterShlang.transform.position.x, waterShlang.transform.position.y, waterShlang.transform.position.z), waterShlang.transform.forward, out hit);
        if (hit.collider != null && hit.collider != prevCol)
        {
            mnozhitel++;
            prevCol = hit.collider;
            if (kol >= 30)
            { hit.collider.transform.GetComponentInChildren<ParticleSystem>().Stop(); }
            kol = 0;
        }
        else kol++;
    }

    private void UpdateMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
            mouseDrag = true;
        else
            if (Input.GetMouseButtonUp(0))
            mouseDrag = false;
    }
    private IEnumerator MovePlayer(Vector3 endPos)
    {
        float progress = 0, elapsedtime = 0;
        float _duration = 0.1f;
        Vector3 startPos = player.position;
        while (progress <= 1)
        {
            player.position = Vector3.Lerp(startPos, endPos, progress);
            elapsedtime += Time.unscaledDeltaTime;
            progress = elapsedtime / _duration;
            yield return null;
        }
        player.position = endPos;
    }
}
