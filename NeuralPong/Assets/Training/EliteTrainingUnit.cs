using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class EliteTrainingUnit : MonoBehaviour
{
    //[SerializeField] ChampionshipManager _cm;
    public int index = -1;
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] GameManager _gm;
    [SerializeField] AgentInputGatherer _left_player;
    //[SerializeField] private string leftFirstFile = "";
    [SerializeField] SeekerAIInputGatherer _right_player;
    public Vector2Int result = Vector2Int.zero;
    public bool running = true;
    //[SerializeField] private string rightFirstFile = "";

    public event Action<int> onEnd;


    public bool ended { get { return _gm.gameState == GameManager.GameState.Ended; } }

    public void StartTraining()
    { 
        _left_player.GetComponent<SpriteRenderer>().color = Color.blue;
        _right_player.GetComponent<SpriteRenderer>().color = Color.blue;
        result = Vector2Int.RoundToInt(Vector2.zero);
        _gm.StartGame();
       
        running = true;
    }



    void Start()
    {
        _gm.gameEnd += GameEnd;


        _left_player.GetComponent<SpriteRenderer>().color = Color.blue;
        _left_player.mutate = false;
        _right_player.GetComponent<SpriteRenderer>().color = Color.blue;

    }

    void GameEnd()
    {
        //Debug.Log($"game ended, winner{_gm.winner}");
        result = Vector2Int.RoundToInt(_gm.points);
        inputGatherer Winner;
        inputGatherer Loser;
        if (_gm.winner == GameManager.Winner.Right)
        {
            Winner = _right_player;
            Loser = _left_player;
        }
        else
        {
            Winner = _left_player;
            Loser = _right_player;

        }
        Winner.GetComponent<SpriteRenderer>().color = Color.green;
        Loser.GetComponent<SpriteRenderer>().color = Color.red;
        if (running) onEnd?.Invoke(index);

        running = false;
    }


    public void setNetworks(NeuralNetwork Left)
    {
        _left_player.brain = Left;
        
    }

    public void setLabel(string lbl)
    {
        label.SetText(lbl);
    }
}
