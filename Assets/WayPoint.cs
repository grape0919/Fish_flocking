using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour {

    public int editorPriority = 0;
    public Trace trace;

    void OnTriggerEnter(Collider other)
    {
        trace.NextWayPoint();
    }

    public void OnTouch(Boid boid)
    {
        if (GetComponent<Collider>().isTrigger)
            trace.NextWayPoint();
    }
}
