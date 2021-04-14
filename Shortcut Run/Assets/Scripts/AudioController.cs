using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private AudioSource footsteps;
    [SerializeField] private AudioSource waterSplash;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlayWaterSplash()
    {
        waterSplash.Play();
    }
    public void ToggleFootstepsSound()
    {
        if (footsteps.isPlaying)
            footsteps.Stop();
        else footsteps.Play();
    }
}
