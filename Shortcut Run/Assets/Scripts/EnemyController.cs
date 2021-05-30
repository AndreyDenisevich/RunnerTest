using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float defaultSpeed = 8f;
    [SerializeField] private float boostedSpeed = 10f;
    private float speed;

    private float _platformCount = 0;
    private float prevPosY = 0;

    private float boundX;
    private float boundZ;
    private float StartBalonScale;
    private float startBalonY;

    private bool stopped = true;
    private bool grounded = true;
    private bool boosted = false;

    private Rigidbody _rigidbody;

    private int waterLayer = 7;
    private int groundLayer = 6;
    private int platformLayer = 8;

    [SerializeField] private Transform balonMid;
    [SerializeField] private Transform balonUp;
    [SerializeField] private Transform balonDown;

    [SerializeField] private Animator enemyAnimator;

    [SerializeField] private TrailRenderer boostTrail;

    [SerializeField] private SplineComputer spline;

    private int splinePointNumber;

    [SerializeField] private GameObject platformPrefab;
    public float platforms
    {
        get
        {
            return _platformCount;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        splinePointNumber = 0;
        StartBalonScale = balonMid.localScale.y;
        startBalonY = balonMid.localPosition.y;

        speed = defaultSpeed;

        boundX = this.GetComponent<Collider>().bounds.size.x;
        boundZ = this.GetComponent<Collider>().bounds.size.z;
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBalonScale(_platformCount);

        Vector3 vector = (spline.GetPointPosition(splinePointNumber)-transform.position).normalized;
        vector.y = 0;
        transform.forward = vector;
    }
    void FixedUpdate()
    {
        int _layer =CheckLayerUnderEnemy();
        

        if (transform.position.y < -2f)
            Destroy(this.gameObject);

        EnemyMovement();
        if (!stopped)
        {
            UpdateSplinePoint();
            UpdateGroundedAndStopped(_layer);
            UpdateBoosted(_layer);
        }

        prevPosY = transform.position.y;
    }
    public void EnemyKicked(Vector3 forward)
    {
        forward.y = 0.05f;
        _rigidbody.AddForce(forward * 2000f);
        boostTrail.emitting = true;
        stopped = true;
        this.GetComponent<Collider>().enabled = false;
        enemyAnimator.SetBool("grounded", false);
    }
    private int CheckLayerUnderEnemy()
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
            if (Mathf.Abs(prevPosY - transform.position.y) < 0.01f && (_layer == groundLayer || _layer == platformLayer))
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
        if (_layer == waterLayer && _layer == waterLayer)
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
    private void UpdateBoosted(int _layer)
    {
        if (_layer == platformLayer)
        {
            if (!boosted)
            { EnableBoostTrail(); }
            boosted = true;
        }
        else if (_layer == groundLayer)
        {
            if (boosted)
            { DisableBoostTrail(); }
            boosted = false;
        }
    }
    private void EnemyMovement()
    {
        if (!stopped)
        {
            transform.Translate(0,0,speed * Time.fixedDeltaTime);
        }
    }
    private void UpdateSplinePoint()
    {
        Vector3 point = spline.GetPointPosition(splinePointNumber);
        if (!stopped && Mathf.Abs(point.x - transform.position.x + point.z - transform.position.z) < 0.2f)
            splinePointNumber++;
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
        _platformCount -= 0.25f;
    }
    private void Jump()
    {
        _rigidbody.AddForce(0, 300, 0);
        enemyAnimator.SetBool("grounded", false);
    }
    private void EnableBoostTrail()
    {
        StopAllCoroutines();
        StartCoroutine(BoostSpeed(speed, boostedSpeed));
        enemyAnimator.SetBool("boosted", true);
        boostTrail.emitting = true;
    }
    private void DisableBoostTrail()
    {
        StopAllCoroutines();
        StartCoroutine(BoostSpeed(speed, defaultSpeed));
        enemyAnimator.SetBool("boosted", false);
        boostTrail.emitting = false;
    }
    public void UpdateBalonScale(float platformCount)
    {
        if (platformCount > 0)
        {
            balonMid.gameObject.SetActive(true);
            balonMid.localScale = new Vector3(balonMid.localScale.x, StartBalonScale * (platformCount+1), balonMid.localScale.z);
            balonMid.localPosition = new Vector3(balonMid.localPosition.x, startBalonY + (platformCount+1)* 0.03f / 200f, balonMid.localPosition.z);
            balonUp.localScale = new Vector3(balonUp.localScale.x, 200f / (platformCount+1), balonUp.localScale.z);
            balonDown.localScale = new Vector3(balonUp.localScale.x, 200f / (platformCount+1), balonUp.localScale.z);
        }
        else
        { 
            balonMid.gameObject.SetActive(false); 
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "WaterCube")
        {
            _platformCount+=0.5f;
            Destroy(other.gameObject);
        }
    }
    public void StartGame()
    {
        StartCoroutine(start());
    }
    private IEnumerator BoostSpeed(float startSpeed, float endSpeed)
    {
        float progress = 0;
        float elapsedTime = 0f;
        float duration = 1f;
        while (progress <= 1)
        {
            speed = startSpeed - (startSpeed - endSpeed) * progress;
            elapsedTime += Time.unscaledDeltaTime;
            progress = elapsedTime / duration;
            yield return null;
        }
        speed = endSpeed;
    }
    private IEnumerator start()
    {
        yield return new WaitForSeconds(3);
        enemyAnimator.SetBool("isRun", true);
        stopped = false;
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
