using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheBall : MonoBehaviour
{
    [SerializeField]
    float maxSpeed = 20;
    [SerializeField]
    float minSpeed = 15;
    //[SerializeField] bool _startAutomatically = true;
    //public bool startAutomatically { get { return _startAutomatically; } }
    //[SerializeField] bool randomDirection = true;
    [SerializeField] LaunchMode launchMode = LaunchMode.AlternatingLR;

    //[SerializeField] private Transform firstDirection;
    [SerializeField] GameManager _gm;
    [SerializeField] Transform definedDirection;
    public Rigidbody2D ballRb { get; private set; }
    public bool initialized = false;

    private void Awake()
    {
        ballRb = GetComponent<Rigidbody2D>();
        initialized = true;
    }

    void Start()
    {
       
        /*if (_startAutomatically && _gm.gameState == GameManager.GameState.Waiting)
        {
            doFirstMovement();
        }*/

    }



    void Update()
    {
        

    }


    private void FixedUpdate()
    {
        if (ballRb.velocity.magnitude >= maxSpeed)
        {
            ballRb.velocity = ballRb.velocity.normalized * maxSpeed;
        }
        else if (ballRb.velocity.magnitude <= minSpeed)
        {
            ballRb.velocity = ballRb.velocity.normalized * minSpeed;
        }
        /*else if (ballRb.velocity.magnitude <= 0.01)
        {
            Debug.Log("aaaaa");
            transform.localPosition = Vector3.zero;
            this.doFirstMovement();
        }*/
        //transform.rotation = Quaternion.identity;
        //ballRb.angularVelocity = 0;

    }

    private int alternateAux = 1;
    public Vector2 launchVector= Vector2.up;
    public void doFirstMovement()
    {

        if (launchMode == LaunchMode.RandomDirection)
        {
            transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            if (transform.up .x >=0 && alternateAux==-1)
            {
                transform.up *= -1;
            }
            if (transform.up.x <= 0 && alternateAux == 1)
            {
                transform.up *= -1;
            }
            ballRb.AddForce(transform.up*5, ForceMode2D.Impulse);
            
            alternateAux *= -1;
        }
        else if (launchMode == LaunchMode.WellDistrubuted)
        {
            launchVector= launchVector.Rotate(23);
            ballRb.AddForce(launchVector * 5, ForceMode2D.Impulse);
            launchVector *= -1;
        }
        else if (launchMode == LaunchMode.AlternatingLR)
        {
            ballRb.AddForce(new Vector2(-5 * alternateAux, 0), ForceMode2D.Impulse);
            alternateAux *= -1;
        }
        else if (launchMode == LaunchMode.UpOrDown)
        {
            ballRb.AddForce(new Vector2(0, -5*alternateAux), ForceMode2D.Impulse);
            alternateAux *= -1;
        }
        else if (launchMode == LaunchMode.PreDefined)
        {

            ballRb.AddForce(definedDirection.up * 5, ForceMode2D.Impulse);
        }
        else if (launchMode == LaunchMode.AlwaysLeft)
        {
            ballRb.AddForce(new Vector2(-5, 0), ForceMode2D.Impulse);
        }
        else if (launchMode == LaunchMode.AlwaysRight)
        {
            ballRb.AddForce(new Vector2(5, 0), ForceMode2D.Impulse);
        }
        else
        {
            Debug.Log("launch mode not implemented");
        }

        //_gm.StartGame();
    }
    

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("BALL HIT");
        if (Mathf.Abs(ballRb.velocity.x) <= 2 && collision.gameObject.CompareTag("UB_border"))
        {

            float was = Mathf.Abs(ballRb.velocity.x);
            Vector2 aux = ballRb.velocity;
            float signx = Mathf.Sign(aux.x);
            aux.x = signx * 6f;
            //if (aux.x == 0) aux.x = Random.Range(0f, 1f) > 0.5f ? 6f : -6f;

            float signy = Mathf.Sign(aux.y);
            aux.y = signy * 4f;

            ballRb.velocity = aux;
            //Debug.Log($"speed is now {ballRb.velocity.x}");
            float now = Mathf.Abs(ballRb.velocity.x);
            if (was > now)
            {
                Debug.Log("oops");
            }
        }

        if (collision.gameObject.CompareTag("Goals_border"))
        {
            _gm.onGoalHit();
        }
    }

    public enum LaunchMode
    {
        PreDefined,
        RandomDirection,
        WellDistrubuted,
        AlternatingLR,
        UpOrDown,
        AlwaysLeft,
        AlwaysRight,
    }
}

public static class Vector2Extension
{   //http://answers.unity.com/answers/734946/view.html

    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
}