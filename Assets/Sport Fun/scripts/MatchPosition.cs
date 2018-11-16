using UnityEngine;

public class MatchPosition : MonoBehaviour {
    public Transform Target;

    public float SmoothPosition = .5f;
    public Vector3 offset = new Vector3(0f, 7.5f, 0f);
    public bool MatchRotation = false;
    public float SmoothRotation = .5f;

    // Update is called once per frame
    void LateUpdate() {
        transform.position = Target.position + offset;
        if (MatchRotation)
            transform.rotation = Quaternion.Slerp(transform.rotation, Target.rotation, Time.deltaTime * SmoothRotation);
	
	}
}
