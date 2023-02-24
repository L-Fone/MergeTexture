using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class TimeWatch
{
    private static Stopwatch _startTime = null;

    public static void Start()
    {
        if (_startTime == null)
            _startTime = new Stopwatch();
        _startTime.Reset();
        _startTime.Start();
    }

    public static void ShowTime(string sign)
    {
        if (_startTime != null)
        {
            _startTime.Stop();
            UnityEngine.Debug.LogErrorFormat("{0} 测试时间为 {1} ms", sign, _startTime.ElapsedMilliseconds);
        }
    }
}
