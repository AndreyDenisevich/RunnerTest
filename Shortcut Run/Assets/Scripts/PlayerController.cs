using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField]private float speed = 10f;
    private float sens = 2f;
    private float boundX;
    private float boundZ;

    private bool stopped = true;
    private bool grounded = true;
    private float prevPosY = 0;

    private bool mouseDrag;
    private Vector3 prevMousePos;

    private Rigidbody _rigidbody;

    [SerializeField] private GameController _gameController;
    [SerializeField] private AudioController _audioController;

    [SerializeField] private Animator playerAnimator;

    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private Transform balon;

    [SerializeField] private int waterLayer;
    [SerializeField] private int groundLayer;

    private float platformCount = 0;
    
    void Start()
    {
        boundX = this.GetComponent<Collider>().bounds.size.x;
        boundZ = this.GetComponent<Collider>().bounds.size.z;
        _rigidbody = GetComponent<Rigidbody>();
        prevMousePos = Input.mousePosition;
        StartCoroutine(waitForTap());
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            mouseDrag = true;
        if (Input.GetMouseButtonUp(0))
            mouseDrag = false;
        _gameController.UpdateBalonScale(platformCount);
    }

    
    void FixedUpdate()
    {
        int _leftLayer=CheckLeftLayerUnderPlayer();
        int _rightLayer=CheckRightLayerUnderPlayer();

        if (transform.position.y < -2f)
            Destroy(this.gameObject);

        if (!stopped)
            transform.Translate(speed * Time.fixedDeltaTime, 0, 0);

        UpdateGroundedAndStopped(_leftLayer,_rightLayer);

        prevPosY = transform.position.y;

      if(!stopped&&mouseDrag)
        {
            transform.Rotate(0, 6 * sens * (Input.mousePosition.x - prevMousePos.x) * Time.fixedDeltaTime, 0);
        }
        prevMousePos = Input.mousePosition;
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
    private int CheckLeftLayerUnderPlayer()
    {
        RaycastHit hitLeft;
        Physics.Linecast(transform.position, new Vector3(transform.position.x+boundX/2, transform.position.y - 1f, transform.position.z+boundZ/2), out hitLeft);
        if (hitLeft.collider != null)
          return hitLeft.collider.gameObject.layer;
        else return -1;
    }
    private int CheckRightLayerUnderPlayer()
    {
        RaycastHit hitRight;
        Physics.Linecast(transform.position, new Vector3(transform.position.x - boundX / 2, transform.position.y - 1f, transform.position.z - boundZ/2), out hitRight);
        if (hitRight.collider != null)
            return hitRight.collider.gameObject.layer;
        else return -1;
    }
    private void UpdateGroundedAndStopped(int _leftLayer,int _rightLayer)
    {
        if (!grounded && !stopped)
        {
            if (Mathf.Abs(prevPosY - transform.position.y) < 0.01f && (_leftLayer == groundLayer||_rightLayer==groundLayer))
            {
                grounded = true;
                Grounded();
            }
            else if (Mathf.Abs(prevPosY - transform.position.y) < 0.01f && transform.position.y < -0.15f && _rightLayer == waterLayer  && _rightLayer == waterLayer ) 
            {
                stopped = true;
                Dead();
            }
        }
        if (_leftLayer == waterLayer&& _rightLayer == waterLayer)
        {
            if (platformCount > 0&&grounded)
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

    private void SpawnPlatform()
    {
        GameObject obj = Instantiate(platformPrefab) as GameObject;
        obj.transform.position = new Vector3(transform.position.x, -0.17f, transform.position.z);
        obj.transform.rotation = transform.rotation;
        obj.transform.Rotate(0, 90, 0);//govnokod
        platformCount-=0.5f;
        
        //_gameController.UpdateBalonScale(platformCount);
        _gameController.SmokeUnderPlayer();
    }
    private void Dead()
    {
        playerAnimator.SetBool("grounded", true);
        playerAnimator.SetBool("isRun", false);
        playerAnimator.SetBool("dead", true);
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
    private IEnumerator waitForTap()
    {
        while (Input.touchCount < 1&&!Input.GetMouseButtonDown(0))
        {
            yield return null;
        }
        _gameController.StartGame();
        yield return new WaitForSeconds(3);
        stopped = false;
        playerAnimator.SetBool("isRun", true);
        _audioController.ToggleFootstepsSound();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag=="coin")
        {
            _gameController.CoinCollected();
            Destroy(other.gameObject);
        }
        if(other.tag=="WaterCube")
        {
            platformCount++;
            //_gameController.UpdateBalonScale(platformCount);
            Destroy(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.name=="Finish"&&!stopped)
        {
            stopped = true;
            playerAnimator.SetBool("isRun", false);
            
            //transform.position = new Vector3(7.5f, transform.position.y,102f);
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
