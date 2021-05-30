using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField]private float defaultSpeed = 10f;
    [SerializeField]private float boostedSpeed = 12f;

    private float speed;
    private float sens = 2f;

    private float boundX;

    private bool stopped = true;
    private bool grounded = true;
    private bool boosted = false;
    private float prevPosY = 0;

    private bool mouseDrag;
    private Vector3 prevMousePos;

    private Rigidbody _rigidbody;

    [SerializeField] private GameController _gameController;
    [SerializeField] private AudioController _audioController;

    [SerializeField] private TrailRenderer boostTrail;

    [SerializeField] private Animator playerAnimator;

    [SerializeField] private GameObject platformPrefab;

    [SerializeField] private int waterLayer;
    [SerializeField] private int groundLayer;
    [SerializeField] private int platformLayer;

    [SerializeField] private Transform balonMid;
    [SerializeField] private Transform balonUp;
    [SerializeField] private Transform balonDown;
    private float startBalonY;
    private float StartBalonScale;

    private float platformCount = 0;
    private float platformSizeModifier;
    
    public float platforms
    {
        set { platformCount = value; }
    }
    void Start()
    {
        StartBalonScale = balonMid.localScale.y;
        startBalonY = balonMid.localPosition.y;

        speed = defaultSpeed;

        boundX = this.GetComponent<Collider>().bounds.size.x;
        _rigidbody = GetComponent<Rigidbody>();
        prevMousePos = Input.mousePosition;
    }

    private void Update()
    {
        UpdateMouseDrag();
        UpdateBalonScale();
    }
    private void UpdateMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
            mouseDrag = true;
        if (Input.GetMouseButtonUp(0))
            mouseDrag = false;
    }

    
    void FixedUpdate()
    {
        int _layer = CheckLayerUnderPlayer();

        if (transform.position.y < -2f)
            Destroy(this.gameObject);

        PlayerMovement();

        UpdateGroundedAndStopped(_layer);
        UpdateBoosted(_layer);

        prevPosY = transform.position.y;

        MouseControl();
        prevMousePos = Input.mousePosition;

        TouchControl();
    }
    private void MouseControl()
    {
        if (!stopped && mouseDrag)
        {
            transform.Rotate(0, 2 * sens * (Input.mousePosition.x - prevMousePos.x) * Time.fixedDeltaTime, 0);
        }
    }
    private void TouchControl()
    {
        if (!stopped && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 pos = touch.deltaPosition;
                transform.Rotate(0, sens * pos.x * touch.deltaTime, 0);
            }
        }
    }
    private int CheckLayerUnderPlayer()
    {
        RaycastHit hit;
        Ray ray = new Ray(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), Vector3.down);
        Physics.SphereCast(ray, (boundX-0.1f) / 2, out hit, 5f);
        if (hit.collider != null)
          return hit.collider.gameObject.layer;
        else return -1;
    }
    private void UpdateGroundedAndStopped(int _layer)
    {
        if (!grounded && !stopped)
        {
            if (Mathf.Abs(prevPosY - transform.position.y) < 0.01f && (_layer == groundLayer || _layer == platformLayer ))
            {
                grounded = true;
                Grounded();
            }
            else if (Mathf.Abs(prevPosY - transform.position.y) < 0.01f && transform.position.y < -0.15f && _layer == waterLayer )
            {
                stopped = true;
                Dead();
            }
        }
        if (_layer == waterLayer && _layer == waterLayer)
        {
            if (platformCount > 0 && grounded)
            {
                SpawnPlatform();
            }
            else
            if (grounded)
            {
                grounded = false;
                Jump();
            }
        }
    }
    private void UpdateBoosted(int _layer)
    {
        if (_layer==platformLayer)
        {
            if (!boosted)
            { EnableBoostTrail(); }
            boosted = true;
        }else if(_layer==groundLayer)
        {
            if (boosted)
            { DisableBoostTrail(); }
            boosted = false;
        }
    }
    public void UpdateBalonScale()
    {
        if (platformCount > 0)
        {
            balonMid.gameObject.SetActive(true);
            balonMid.localScale = new Vector3(balonMid.localScale.x, StartBalonScale * (platformCount + 1), balonMid.localScale.z);
            balonMid.localPosition = new Vector3(balonMid.localPosition.x, startBalonY + (platformCount + 1) * 0.03f / 200f, balonMid.localPosition.z);
            balonUp.localScale = new Vector3(balonUp.localScale.x, 200f / (platformCount + 1), balonUp.localScale.z);
            balonDown.localScale = new Vector3(balonUp.localScale.x, 200f / (platformCount + 1), balonUp.localScale.z);
        }
        else
        {
            balonMid.gameObject.SetActive(false);
        }
    }
    private void PlayerMovement()
    {
        if (!stopped)
        {
            transform.Translate(speed * Time.fixedDeltaTime, 0, 0);
        }
    }
    private void SpawnPlatform()
    {  
        GameObject obj = Instantiate(platformPrefab) as GameObject;
        obj.transform.position = new Vector3(transform.position.x, -0.17f, transform.position.z);
        obj.transform.rotation = transform.rotation;
        obj.transform.localScale = new Vector3(obj.transform.localScale.x * platformSizeModifier, obj.transform.localScale.y, obj.transform.localScale.z * platformSizeModifier);
        obj.transform.Rotate(0, 90, 0);//govnokod
        platformCount-=0.25f;
    }
    private void Dead()
    {
        this.GetComponent<Collider>().enabled = false;
        _gameController.PlayerDead();
    }
    private void Grounded()
    {
        playerAnimator.SetBool("grounded", true);
        _audioController.ToggleFootstepsSound();
    }
    private void Jump()
    {
        _rigidbody.AddForce(0, 300, 0);
        _audioController.ToggleFootstepsSound();
        playerAnimator.SetBool("grounded", false);
    }
    private void EnableBoostTrail()
    {
        StopAllCoroutines();
        StartCoroutine(BoostSpeed(speed, boostedSpeed));
        playerAnimator.SetBool("boosted", true);
        boostTrail.emitting = true;
    } 
    private void DisableBoostTrail()
    {
        StopAllCoroutines();
        StartCoroutine(BoostSpeed(speed, defaultSpeed));
        playerAnimator.SetBool("boosted", false);
        boostTrail.emitting = false;
    }
    public void StartGame()
    {
        StartCoroutine(start());
        platformSizeModifier = 1 + (PlayerPrefs.GetInt("WaterLevel",1) - 1) * 0.1f;
    }
    private IEnumerator start()
    {
        yield return new WaitForSeconds(3);
        stopped = false;
        playerAnimator.SetBool("isRun", true);
        _audioController.ToggleFootstepsSound();
    }
    private IEnumerator BoostSpeed(float startSpeed,float endSpeed)
    {
        float progress = 0;
        float elapsedTime = 0f;
        float duration = 1f;
        while(progress<=1)
        {
            speed = startSpeed - (startSpeed - endSpeed)*progress;
            elapsedTime += Time.unscaledDeltaTime;
            progress = elapsedTime / duration;
            yield return null;
        }
        speed = endSpeed;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag=="WaterCube")
        {
            platformCount+=0.5f;
            _gameController.WaterCollected();
            Destroy(other.gameObject);
        }
        if(speed-defaultSpeed>0.5f&&other.gameObject.layer==9&&grounded)//ne gamedesigner
        {
            platformCount+=other.transform.parent.GetComponent<EnemyController>().platforms;

            other.transform.parent.GetComponent<EnemyController>().EnemyKicked(transform.right);
            //Destroy(other.transform.parent.gameObject);
        }
        if(other.tag=="ConeObstacle")
        {
            _gameController.PlayerDeadOnObstacle();
            Destroy(this.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.name=="Finish"&&!stopped)
        {
            stopped = true;
            playerAnimator.SetBool("isRun", false);
            
            Quaternion rot = transform.rotation;
            rot.eulerAngles = new Vector3(rot.eulerAngles.x, -90f, rot.eulerAngles.z);
            transform.rotation = rot;
            _audioController.ToggleFootstepsSound();

            if (other.GetComponent<FinishController>().pos == 1)
                _gameController.PlayerWin(platformCount);
            else _gameController.PlayerFinished();

            other.GetComponent<FinishController>().Finished(this.name);
        }
    }
}
