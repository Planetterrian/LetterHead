using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class TimerManager : Singleton<TimerManager>
{
    public class TimerEvent
    {
        public float time;
        public Action action;

        public void Fire()
        {
            if(action != null)
                action();
        }
    }

    private List<TimerEvent> events = new List<TimerEvent>();

    public static TimerEvent AddEvent(float time, Action callback)
    {
        return Instance.DoAddEvent(time, callback);
    }

    private TimerEvent DoAddEvent(float time, Action callback)
    {
        var evnt = new TimerEvent()
        {
            action = callback,
            time = Time.time + time,
        };

        events.Add(evnt);

        return evnt;
    }

    public void CancelEvent(TimerEvent evnt)
    {
        events.Remove(evnt);
    }

	// Update is called once per frame
	void Update () 
    {
	    for (int i = events.Count - 1; i >= 0 ; i--)
	    {
	        if(events[i].time <= Time.time)
	        {
	            events[i].Fire();
                events.RemoveAt(i);
	        }
	    }	
	}
}
