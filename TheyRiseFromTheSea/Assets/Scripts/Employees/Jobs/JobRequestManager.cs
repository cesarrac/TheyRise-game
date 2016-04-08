using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class JobRequestManager : MonoBehaviour {

    static JobRequestManager instance;

    Queue<Job> jobQueue = new Queue<Job>();

    void Awake()
    {
        instance = this;
    }
}
