using UnityEngine;
using System.Collections;

public class TimeSpeed_Controller : MonoBehaviour {

	public void NormalTime()
    {
        Time.timeScale = 1;
    }

    public void Pause()
    {
        Time.timeScale = 0;
    }

    public void Time2X()
    {
        Time.timeScale = 2;
    }

    public void Time4X()
    {
        Time.timeScale = 4;
    }

   
}
