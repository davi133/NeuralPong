using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class AgentInputGatherer : inputGatherer
{
    [SerializeField] GameManager _gm;
    public NeuralNetwork brain;
    [SerializeField] private bool considerOpponent = false;
    [SerializeField] private bool noise = false;
    [SerializeField] private float triggerBias = 0.5f;
    [field: SerializeField] public bool mutate { get; set; } = false;
    [field: SerializeField] public string file { get; set; } = "";
    [field: SerializeField] public string fileToSave { get; set; } = "";

    #region AI inputs
    //Vector2 adversaryPos;   //
    //float adversaryRot;     //

    Vector2 ownPos;         //+2
    float ownRot;           //+1

    Vector2 ballPos;        //+2
    Vector2 ballVelocity;   //+2
    float ballRot;          //+1
                            //total:8 inputs 

    #endregion

    #region AI outputs
    private float _moveInput = 0;
    private float _rotInput = 0;
    #endregion

    public void Start()
    {

        reStart();
    }

    private void reStart()
    {
        if (file != "")
        {
            brain = NeuralNetwork.fromFile(file);
            //Debug.Log($"{gameObject.name} is loading {file}");
            if (mutate)
            {
                brain = brain.mutate();
                //Debug.Log($"{gameObject.name} is mutating");
            }
        }
        if (brain==null)
        {
            //Debug.Log("nova rede");
            brain = new NeuralNetwork(new int[] { 6, 8, 8, 8, 8, 2 });
        }

        if (fileToSave != "")
        {
            brain.saveToFile(fileToSave);
        }
    }

    private void FixedUpdate()
    {

        if (_gm.gameState == GameManager.GameState.Ended)
        {
            _rotInput = 0;
            _moveInput = 0;
            return;
        }
        float[] inputs;

        var ar = _gm.arena.position;
        bool amIOnRight = _gm.arena.position.x < transform.position.x;
        Transform adversary = amIOnRight ? _gm.playerLeft : _gm.playerRight;


        #region considering opponent gathering
        if (considerOpponent)
        {
            inputs = new float[9];
            //inputs[0] = amIOnRight ? adversary.localPosition.x : -adversary.localPosition.x;
            inputs[0] = amIOnRight ? adversary.localPosition.y : -adversary.localPosition.y;
            inputs[1] = (adversary.rotation.eulerAngles.z % 180) * Mathf.Deg2Rad;

            //inputs[3] = amIOnRight ? transform.localPosition.x : -transform.localPosition.x;
            inputs[2] = amIOnRight ? transform.localPosition.y : -transform.localPosition.y;
            inputs[3] = (transform.rotation.eulerAngles.z % 180) * Mathf.Deg2Rad;

            inputs[4] = amIOnRight ? _gm.ball.transform.localPosition.x : -_gm.ball.transform.localPosition.x;
            inputs[5] = amIOnRight ? _gm.ball.transform.localPosition.y : -_gm.ball.transform.localPosition.y;
            inputs[6] = amIOnRight ? _gm.ball.ballRb.velocity.x : -_gm.ball.ballRb.velocity.x;
            inputs[7] = amIOnRight ? _gm.ball.ballRb.velocity.y : -_gm.ball.ballRb.velocity.y;
            inputs[8] = _gm.ball.ballRb.angularVelocity;
        }
        #endregion
        #region input gathering igonring opponent
        else
        {
            inputs = new float[6];
            //inputs[0] = amIOnRight ? transform.localPosition.x : -transform.localPosition.x;
            inputs[0] = amIOnRight ? transform.localPosition.y : -transform.localPosition.y;
            inputs[1] = (transform.rotation.eulerAngles.z % 180) * Mathf.Deg2Rad;

            //inputs[3] = amIOnRight ? _gm.ball.transform.localPosition.x : -_gm.ball.transform.localPosition.x;
            inputs[2] = amIOnRight ? _gm.ball.transform.localPosition.y : -_gm.ball.transform.localPosition.y;
            inputs[3] = amIOnRight ? _gm.ball.ballRb.velocity.x : -_gm.ball.ballRb.velocity.x;
            inputs[4] = amIOnRight ? _gm.ball.ballRb.velocity.y : -_gm.ball.ballRb.velocity.y;
            inputs[5] = _gm.ball.ballRb.angularVelocity;
        }
        #endregion


        float[] output = brain.feedInputs(inputs);
        //float[] output = new float[2];
        if (!amIOnRight)
        {
            output[0] = -output[0];
            output[1] = -output[1];
        }

        if (noise)
        {
            output[0] += Random.Range(.25f, .25f);
            output[1] += Random.Range(.25f, .25f);
        }

        if (output[0] > -triggerBias && output[0] < triggerBias) _moveInput = 0;
        else if (output[0] > 0) _moveInput = 1;
        else if (output[0] < 0) _moveInput = -1;

        if (output[1] > -triggerBias && output[1] < triggerBias) _rotInput = 0;
        else if (output[1] > 0) _rotInput = 1;
        else if (output[1] < 0) _rotInput = -1;

        //Debug.Log($"[{output[0]}, {output[1]}");


    }


    public override float movementInput()
    {
        return _moveInput;
    }

    public override float rotationtInput()
    {
        return _rotInput;
    }
}

