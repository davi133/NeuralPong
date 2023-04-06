using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class TrainingUnit : MonoBehaviour
{

    //[SerializeField] ChampionshipManager _cm;
    public int index = -1;
    public bool moreDebug = false;
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] GameManager _gm;
    [SerializeField] AgentInputGatherer _left_player;
    TheBall BOLA;
    //[SerializeField] private string leftFirstFile = "";
    [SerializeField] AgentInputGatherer _right_player;
    public AgentInputGatherer winner = null;
    public Vector2Int result = Vector2Int.zero;
    public bool running = false;
    //[SerializeField] private string rightFirstFile = "";

    public event Action<int> onEnd;


    public bool ended { get { return _gm.gameState == GameManager.GameState.Ended; } }

    public void StartTraining()
    {
        
        running = true;
        if (moreDebug) Debug.Log("starting trainin here");
        _left_player.GetComponent<SpriteRenderer>().color = Color.blue;
        _right_player.GetComponent<SpriteRenderer>().color = Color.blue;
        winner = null;
        result = new Vector2Int(0, 0);
        _gm.StartGame();
    }

    void Start()
    {
        _gm.gameEnd += GameEnd;
        BOLA = _gm.ball;
        _left_player.GetComponent<SpriteRenderer>().color = Color.blue;
        _left_player.mutate = false;


        _right_player.GetComponent<SpriteRenderer>().color = Color.blue;
        _right_player.mutate = false;

    }

    void GameEnd()
    {
        //Debug.Log($"game ended, winner{_gm.winner}");
        result = Vector2Int.RoundToInt(_gm.points);
        AgentInputGatherer Loser;
        if (_gm.winner == GameManager.Winner.Right)
        {
            winner = _right_player;
            Loser = _left_player;
        }
        else
        {
            winner = _left_player;
            Loser = _right_player;

        }
        winner.GetComponent<SpriteRenderer>().color = Color.green;
        Loser.GetComponent<SpriteRenderer>().color = Color.red;
        //if (running) onEnd?.Invoke(this.index);
        onEnd?.Invoke(this.index);
        running = false;
    }

    public Vector2 getResult()
    {
        return _gm.points;
    }

    public void setNetworks(NeuralNetwork Left, NeuralNetwork Right)
    {
        _left_player.brain = Left;
        _right_player.brain = Right;
    }
    public void setLeftNet(NeuralNetwork l)
    {
        _left_player.brain = l;
    }
    public void setRightNet(NeuralNetwork r)
    {
        _left_player.brain = r;
    }

    public void setLabel(string lbl)
    {
        label.SetText(lbl);
    }
}
