using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateScript : MonoBehaviour
{
   

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(0, -.2f, 0); 
    }
}
