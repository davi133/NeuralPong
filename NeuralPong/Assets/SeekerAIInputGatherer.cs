using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekerAIInputGatherer : inputGatherer
{
    [SerializeField] GameManager _gm;
    [SerializeField] int frameDelay = 0;
    private float _moveInput = 0;
    private float _rotInput = 0;

    public override float movementInput()
    {
       
        return _moveInput;
    }

    public override float rotationtInput()
    {
        return _rotInput;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (frameDelay ==0)
        {
            pastFrames = new float[1];
        }
        else
        {
            pastFrames = new float[frameDelay];
        }
        
    }

    // Update is called once per frame
    float[] pastFrames;
    int framePointer=0;
    void FixedUpdate()
    {
        int currentFrame = frameDelay !=0?(framePointer + 1) % frameDelay:0;
        _moveInput = pastFrames[currentFrame] - transform.position.y; 
        _moveInput = Mathf.Clamp(_moveInput, -1, 1);
        //Debug.Log($"{framePointer} {currentFrame}");
        framePointer = currentFrame;
        pastFrames[framePointer] = _gm.ball.transform.position.y;
       
        
    }

}
