using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Transform playerTransform;

    private float deltaPosX;
    private float deltaPosZ;
    private float deltaRotationY;
    private Quaternion rot;
    void Start()
    {
        deltaPosX=transform.position.x - playerTransform.position.x;
        deltaPosZ=transform.position.z - playerTransform.position.z;
        deltaRotationY = transform.rotation.eulerAngles.y - playerTransform.rotation.eulerAngles.y;
        rot = transform.rotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = new Vector3(playerTransform.position.x + deltaPosX, transform.position.y, playerTransform.position.z + deltaPosZ);
        rot.eulerAngles = new Vector3(rot.eulerAngles.x, playerTransform.rotation.eulerAngles.y + deltaRotationY, rot.eulerAngles.z);
        transform.rotation = rot;
    }
}
