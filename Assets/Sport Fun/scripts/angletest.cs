using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class angletest : MonoBehaviour
{
    public GameObject capsule;
    private CapsuleCollider cCollider;
    public GameObject plane;
    public float angle;
    public bool xAxis;
    public bool yAxis;
    public bool zAxis;
    [Range(-1,1)]
    public int adjustment;
    public bool reset;

    public float slope;
    public Vector3 slopeNormal;

    public Vector3 resultDir;
    public Vector3 directionDir;

    private Transform _transform;
    // Use this for initialization
    void Start()
    {
        _transform = transform;
        cCollider = capsule.GetComponent<CapsuleCollider>();
    }

    public void GetSlope()
    {
        RaycastHit hit;
        Ray r = new Ray(capsule.transform.position, Vector3.down);
        slopeNormal = Vector3.zero;
        slope = 0f;
        if (Physics.Raycast(r, out hit, 10f))
        {
            slopeNormal = hit.normal;
            slope = Vector3.Angle(hit.normal, Vector3.up);
            Debug.DrawRay(transform.position, hit.normal, Color.gray);

        }

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

        GetSlope();

        // adjust the input by the slope, so that going up/ downhill is smoother
        var rot2 = Quaternion.Euler(slopeNormal);

        directionDir = (targetDir + slopeNormal).normalized; // rot2 * targetDir;
        var target2 = this.transform.position + (directionDir * 20);

        Debug.DrawLine(_transform.position, target2, Color.green);

        rot2 = Quaternion.FromToRotation(Vector3.up, slopeNormal);

        directionDir = rot2 * targetDir;
        target2 = this.transform.position + (directionDir * 20);
        Debug.DrawLine(_transform.position, target2, Color.blue);
    }

}
