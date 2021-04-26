using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishController : MonoBehaviour
{
    // Start is called before the first frame update
    private int _position = 1;
    [SerializeField] Uimanager _uiManager;
    public int pos
    {
        get { return _position; }
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Finished(string runnerName)
    {
        _uiManager.UpdateLeaderBoard(_position, runnerName);
        _position++;
    }
}
