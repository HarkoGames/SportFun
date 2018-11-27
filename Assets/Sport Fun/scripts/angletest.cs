using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class angletest : MonoBehaviour
{
    public float angle;
    public bool xAxis;
    public bool yAxis;
    public bool zAxis;
    [Range(-1,1)]
    public int adjustment;
    public bool reset;

    public Vector3 resultDir;

    private Transform _transform;
    // Use this for initialization
    void Start()
    {
        _transform = transform;

    }

    // Update is called once per frame
    void Update()
    {
        if (reset)
        {
            this.angle = 0;
        }

        var targetDir = Vector3.forward;

        var targetX = (xAxis ? adjustment * angle : 0);
        var targetY = (yAxis ? adjustment * angle : 0);
        var targetZ = (zAxis ? adjustment * angle : 0);

        var rot = Quaternion.Euler(targetX,targetY, targetZ);
        targetDir =  rot * targetDir;


        var target = this.transform.position + (targetDir * 20);

        Debug.DrawLine(_transform.position, target, Color.cyan);

        resultDir = targetDir;
    }

}
