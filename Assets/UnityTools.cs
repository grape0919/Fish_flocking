using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityTools
{
    public static I GetInterface<I>(this Component cmp) where I : class
    {
        return cmp.GetComponent(typeof(I)) as I;
    }
}
