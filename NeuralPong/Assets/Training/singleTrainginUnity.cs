using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class singleTrainginUnity : MonoBehaviour
{
    [SerializeField] GameManager _gm;
    [SerializeField] AgentInputGatherer _AILeft;
    [SerializeField] private string leftFirstFile = "";
    [SerializeField] AgentInputGatherer _AIRight;
    [SerializeField] private string rightFirstFile = "";
    //Vector2 _scores_last_time = Vector2.zero;

    void Start()
    {
        Application.runInBackground = true;
        Time.timeScale = 5;
        //_gm.goalHit += () => { _scores_last_time = _gm.points; };
        _gm.gameEnd += GameEnd;

        _AILeft.GetComponent<SpriteRenderer>().color = Color.blue;
        _AILeft.mutate = false;
        _AILeft.file = leftFirstFile;
        _AILeft.fileToSave = "_left";
        
        _AIRight.GetComponent<SpriteRenderer>().color = Color.red;
        _AIRight.mutate = false;
        _AIRight.file = rightFirstFile;
        _AIRight.fileToSave = "_right";
        

    }

    void GameEnd()
    {
        Debug.Log($"game ended, winner{_gm.winner}");
      
        AgentInputGatherer won;
        AgentInputGatherer lost;
        
        if (_gm.winner == GameManager.Winner.Right)
        {
            won = _AIRight;
            lost = _AILeft;
        }
        else
        {
            won = _AILeft;
            lost = _AIRight;
        }

        won.GetComponent<SpriteRenderer>().color = Color.green;
        lost.GetComponent<SpriteRenderer>().color = Color.red;

        lost.file = won.fileToSave;
        won.file = won.fileToSave;
        lost.mutate = true;
        won.mutate = false;

       // won.reStart();
        //lost.reStart();
        
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
