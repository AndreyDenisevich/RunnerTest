using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private float speed = 5f;
    private float sens = 2f;

    private bool stopped = true;
    private bool grounded = true;
    private float prevPosY = 0;

    //private bool mouseDrag;
    //private Vector3 prevMousePos;

    private Rigidbody _rigidbody;

    [SerializeField] private GameController _gameController;
    [SerializeField] private AudioController _audioController;

    [SerializeField] private Animator playerAnimator;

    [SerializeField] private int waterLayer;
    [SerializeField] private int groundLayer;
    
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        //prevMousePos = Input.mousePosition;
        StartCoroutine(waitForTap());
    }

    /*private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            mouseDrag = true;
        if (Input.GetMouseButtonUp(0))
            mouseDrag = false;
    }*/

    
    void FixedUpdate()
    {
        int _layer=CheckLayerUnderPlayer();

        if (transform.position.y < -2f)
            Destroy(this.gameObject);

        if (!stopped)
            transform.Translate(speed * Time.fixedDeltaTime, 0, 0);

        UpdateGroundedAndStopped(_layer);

        prevPosY = transform.position.y;

        /*if(!stopped&&mouseDrag)
        {
            transform.Rotate(0, 3*sens * (Input.mousePosition.x-prevMousePos.x) * Time.fixedDeltaTime, 0);
        }
        prevMousePos = Input.mousePosition;*/
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
        Physics.Linecast(transform.position, new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z), out hit);
        if (hit.collider != null)
          return hit.collider.gameObject.layer;
        else return -1;
    }
    private void UpdateGroundedAndStopped(int _layer)
    {
        if (!grounded && !stopped)
        {
            if (Mathf.Abs(prevPosY - transform.position.y) < 0.01f && _layer == groundLayer)
            {
                grounded = true;
                Grounded();
            }
            else if (Mathf.Abs(prevPosY - transform.position.y) < 0.01f && transform.position.y < -0.15f && _layer == waterLayer) 
            {
                stopped = true;
                Dead();
            }
        }
        if (_layer == waterLayer && _layer != groundLayer)
        {
            if (grounded)
            {
                grounded = false;
                Jump();
            }
        }
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
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.name=="Finish")
        {
            stopped = true;
            playerAnimator.SetBool("isRun", false);
            playerAnimator.SetBool("win", true);
            _audioController.ToggleFootstepsSound();
            _gameController.PlayerWin();
        }
    }
}
