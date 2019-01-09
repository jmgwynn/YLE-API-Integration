using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumHolder : MonoBehaviour {

    public int index;
    public Menu men;


    public void SendToDetails()
    {
        men.ToDetails(index);
    }
}
