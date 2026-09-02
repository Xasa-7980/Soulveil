using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsKinematicDisable : MonoBehaviour
{

    public float time = 3;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("DisableIsKinematic", time);
    }

    public void DisableIsKinematic()
    {
        GetComponent<Rigidbody>().isKinematic = false;
    }
}
