using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class EnemyController : MonoBehaviour
{
    [SerializeField]private float speed = 5f;
    private float _platformCount = 0;
    private float prevPosY = 0;

    private float boundX;
    private float boundZ;
    private float StartBalonScale;

    private bool stopped = true;
    private bool translate = false;
    private bool grounded = true;

    private Rigidbody _rigidbody;

    private int waterLayer = 7;
    private int groundLayer = 6;

    [SerializeField] private Transform balonMid;
    [SerializeField] private Transform balonUp;
    [SerializeField] private Transform balonDown;

    [SerializeField] private Animator enemyAnimator;

    [SerializeField] private SplineFollower[] splines;

    private int splineNumber = 0;

    [SerializeField] private GameObject platformPrefab;
    // Start is called before the first frame update
    void Start()
    {
        StartBalonScale = balonMid.localScale.y;
        boundX = this.GetComponent<Collider>().bounds.size.x;
        boundZ = this.GetComponent<Collider>().bounds.size.z;
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBalonScale(_platformCount);

    }
    void FixedUpdate()
    {
        int _leftLayer = CheckLeftLayerUnderEnemy();
        int _rightLayer = CheckRightLayerUnderEnemy();

        if (transform.position.y < -2f)
            Destroy(this.gameObject);
        if (translate)
            transform.Translate(0, 0, 10f*Time.fixedDeltaTime);
        UpdateGroundedAndStopped(_leftLayer, _rightLayer);

        prevPosY = transform.position.y;
    }
    private void UpdateGroundedAndStopped(int _leftLayer, int _rightLayer)
    {
        if (!grounded && !stopped)
        {
            if (Mathf.Abs(prevPosY - transform.position.y) < 0.01f && (_leftLayer == groundLayer || _rightLayer == groundLayer))
            {
                grounded = true;
                Grounded();
            }
            else if (Mathf.Abs(prevPosY - transform.position.y) < 0.01f && transform.position.y < -0.15f && _rightLayer == waterLayer && _rightLayer == waterLayer)
            {
                stopped = true;
                Dead();
            }
        }
        if (_leftLayer == waterLayer && _rightLayer == waterLayer)
        {
            if (_platformCount > 0 && grounded)
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
    private void Grounded()
    {
        enemyAnimator.SetBool("grounded", true);
    }
    private void Dead()
    {
        enemyAnimator.SetBool("grounded", true);
        enemyAnimator.SetBool("isRun", false);
        enemyAnimator.SetBool("dead", true);
        this.GetComponent<Collider>().enabled = false;
    }
    private void SpawnPlatform()
    {
        GameObject obj = Instantiate(platformPrefab) as GameObject;
        obj.transform.position = new Vector3(transform.position.x, -0.17f, transform.position.z);
        obj.transform.rotation = transform.rotation;
        obj.transform.Rotate(0, 90, 0);//govnokod
        _platformCount -= 0.5f;
        //UpdateBalonScale(_platformCount);
    }
    private void Jump()
    {
        _rigidbody.useGravity = true;
        splines[0].follow = false;
        translate = true;
        _rigidbody.AddForce(0, 300, 0);
        enemyAnimator.SetBool("grounded", false);
    }
    private int CheckLeftLayerUnderEnemy()
    {
        RaycastHit hitLeft;
        Physics.Linecast(new Vector3(transform.position.x,transform.position.y+1f,transform.position.z), new Vector3(transform.position.x + boundX / 2, transform.position.y - 1f, transform.position.z + boundZ / 2), out hitLeft);
        if (hitLeft.collider != null)
            return hitLeft.collider.gameObject.layer;
        else return -1;
    }
    private int CheckRightLayerUnderEnemy()
    {
        RaycastHit hitRight;
        Physics.Linecast(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), new Vector3(transform.position.x - boundX / 2, transform.position.y - 1f, transform.position.z - boundZ / 2), out hitRight);
        if (hitRight.collider != null)
            return hitRight.collider.gameObject.layer;
        else return -1;
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
            balonUp.localScale = new Vector3(balonUp.localScale.x, 200f / platformCount, balonUp.localScale.z);
            balonDown.localScale = new Vector3(balonUp.localScale.x, 200f / platformCount, balonUp.localScale.z);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "WaterCube")
        {
            _platformCount++;
            //UpdateBalonScale(_platformCount);
            Destroy(other.gameObject);
        }
    }
    public void StartGame()
    {
        StartCoroutine(start());
    }
    private IEnumerator start()
    {
        yield return new WaitForSeconds(3);
        enemyAnimator.SetBool("isRun", true);
        stopped = false;
        splines[0].follow = true;
        splines[0].followSpeed = speed;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Finish"&&!stopped)
        {
            stopped = true;
            enemyAnimator.SetBool("isRun", false);
            other.GetComponent<FinishController>().Finished(this.name);
        }
    }
}
