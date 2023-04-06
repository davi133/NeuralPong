using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    //[SerializeField] GameManager arena;
    public bool moreDebug = false;
    [SerializeField] int win_goal = 10;
    private Vector2 _points = Vector2.zero;
    public Vector2 points { 
        get { return _points; } 
        private set 
        {
            _points = value;
            if (_uim) _uim.UpdatePlacar(_points);
            if (placar) placar.SetText($"{Vector2Int.RoundToInt(points)}");
        } 
    }
    [SerializeField] bool autoStartGame = false;
    [SerializeField] bool autoStartRound = false;
    [SerializeField] bool autoReStartGame = false;
    [SerializeField] bool restartPosition = false;

    float lastPoint=0;
    public delegate void PongEvent();
    public PongEvent gameStart;
    public PongEvent roundStart;
    public PongEvent goalHit;
    public PongEvent gameEnd;
    
    [SerializeField] Winner _winner = Winner.None;
    public Winner winner
    {
        get { return _winner; }
        private set
        {
            if (_uim) _uim.setWinner(value);
            _winner = value;
        }
    }

    [SerializeField] GameState _gameState = GameState.PreGame;
    public GameState gameState
    {
        get { return _gameState; }
        private set
        {
            if (_uim) _uim.setStatus(value);
            _gameState = value;
        }
    }


    [Header("Game members")]
    [SerializeField] Transform _pLeft;
    public Transform playerLeft { get { return _pLeft; } private set { _pLeft = value; } }
    [SerializeField] Transform _pRight;
    public Transform playerRight { get { return _pRight; } private set { _pRight = value; } }
    [SerializeField] TheBall _ball;
    public TheBall ball { get { return _ball; } private set { _ball = value; } }
    Rigidbody2D _ballRb;
    [SerializeField] Transform _arena;
    public Transform arena { get { return _arena; } private set { _arena = value; } }
    [SerializeField] UI_manager _uim;
    [SerializeField] TextMeshProUGUI placar;
    
    private void Start()
    {
        _ballRb = ball.GetComponent<Rigidbody2D>();
        _prepareMatch();
    }
    private void Update()
    { //|| gameState == GameState.PreGame)
        if (gameState == GameState.PreGame && (autoStartGame || Input.GetKey(KeyCode.Space))
            || (gameState == GameState.Ended && (autoReStartGame || Input.GetKey(KeyCode.Space))))
        {
            //Debug.Log("calling");
            StartGame();
        }

        if ((!autoStartRound && gameState == GameState.Waiting && Input.GetKey(KeyCode.Space))
           || (autoStartRound && gameState == GameState.Waiting))
        {
            //Debug.Log("calling round");
            _startRound();
        }

        if (Time.time > lastPoint +90 && gameState == GameState.Ongoing)
        {
            //Debug.Log(_ball.GetComponent<Rigidbody2D>().velocity);
            //Debug.Log(_ball.GetComponent<Rigidbody2D>().velocity.magnitude <0.1f);
            lastPoint = Time.time;
            _ball.transform.localPosition = Vector3.zero;
            _restatPlayersPositions();
            _ball.doFirstMovement();
        }
        if (_ballRb.velocity.magnitude <1f && gameState == GameState.Ongoing)
        {
            //Debug.Log(_ball.GetComponent<Rigidbody2D>().velocity);
            //Debug.Log(_ball.GetComponent<Rigidbody2D>().velocity.magnitude);
            //lastPoint = Time.time;
            //_ball.transform.localPosition = Vector3.zero;
            _restatPlayersPositions();
            _ball.doFirstMovement();
        }


    }

    public void StartGame()
    {
        //Debug.Log("StartGame");
        gameState = GameState.Waiting;
        winner = Winner.None;
        ball.launchVector = Vector2.up;
        points = Vector2.zero;
        _startRound();
        gameStart?.Invoke();
        lastPoint = Time.time;
    }
    void _startRound()
    {
        //Debug.Log("_startRound");
        gameState = GameState.Ongoing;
        _prepareMatch();
        _ball.doFirstMovement();
        
        roundStart?.Invoke();
    }

    
    void _restatPlayersPositions()
    {
        playerLeft.SetPositionAndRotation(_arena.position + new Vector3(-7, 0, 0), Quaternion.identity);
        playerRight.SetPositionAndRotation(_arena.position + new Vector3(7, 0, 0), Quaternion.identity);
        if (moreDebug)
        {
            Debug.Log(playerLeft.position);
            Debug.Log(playerRight.position);
        }
    }
    void _resetBall()
    {
        _ball.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
        _ball.transform.position = _arena.position;
        _ball.transform.rotation = Quaternion.identity;
        _ball.GetComponent<Rigidbody2D>().angularVelocity = 0;
    }
    void _prepareMatch()
    {

        
        _resetBall();
        if (restartPosition)
            _restatPlayersPositions();

    }
    
    public void onGoalHit()
    {
        lastPoint = Time.time;
        //Debug.Log("goal hit");
        if (_ball.transform.position.x > _arena.position.x)
        {
            //_points.x += 1;
            points = new Vector2(points.x + 1, points.y);
        }
        else
        {
            //_points.y += 1;
            points = new Vector2(points.x, points.y+1);
        }

        //reseting ball;
        _resetBall();

        //Debug.Log(_points);
        //reseting players
        /*if (restartPosition)
        {
            _restatPlayersPositions();
        }*/

        gameState = GameState.Waiting;
        _testEndGame();

        //if (_uim) _uim.UpdatePlacar(_points);
        goalHit?.Invoke();
    }

    bool endedNow = false;//redundancia por causa de bug
    void _testEndGame()
    {
        if (gameState == GameState.Ended)
        {
            Debug.Log("how did you slip?");
            return;
        }
        if (_points.x >= win_goal)
        {
            winner = Winner.Left;
            gameState = GameState.Ended;
            endedNow = true;
        }
        else if (_points.y >= win_goal)
        {
            winner = Winner.Right;
            gameState = GameState.Ended;
            endedNow = true;
        }
        else
        {
            endedNow = false;
        }

        if (gameState == GameState.Ended)
        {
            if (endedNow)
                gameEnd?.Invoke();
            else
                Debug.Log("como????");
            endedNow = false;

            if (autoReStartGame)
            {
                StartGame();
            }
        }
            
    }
    
    public enum Winner
    {
        None = 0,
        Right = 1,
        Left = -1,
    }

    public enum GameState
    {
        PreGame,
        Waiting,
        Ongoing,
        Ended
    }
}
