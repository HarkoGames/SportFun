using UnityEngine;
using System.Collections;

namespace SportFun
{
    [RequireComponent(typeof(CommotionController))]
    public class PlayerInput : MonoBehaviour
    {

        private CommotionController myController;
        // Use this for initialization
        void Start()
        {
            myController = gameObject.GetComponent<CommotionController>();
            myCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        }

        string message;

        void OnGUI()
        {
            if (gameObject.name.Contains("(2)"))
            {
                GUI.Label(new Rect(50, 10, 1200, 60), string.Format("Player Input: {0}", message));
            }
        }

        public bool UseMouseLook = false;
        public float MouseRotateThreshold;
        public Camera myCam;
        // Update is called once per frame
        void Update()
        {
            message = "";

            Vector3 inputVector = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
            //Vector3 inputVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            var desiredVelocity = Utilities.GetDesiredVelocity(myCam, transform, inputVector); // move it to camera coordinates

            var mouseLookVector = Utilities.GetMouseLookRotation(myCam, inputVector);
            var freeLookVector = Utilities.GetDesiredFacingDirection(myCam, transform, inputVector);
            var currentVector = transform.rotation.eulerAngles;
            var freeLookDiff = currentVector.y - freeLookVector.y;
            Vector3 rotationVector = freeLookVector;
            if (UseMouseLook)
                rotationVector = mouseLookVector;

            //                Vector3 rotationVector = UseMouseLook ? GetMouseLookRotation(inputVector) : GetDesiredFacingDirection(inputVector);
            myController.SetActionKey(ActionKeys.Run, Input.GetButton("Run"));
            myController.SetActionKey(ActionKeys.Fight, Input.GetButtonDown("ChangeStance"));
            myController.SetActionKey(ActionKeys.Attack, Input.GetKeyDown(KeyCode.Q));
            myController.SetActionKey(ActionKeys.Jump, Input.GetKeyDown(KeyCode.Space));

            myController.Move(desiredVelocity, rotationVector);
            //Debug.Log("freelook Diff: " + freeLookDiff.ToString());
//            Debug.Log("player dv: " + desiredVelocity.ToString());

            //message += "i: " + inputVector + ",  dv: " + desiredVelocity;
            /*
                    if (Input.GetButtonUp("Inventory"))
                        Messenger<GUIWindowToToggle>.Broadcast("Toggle", GUIWindowToToggle.Inventory);
                    if (Input.GetButtonUp("CharacterWindow"))
                        Messenger<GUIWindowToToggle>.Broadcast("Toggle", GUIWindowToToggle.Character);
                */
        }
    }
}
