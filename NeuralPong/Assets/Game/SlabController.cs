using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlabController : MonoBehaviour
{
    float rotation_input = 0;
    float direction_input = 0;
    [SerializeField] float speed = 5;
    [SerializeField] float rotation_speed = 100;


    float upperLimit = 4;
    float bottonLimit = -4;

    //[SerializeField] GameManager _gm;
    
    [SerializeField]
    private inputGatherer _controller;

    Vector3 _starting_position;

    private void Start()
    {
        _starting_position = transform.position;
        //Debug.Log(transform.localPosition);

        if (!_controller)
            _controller = GetComponent<inputGatherer>();
    }

    // Update is called once per frame
    void Update()
    {
        //gathering inputs


    }
    private void FixedUpdate()
    {
        if (_controller)
        {
            direction_input = _controller.movementInput();
            rotation_input = _controller.rotationtInput();
        }


        float moveDistance = direction_input * speed * Time.fixedDeltaTime;
        transform.localPosition += new Vector3(0, moveDistance, 0);

        if (transform.localPosition.y >= upperLimit)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, upperLimit, transform.localPosition.z);
        }
        else if (transform.localPosition.y <=  bottonLimit)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, bottonLimit, transform.localPosition.z);
        }


        //rotating
        float rotation = transform.rotation.eulerAngles.z;
        rotation -= rotation_input * rotation_speed * Time.fixedDeltaTime;
        transform.rotation = Quaternion.Euler(0, 0, rotation);
    }


}
