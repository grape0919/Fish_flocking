using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Trace : MonoBehaviour
{
    public WayPoint[] wayPoints;
    private WayPoint curWP = null;

    public void Start()
    {
        foreach (var wp in wayPoints)
            SetTrigger(wp, false);

        if (wayPoints.Length > 0)
        {
            curWP = wayPoints[0];
            SetTrigger(curWP, true);
        }
    }

    void SetTrigger(WayPoint wp, bool value)
    {
        wp.GetComponent<Collider>().isTrigger = value;
    }

    public Vector3 GetAtractionPoint()
    {
        return curWP.transform.position;
    }

    public void NextWayPoint()
    {
        SetTrigger(curWP, false);

        var nextIndex = Array.FindIndex(wayPoints, (v) => v == curWP) + 1;

        if (nextIndex == wayPoints.Length)
            nextIndex = 0;

        curWP = wayPoints[nextIndex];
        SetTrigger(curWP, true);
    }
}
