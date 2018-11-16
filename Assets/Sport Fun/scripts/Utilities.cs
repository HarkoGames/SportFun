using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SportFun
{
    public static class Utilities
    {
        public static Quaternion CalculateFacingDirection(Transform t, Vector3 dr, float RotateSpeed)
        {
            Quaternion facingDir = new Quaternion();
            //message += "\ninitial dr: " + dr.ToString();
            if (dr != Vector3.zero) // only do rotation if there is actually some input
            {
                /* handle facing direction - taken from charactermotor code in gta shooter example*/
                dr = (dr.magnitude > 1 ? dr.normalized : dr);
                // Calculate which way character should be facing
                float facingWeight = dr.magnitude;
                Vector3 combinedFacingDirection = (
                    t.rotation * dr * (1 - facingWeight)
                    + dr * facingWeight
                );
                //message += " combined Facing Direction: " + combinedFacingDirection.ToString();
                combinedFacingDirection = combinedFacingDirection - Vector3.Project(combinedFacingDirection, t.up); //Util.ProjectOntoPlane(combinedFacingDirection, transform.up);
                combinedFacingDirection = .5f * combinedFacingDirection; // alignCorrection .5f ?

                if (combinedFacingDirection.sqrMagnitude > 0.01f)
                {
                    //message += string.Format(" updating facingDir: {0} to ", facingDir);
                    Vector3 newForward = Vector3.Slerp(t.forward, combinedFacingDirection, RotateSpeed * Time.deltaTime);//Util.ConstantSlerp(transform.forward, combinedFacingDirection, RotateSpeed * Time.deltaTime);
                    newForward = newForward - Vector3.Project(newForward, t.up);//Util.ProjectOntoPlane(newForward, transform.up);   

                    Vector3 drawPosition = t.position + new Vector3(0, 2, 0);
                    Debug.DrawLine(drawPosition, drawPosition + newForward, Color.yellow);

                    facingDir.SetLookRotation(newForward, t.up);
                    //message += string.Format("{0} ", facingDir);
                }
            }
            else
            {
                // message += " keeping rotation: " + transform.rotation.ToString();
                //facingDir = myAnim.bodyRotation; // don't change if no input vector was sent
                facingDir = t.rotation; // don't change if no input vector was sent
            }
            //message += " dr: " + dr + " facingDir: " + facingDir.ToString() + "\n";
            return facingDir;
        }

        public static float GetFacingDifference(Quaternion rotA, Quaternion rotB)
        {
            // found on unity answers http://answers.unity3d.com/questions/26783/how-to-get-the-signed-angle-between-two-quaternion.html
            float diff = 0f;

            // get a "forward vector" for each rotation
            var forwardA = rotA * Vector3.forward;
            var forwardB = rotB * Vector3.forward;

            return GetFacingDifference(forwardA, forwardB);
        }

        public static float GetFacingDifference(Vector3 rotA, Vector3 rotB)
        {
            // found on unity answers http://answers.unity3d.com/questions/26783/how-to-get-the-signed-angle-between-two-quaternion.html
            float diff = 0f;

            // get a "forward vector" for each rotation
            var forwardA = rotA.normalized;
            var forwardB = rotB.normalized;

            // get a numeric angle for each vector, on the X-Z plane (relative to world forward)
            var angleA = Mathf.Atan2(forwardA.x, forwardA.z) * Mathf.Rad2Deg;
            var angleB = Mathf.Atan2(forwardB.x, forwardB.z) * Mathf.Rad2Deg;

            // get the signed difference in these angles
            diff = Mathf.DeltaAngle(angleA, angleB);

            return diff;
        }


        public static Vector3 GetDesiredVelocity(Camera c, Transform t, Vector3 inputVector)
        {
            // make the input vector magnitude >= 1
            Vector3 directionVector = (inputVector.magnitude > 1 ? inputVector.normalized : inputVector);
            directionVector = directionVector.normalized * Mathf.Pow(directionVector.magnitude, 2);

            // rotate the input vector into camera space so up is camera's up and right is camera's right
            directionVector = c.transform.rotation * directionVector;

            // make input to be perpindicular to character's up vector
            Quaternion camToCharSpace = Quaternion.FromToRotation(c.transform.forward * -1, t.up);
            directionVector = (camToCharSpace * directionVector);

            return directionVector;
        }

        public static Vector3 GetDesiredFacingDirection(Camera c, Transform t, Vector3 inputVector)
        {
            // Get input vector from kayboard or analog stick and make it length 1 at most
            Vector3 directionVector = inputVector;
            //if (directionVector.magnitude > 1) 
            directionVector = directionVector.normalized;

            // Rotate input vector into camera space so up is camera's up and right is camera's right
            directionVector = c.transform.rotation * directionVector;

            // Rotate input vector to be perpendicular to character's up vector
            Quaternion camToCharacterSpace = Quaternion.FromToRotation(c.transform.forward * -1, t.up);
            directionVector = (camToCharacterSpace * directionVector);
            //Debug.Log("DirectionVector: " + directionVector.ToString());
            return directionVector;
        }

        public static Vector3 GetMouseLookRotation(Camera c, Vector3 inputVector)
        {
            if (c != null)
            {
                return c.transform.rotation * Vector3.forward;
            }
            else
            {
                Debug.LogWarning("Attempting to use Mouselook but no camera is set.");
                return Vector3.zero;
            }
        }

        public static IEnumerator WaitForTransition(this Animator anim, System.Action a )
        {
            while (anim.GetBool("Exit"))
            {
                yield return null;
            }
            a();
            yield return null;
        }

        public static void RunAction(System.Action a)
        {
            if (a != null)
            {
                a();
            }
        }
    }

    public class Timer
    {
        float currentTime;
        public bool IsRunning;
        public int TargetTime { get; set; }
        public bool IsCountdown;
        public float Progress { get; set; }
        public bool IsFinished { get; set; }

        public Timer()
        {
            IsFinished = false;
            currentTime = 0f;
        }

        public Timer(int targetTime, bool isCountdown = false)
            : base()
        {
            TargetTime = targetTime;
            IsCountdown = isCountdown;
        }

        public static Timer StartTimer(int targetTime, bool isCountdown)
        {
            var t = new Timer() { TargetTime = targetTime, IsCountdown = isCountdown };
            t.Start();
            return t;
        }

        public void Start()
        {
            currentTime = IsCountdown ? TargetTime : 0f;
            IsFinished = false;
            IsRunning = true;
            Progress = 0f;
        }

        public void Stop()
        {
            IsRunning = false;
            IsFinished = true;
        }

        /// <summary>
        /// Update the timer.  Return true if it has completed
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public bool Update(float deltaTime)
        {
            if (!IsRunning) return false;
            if (IsCountdown)
            {
                currentTime -= deltaTime;
                if (currentTime <= 0)
                {
                    IsFinished = true;
                }
                Progress = (TargetTime - currentTime) / TargetTime;
            }
            else
            {
                currentTime += deltaTime;
                if (currentTime >= TargetTime)
                {
                    IsFinished = true;
                }
                Progress = currentTime / TargetTime;
            }
            return IsFinished;
        }
    }
}
