using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputGatherer : inputGatherer
{

    public override float movementInput()
    {
       
        return Input.GetAxisRaw("Vertical");
    }

    public override float rotationtInput()
    {
        return Input.GetAxisRaw("Horizontal");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
      
    }
}
