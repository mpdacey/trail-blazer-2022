using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunSetting : MonoBehaviour
{
    public Color startColor;
    public Color finishColor;
    public float startIntensity;
    public float finishIntensity;

    private Light sun;
    private Vector3 start;
    private Vector3 goal;
    private Transform player;
    private float currentPercent = 0;
    private float initialDistance;
    private float closestDistance;

    private void Start()
    {
        sun = GetComponent<Light>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        start = GameObject.Find("Torches").transform.GetChild(0).position;
        goal = GameObject.FindGameObjectWithTag("Goal").transform.position;

        initialDistance = Vector3.Distance(start, goal);
        closestDistance = initialDistance;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float currentDistance = Vector3.Distance(player.position, goal);

        if (currentDistance < closestDistance)
        {
            closestDistance = currentDistance;
            currentPercent = 1 - closestDistance / initialDistance;

            sun.color = Color.Lerp(startColor, finishColor, currentPercent);
            sun.intensity = Mathf.Lerp(startIntensity, finishIntensity, currentPercent);
        }
    }

    public void ResetDistance()
    {
        closestDistance = Vector3.Distance(start, goal);
        sun.color = startColor;
        sun.intensity = startIntensity;
    }
}
