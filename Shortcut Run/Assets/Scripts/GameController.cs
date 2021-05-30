using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private ParticleSystem waterSplash;
    [SerializeField] private ParticleSystem waterShlang;
    [SerializeField] private ParticleSystem[] winParticles;

    [SerializeField] private Uimanager _uiManager;
    [SerializeField] private AudioController _audioController;

    [SerializeField] private Animator playerAnimator;

    [SerializeField] private EnemyController[] enemies;
    [SerializeField] private PlayerController player;
    [SerializeField] private Transform finish;

    private bool gameStarted = false;

    private int mnozhitel = 1;

    private Collider prevCol=null;
    private float tushenieElapsedTime=0;

    private int racePosition;

    private bool mouseDrag = false;
    private Vector3 prevMousePos;

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
                if (enemies[i]!=null&&player!=null&&enemies[i].transform.position.z > player.transform.position.z)
                    racePosition++;
            }
        }
        else racePosition = 0;
        _uiManager.UpdateRacePosition(racePosition);
    }
    private void UpdateMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
            mouseDrag = true;
        else
            if (Input.GetMouseButtonUp(0))
            mouseDrag = false;
    }
    public void StartGame()
    {
        player.StartGame();
        for(int i=0;i<enemies.Length;i++)
        {
            enemies[i].StartGame();
        }
    }
    public void WaterCollected()
    {
        _uiManager.AddCoins();
    }
    public void PlayerDead()
    {
        ParticleSystem splash = Instantiate(waterSplash) as ParticleSystem;
        splash.transform.position=mainCam.transform.parent.position;
        splash.Play();
        _audioController.PlayWaterSplash();
        _uiManager.ShowRestartButton();
        cameraPivot.GetComponent<CameraFollow>().enabled = false;
    }
    public void PlayerDeadOnObstacle()
    {
        _uiManager.ShowRestartButton();
        cameraPivot.GetComponent<CameraFollow>().enabled = false;
    }
    public void PlayerWin(float platformCount)
    {
        gamestarted = false;

        cameraPivot.GetComponent<CameraFollow>().enabled = false;
        SetCameraAndPlayerFinishTransform();

        if (platformCount > 0)
        {
            _uiManager.UpdateMnozhitel(mnozhitel);
            StartCoroutine(BonusLevelController(platformCount));
        }
        else
        {
            BonusLevelEnd();
        }
    }
    public void PlayerFinished()
    {
        gamestarted = false;

        cameraPivot.GetComponent<CameraFollow>().enabled = false;
        SetCameraAndPlayerFinishTransform();

        _uiManager.ShowLosePanel(); 
    }
    private void PlayWinParticles()
    {
        for (int i = 0; i < winParticles.Length; i++)
        { winParticles[i].Play(); }
    } 
    void SetCameraAndPlayerFinishTransform()
    {
        Quaternion rot = cameraPivot.rotation;
        rot.eulerAngles = new Vector3(rot.eulerAngles.x, -90f, rot.eulerAngles.z);
        cameraPivot.rotation = rot;
        StartCoroutine(MovePlayer(new Vector3(finish.position.x, cameraPivot.position.y, finish.position.z + 2f)));
        cameraPivot.position = new Vector3(finish.position.x, cameraPivot.position.y, finish.position.z + 2f);
    }
    void BonusLevelEnd()
    {
        _uiManager.DisableMnozhitel();
        _uiManager.ShowWinPanel(mnozhitel);
        playerAnimator.SetBool("win", true);
        PlayWinParticles();
    }
    private void RayCheck()
    {
        RaycastHit hit;
        Physics.Raycast(new Vector3(waterShlang.transform.position.x, waterShlang.transform.position.y, waterShlang.transform.position.z), waterShlang.transform.forward, out hit);
        if (hit.collider == prevCol&&hit.collider!=null)
            tushenieElapsedTime+=Time.unscaledDeltaTime;
        else tushenieElapsedTime = 0;

        if(tushenieElapsedTime>=2f&&hit.collider!=null&&hit.collider.transform.GetComponentInChildren<ParticleSystem>().isPlaying)
        {
            hit.collider.transform.GetComponentInChildren<ParticleSystem>().Stop();
            mnozhitel++;
            _uiManager.UpdateMnozhitel(mnozhitel);
            tushenieElapsedTime = 0;
        }
        prevCol = hit.collider;
    }
    private float NewCamPos(float rotationX,float camPosY,float sens)
    {
        if (rotationX > -30f)
        {
            camPosY += sens*4 * 0.05f * (Input.mousePosition.y - prevMousePos.y) * Time.unscaledDeltaTime;
        }
        if (rotationX <= -30f && rotationX > -45f)
        {
            camPosY +=sens*4 * 0.065f * (Input.mousePosition.y - prevMousePos.y) * Time.unscaledDeltaTime;
        }
        if (rotationX <= -30f && rotationX > -60f)
        {
            camPosY +=sens*4 * 0.085f * (Input.mousePosition.y - prevMousePos.y) * Time.unscaledDeltaTime;
        }
        if (rotationX <= -60f)
        {
            camPosY +=sens*4 * 0.1f * (Input.mousePosition.y - prevMousePos.y) * Time.unscaledDeltaTime;
        }
        return camPosY;
    }
    private IEnumerator BonusLevelController(float platforms)
    {
        Quaternion rot = waterShlang.transform.rotation;

        waterShlang.Play();

        float leftRightRot = 30f;
        float downPosCam = cameraPivot.position.y;

        float rotationY = 0f;
        float rotationX = 0f;

        float startPlatforms = platforms;
        float endPlatforms = 0;

        float elapsedtime = 0;
        float _duration = platforms;
       
        float camPosY = cameraPivot.position.y;

        prevMousePos = Input.mousePosition;
        while (elapsedtime<_duration)
        {
            UpdateMouseDrag();
            if (mouseDrag)
            { MouseControl(ref rotationX, ref rotationY, ref camPosY); }

            TouchControl(ref rotationX, ref rotationY, ref camPosY);

            ClampfControl(ref rotationX, ref rotationY, ref camPosY, leftRightRot, downPosCam);
            
            prevMousePos = Input.mousePosition;

            player.platforms = (startPlatforms - endPlatforms) * (1 - elapsedtime/_duration);
            rot.eulerAngles = new Vector3(rotationX,rotationY, rot.eulerAngles.z); 
            waterShlang.transform.rotation = rot;

            cameraPivot.position = new Vector3(cameraPivot.position.x, camPosY, cameraPivot.position.z);
            
            elapsedtime += Time.unscaledDeltaTime;
            
            RayCheck();
            yield return null;
        }

        player.platforms = 0;

        waterShlang.Stop();
        BonusLevelEnd();
    }
    private void MouseControl(ref float rotationX,ref float rotationY,ref float camPosY)
    {
        rotationY += 4f / 3f * (Input.mousePosition.x - prevMousePos.x) * Time.unscaledDeltaTime;
        rotationX += -2f / 3f * (Input.mousePosition.y - prevMousePos.y) * Time.unscaledDeltaTime;
        camPosY = NewCamPos(rotationX, camPosY, 1f / 3f);
    }
    private void TouchControl(ref float rotationX, ref float rotationY, ref float camPosY)
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 pos = touch.deltaPosition;
                rotationX += -0.5f * pos.y * touch.deltaTime;
                rotationY += 0.25f * pos.x * touch.deltaTime;
                camPosY = NewCamPos(rotationX, camPosY, 1f / 8f);
            }
        }
    }
    private void ClampfControl(ref float rotationX, ref float rotationY, ref float camPosY,float leftRightRot,float downPosCam)
    {
        rotationY = Mathf.Clamp(rotationY, -leftRightRot, leftRightRot);
        rotationX = Mathf.Clamp(rotationX, -65f, 0f);
        camPosY = Mathf.Clamp(camPosY, downPosCam, 11f);
    }
    
    private IEnumerator MovePlayer(Vector3 endPos)
    {
        float progress = 0, elapsedtime = 0;
        float _duration = 0.1f;
        Vector3 startPos = player.transform.position;
        while (progress <= 1)
        {
            player.transform.position = Vector3.Lerp(startPos, endPos, progress);
            elapsedtime += Time.unscaledDeltaTime;
            progress = elapsedtime / _duration;
            yield return null;
        }
        player.transform.position = endPos;
    }
}
