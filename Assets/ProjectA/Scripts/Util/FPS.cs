using UnityEngine;
using System.Collections;

public class FPS : MonoBehaviour
{
    float lastInterval;
    int frames = 0;
    public static float fps;

    void Start()
    {
        lastInterval = Time.realtimeSinceStartup;
        frames = 0;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), (Mathf.Round(fps * 100.0f) / 100.0f).ToString() + " fps");
    }

    void Update()
    {
        ++frames;
        float timeNow = Time.realtimeSinceStartup;
        if (timeNow > lastInterval + 0.5f)
        {
            fps = frames / (timeNow - lastInterval);
            frames = 0;
            lastInterval = timeNow;
        }
    }
}