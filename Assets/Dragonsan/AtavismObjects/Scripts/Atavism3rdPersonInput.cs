using System;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
#if AT_MOBILE
using UnityStandardAssets.CrossPlatformInput;
#endif
namespace Atavism
{
    public class Atavism3rdPersonInput : AtavismInputController
    {

#if AT_MOBILE
        private float rotationSpeed = 15f; // Rotation Player         ***PopuGames***
        public bool _canRun = true; // PopuGames
        public bool _canRoate = true; // PopuGames
        public bool rootMotion = true; //PopuGames
        bool touch0CanZoom = true; //PopuGames
        bool touch1CanZoom = true; //PopuGames
        bool touch2CanZoom = true; //PopuGames
        public GameObject autoRunON;
        public GameObject autoRunOFF;
        private Vector3 lastPanPosition;
        private int panFingerId; // Touch mode only
        bool canZoom; //  if touch not on UI
        private bool wasZoomingLastFrame; // Touch mode only
        private Vector2[] lastZoomPositions; // Touch mode only
        private Vector3 moveDirection = Vector3.zero;

        #region ClickToMoveInputController

        public LayerMask layerMask;
        public LayerMask ignoreLayers;

        public bool _activated = false;
        public bool allowKeyMovement = true;

        public float targetRadius = 2f;

        private Vector3 position;
        private GameObject clickedTarget;
        private float distanceToStopAt = 0.25f;
        private bool sentStrike = false;
        private float activateTime = -1f;
        public float harvestDelay = 0.5f;

        public static bool attack;
        public static bool die;

        public float minDistance = 5f;

        public float maxDistance = 20f;

        // The distance in the x-z plane to the target
        public float distance = 10.0f;

        // the height we want the camera to be above the target
        public float height = 8.0f;

        float heightDif = 0f;

        // How much we 
        float heightDamping = 2.0f;

        public Vector3 cameraRotation = Vector3.forward;

        // target indicator
        public ParticleSystem particleIndicator;
        public float particleLife = 1f;
        public float particleYOffset = 0.2f;
        GameObject currentParticle = null;
        float particleExpiration = -1f;

        #endregion
#endif
        
        //public KeyCode walkToggleKey = KeyCode.Backslash;
        // Walk on
        public bool walk = false;
        // The speed when walking
        //	float walkSpeed = 2.0f;
        // when pressing "Fire3" button (cmd) we start running
        //	float runSpeed = 6.0f;
        Vector3 playerAccel = Vector3.zero;
        //	Quaternion playerRotation = Quaternion.identity;
        public float rotateSpeed = 5.0f;

        public bool rotateCharacterToCameraView = false;
        // Are we moving backwards (This locks the camera to not do a 180 degree spin)
        private bool movingBack = false;
        // Is the user pressing any keys?
        //private bool isMoving = false;

        private bool isControllable = true;

        // Last time the jump button was clicked down
        //	private float lastJumpButtonTime = -10.0f;
        // Last time we performed a jump
        //	private float lastJumpTime = -1.0f;

        #region Camera Fields
        public float MouseVelocity = 0.035f;
        public float MouseWheelVelocity = -1.0f;

        // Camera parameters - - exposed via the ParameterRegistry
        //	bool playerVisible = true;
        public bool cameraFirstPerson = false;
        bool cameraFree = false;
        //	bool cameraMotionYaw = true;

        private bool mouseOverUI = false;
        /// <summary>
        ///   This variable allows me to lock the interface to mouse look mode.
        ///   Essentially, this treats the interface as though the right mouse
        ///   button is down all the time for purposes of the camera updates.
        ///   The movement still follows 
        /// </summary>
        public bool mouseLookLocked = false;

        public bool blockFollowMouseWhenWindowOpened = true;
        bool leftButtonDown = false;
        bool rightButtonDown = false;
        bool mouseRun = false;
        //	float minPlayerVisibleDistance = 2f;
        float minThirdPersonDistance = 1f;
        public float headHeightAbovePlayerOrigin = 1.8f;
        public Vector3 cameraTargetOffset;
        public Vector3 cameraTargetCenterOffset;

        public bool cameraFollowMouse = false;
        // cameraPosition is only used if cameraFree is true

        //	bool playerOrientationInitialized = false;
        Quaternion cameraOrientation;
        Vector3 cameraPosition = Vector3.zero;

        // Camera pitch (in degrees)
        public float maxPitch = 85.0f; //275.0f;
        public float minPitch = 275.0f; //85.0f;
        public float idealPitch = 20.0f;
        public float cameraDist = 5;
        public float cameraMaxDist = 20;
        public float cameraMinDist = 0;

        public LayerMask obstacleLayers = 0, groundLayers = 512;

        Vector3 mousePosition = Vector3.zero;

        public SDETargeting sdeTarget;

        #endregion Camera Fields

        //   bool moveForwardLeft = false;
        //   bool moveForwardRight = false;
        //   bool moveBackwardLeft = false;
        //   bool moveBackwardRight = false;
        bool mouseRightButtonClicked = false;
        bool mouseLeftButtonClicked = false;
        bool cameraFollowChar = true;
        public float rotateCameraSpeed = 80.0f;
        Vector3 forcePower = Vector3.zero;
        bool noMove = false;
        bool dead = false;
        string state = "";
        private bool _keyChange = false;
        [AtavismSeparator("Zoom Settings")]
       //public bool zoomActive = false;
        public float zoomCameraDist = 5;
      //  public float zoomIdealPitch = 0.0f;
        // Use this for initialization
        void Start()
        {
            // Need to tell the client that this is the new active input controller
            ClientAPI.InputControllerActivated(this);
            if (cameraFirstPerson)
            {
                cameraMinDist = 0;
                cameraMaxDist = 0;
                cameraDist = 0;
                
            }
            cameraTargetOffset = headHeightAbovePlayerOrigin * Vector3.up;
            //cameraOrientation = transform.rotation * Vector3.back;
            //cameraOrientation = Quaternion.AngleAxis (Mathf.Deg2Rad * 20.0f, Vector3.up);
            AtavismEventSystem.RegisterEvent("MOUSE_SENSITIVE", this);
            AtavismEventSystem.RegisterEvent("CHANGE_KEY", this);
            MouseVelocity = AtavismSettings.Instance.GetGeneralSettings().sensitivityMouse;
            MouseWheelVelocity = AtavismSettings.Instance.GetGeneralSettings().sensitivityWheelMouse;
            Camera camera = Camera.main;
            if (PlayerYaw >= 180)
                CameraYaw = this.PlayerYaw - 180.0f;
            else
                CameraYaw = this.PlayerYaw + 180.0f;
            Vector3 cameraDir = camera.transform.rotation * Vector3.forward;
            Vector3 cameraTarget = target.position + target.rotation * cameraTargetOffset;
            camera.transform.position = (cameraTarget + cameraDir * cameraDist);

            sdeTarget = gameObject.GetComponent<SDETargeting>();
            if (sdeTarget == null)
            {
                sdeTarget = gameObject.AddComponent<SDETargeting>();
            }

            if (sdeTarget != null)
            {
                sdeTarget.A3PI = this;
                sdeTarget.Initiate();
            }
#if AT_MOBILE
            camera.transform.localRotation = Quaternion.identity; //PopuGames
#endif
        }
        private void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("MOUSE_SENSITIVE", this);
            AtavismEventSystem.UnregisterEvent("CHANGE_KEY", this);

        }
        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "MOUSE_SENSITIVE")
            {
                MouseVelocity = AtavismSettings.Instance.GetGeneralSettings().sensitivityMouse;
                MouseWheelVelocity = AtavismSettings.Instance.GetGeneralSettings().sensitivityWheelMouse;
            }
            if (eData.eventType == "CHANGE_KEY")
            {
                if (eData.eventArgs[0].Equals("T"))
                {
                    _keyChange = true;
                }
                else
                {
                    _keyChange = false;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (target == null)
                return;
            // Update camera
            UpdateCamera();
            UpdateCamera(target.position, target.rotation);

        }

        public override Vector3 GetPlayerMovement()
        {
            if (!isControllable)
            {
                // kill all inputs if not controllable.
                //Input.ResetInputAxes ();
            }

            // Check if mouse is over UI for this frame
            mouseOverUI = AtavismCursor.Instance.IsMouseOverUI();
            //mouseOverUI = AtavismUiSystem.IsMouseOverFrame ();

            //UpdateSmoothedMovementDirection ();
            HandleImmediateKeys(UnityEngine.Time.deltaTime, UnityEngine.Time.time);

            if (playerAccel.z < -0.2)
                movingBack = true;
            else
                movingBack = false;

            ClientAPI.GetPlayerObject().GameObject.SendMessage("MovingBack", movingBack);

            return (ClientAPI.GetPlayerObject().Orientation * playerAccel);
        }

        public override void RunCameraUpdate()
        {
            UpdateCamera();
            UpdateCamera(target.position, target.rotation);
        }

        #region Movement

        protected enum MoveEnum : int
        {
            Left = 0,
            Right = 1,
            Forward = 2,
            Back = 3,
            StrafeLeft = 4,
            StrafeRight = 5,
            Up = 6,
            Down = 7,
            AutoRun = 8,
            Count = 9
        }

        protected bool[] movement = new bool[(int)MoveEnum.Count];

#if AT_MOBILE
        public override void AutoRunON() //PopuGames 
        {
            autoRun = true;
            if(autoRunOFF) autoRunOFF.SetActive(true);
            if(autoRunON) autoRunON.SetActive(false);
            
            AtavismEventSystem.DispatchEvent("AUTO_RUN",new string[]{"Y"});
        }

        public override void AutoRunOFF() //PopuGames
        {
            autoRun = false;
            if(autoRunOFF) autoRunOFF.SetActive(false);
            if(autoRunON) autoRunON.SetActive(true);
            AtavismEventSystem.DispatchEvent("AUTO_RUN",new string[]{"N"});
        }
#endif
        
        public void MoveForward(bool status)
        {
            if (status)
            {
                movement[(int)MoveEnum.Forward] = true;
                movement[(int)MoveEnum.Back] = false;
                movement[(int)MoveEnum.AutoRun] = false;
            }
            else
            {
                movement[(int)MoveEnum.Forward] = false;
            }
        }

        public void MoveBackward(bool status)
        {
            if (status)
            {
                movement[(int)MoveEnum.Forward] = false;
                movement[(int)MoveEnum.Back] = true;
                movement[(int)MoveEnum.AutoRun] = false;
            }
            else
            {
                movement[(int)MoveEnum.Back] = false;
            }
        }

        public void TurnLeft(bool status)
        {
            if (status)
            {
                movement[(int)MoveEnum.Left] = true;
                movement[(int)MoveEnum.Right] = false;
            }
            else
            {
                movement[(int)MoveEnum.Left] = false;
            }
        }

        public void TurnRight(bool status)
        {
            if (status)
            {
                movement[(int)MoveEnum.Left] = false;
                movement[(int)MoveEnum.Right] = true;
            }
            else
            {
                movement[(int)MoveEnum.Right] = false;
            }
        }

        public void StrafeLeft(bool status)
        {
            movement[(int)MoveEnum.StrafeLeft] = status;
        }

        public void StrafeRight(bool status)
        {
            movement[(int)MoveEnum.StrafeRight] = status;
        }

        public void MoveUp(bool status)
        {
            if (status)
            {
                movement[(int)MoveEnum.Up] = true;
                movement[(int)MoveEnum.Down] = false;
                movement[(int)MoveEnum.AutoRun] = false;
            }
            else
            {
                movement[(int)MoveEnum.Up] = false;
            }
        }

        public void MoveDown(bool status)
        {
            if (status)
            {
                movement[(int)MoveEnum.Up] = false;
                movement[(int)MoveEnum.Down] = true;
                movement[(int)MoveEnum.AutoRun] = false;
            }
            else
            {
                movement[(int)MoveEnum.Down] = false;
            }
        }

        public void ToggleAutorun()
        {
            movement[(int)MoveEnum.AutoRun] = !movement[(int)MoveEnum.AutoRun];
        }

        /// <summary>
        ///   Handle the keyboard and mouse input for movement of the player and camera.
        ///   This method name says immediate, but really it is acting on a keyboard state
        ///   that may have been filled in by buffered or immediate input.
        /// </summary>
        /// <param name="timeSinceLastFrame">This is supposed to be in milliseconds, but seems to be in seconds.</param>
        protected void HandleImmediateKeys(float timeSinceLastFrame, float now)
        {
            // Now handle movement and stuff

            // Ignore the input if we're in the loading state
            //if (Client.Instance.LoadingState)
            //	return;

            // reset acceleration zero
            playerAccel = Vector3.zero;

            if (isControllable && !ClientAPI.UIHasFocus() && !_keyChange)
            {
                // Check key states
                if (Input.GetKey(AtavismSettings.Instance.GetKeySettings().moveForward.key) || Input.GetKey(AtavismSettings.Instance.GetKeySettings().moveForward.altKey))
                    MoveForward(true);
                else
                    MoveForward(false);
                if (Input.GetKey(AtavismSettings.Instance.GetKeySettings().moveBackward.key) || Input.GetKey(AtavismSettings.Instance.GetKeySettings().moveBackward.altKey))
                    MoveBackward(true);
                else
                    MoveBackward(false);
#if AT_MOBILE
                if (CrossPlatformInputManager.GetAxis("Horizontal") != 0 || CrossPlatformInputManager.GetAxis("Vertical") != 0) //         PopuGames  Input virtual joystik (mobile input)
                {
                    if (autoRun) //PopuGames  if the joystick move, disable autorun
                    {
                        AutoRunOFF();
                    }

                    if (_canRun && rootMotion) // PopuGames
                    {
                        MoveForward(true);
                    }
                    else
                    {
                        MoveForward(false);
                    }

                    if (_canRoate) // PopuGames
                    {
                        UpdateSmoothedMovementDirection();
                    }
                }
                else
                {
                    MoveForward(false);
                }


                if (autoRun && _canRoate && _canRun) // PopuGames   when the autoRun button is activated
                {

                    if (rootMotion)
                    {
                        MoveForward(true);
                    }

                    Transform cameraTransform = Camera.main.transform;
                    Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
                    forward.y = 0;
                    forward = forward.normalized;
                    target.rotation = Quaternion.LookRotation(forward);

                } //PopuGames 

#endif
                // Strafing keys
              
                if (Input.GetKey(AtavismSettings.Instance.GetKeySettings().strafeLeft.key)||Input.GetKey(AtavismSettings.Instance.GetKeySettings().strafeLeft.altKey))
                    StrafeLeft(true);
                else
                    StrafeLeft(false);
                if (Input.GetKey(AtavismSettings.Instance.GetKeySettings().strafeRight.key)||Input.GetKey(AtavismSettings.Instance.GetKeySettings().strafeRight.altKey))
                    StrafeRight(true);
                else
                    StrafeRight(false);


                // Turning keys
                if (Input.GetKey(AtavismSettings.Instance.GetKeySettings().turnLeft.key) || Input.GetKey(AtavismSettings.Instance.GetKeySettings().turnLeft.altKey))
                    TurnLeft(true);
                else
                    TurnLeft(false);
                if (Input.GetKey(AtavismSettings.Instance.GetKeySettings().turnRight.key) || Input.GetKey(AtavismSettings.Instance.GetKeySettings().turnRight.altKey))
                    TurnRight(true);
                else
                    TurnRight(false);

                // Auto run
                if (Input.GetKeyUp(AtavismSettings.Instance.GetKeySettings().autoRun.key)||Input.GetKeyUp(AtavismSettings.Instance.GetKeySettings().autoRun.altKey))
                    ToggleAutorun();

                // Update character state
                if (Input.GetKeyUp(AtavismSettings.Instance.GetKeySettings().walkRun.key)||Input.GetKeyUp(AtavismSettings.Instance.GetKeySettings().walkRun.altKey))
                {
                    // Toggle walking state
                    walk = !walk;
                }

                ClientAPI.GetPlayerObject().GameObject.SendMessage("SetWalk", walk);
                /*	
                    if (Input.GetButtonDown ("Jump")) {
                        ClientAPI.GetPlayerObject().GameObject.SendMessage("StartJump");
                    }
                    */
#if !AT_MOBILE                
                if (Input.GetKey(AtavismSettings.Instance.GetKeySettings().jump.key)||Input.GetKeyDown(AtavismSettings.Instance.GetKeySettings().jump.altKey))
                {
                    ClientAPI.GetPlayerObject().GameObject.SendMessage("StartJump");
                }
#endif
            }
            else
            {
                MoveForward(false);
                MoveBackward(false);
                StrafeLeft(false);
                StrafeRight(false);
                TurnLeft(false);
                TurnRight(false);
            }

            // Mouse state
            if (Input.GetMouseButtonDown(0) && !mouseOverUI && !_keyChange)
                leftButtonDown = true;
            if (Input.GetMouseButtonUp(0))
                leftButtonDown = false;
            if (Input.GetMouseButtonDown(1) && !mouseOverUI&& !_keyChange)
            {
                rightButtonDown = true;
            }
            if (Input.GetMouseButtonUp(1) && !_keyChange)
            {
                rightButtonDown = false;
            }
#if !AT_MOBILE
            if (leftButtonDown && rightButtonDown)
            {
                mouseRun = true;
            }
            else if (mouseRun)
            {
                mouseRun = false;
                movement[(int)MoveEnum.AutoRun] = false;
            }
#endif
            if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().PropertyExists("deadstate"))
            {
                dead = (bool)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().GetProperty("deadstate");
            }
            if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().PropertyExists("state"))
            {
                state = (string)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().GetProperty("state");
            }

            // Apply acceleration
            if (ClientAPI.WorldManager.Player.CanMove() && (!dead || state == "spirit"))
            {
                if (movement[(int)MoveEnum.Forward] || movement[(int)MoveEnum.AutoRun] || mouseRun)
                    playerAccel.z += 1.0f;
                if (movement[(int)MoveEnum.Back])
                    playerAccel.z -= 1.0f;
                if (movement[(int)MoveEnum.StrafeLeft])
                    playerAccel.x -= 0.5f;
                if (movement[(int)MoveEnum.StrafeRight])
                    playerAccel.x += 0.5f;
                /*if (movement[(int)MoveEnum.Up])
                    playerAccel.y += 0.5f;
                if (movement[(int)MoveEnum.Down])
                    playerAccel.y -= 0.5f;*/

                if (playerAccel.z < -0.2)
                    movingBack = true;
                else
                    movingBack = false;
            }

            //log.DebugFormat ("HandleImmediateKeys: playerAccel = {0}", playerAccel);
#if !AT_MOBILE
            // If mouse2 (button1) is down, left and right are strafe.
            // Otherwise, they are rotation.
            if (ClientAPI.WorldManager.Player.CanMove() && (mouseLookLocked || (rightButtonDown && !mouseOverUI)) && (!dead || state == "spirit"))
            {
                // Apply the left and right as strafe
                if (movement[(int)MoveEnum.Left])
                    playerAccel.x -= 0.5f;
                if (movement[(int)MoveEnum.Right])
                    playerAccel.x += 0.5f;

                if (ClientAPI.WorldManager.Player.MobController.RotatingDirection != 0)
                {
                    ClientAPI.WorldManager.SendPlayerRotationDiration(0);
                    ClientAPI.WorldManager.Player.MobController.RotatingDirection = 0;
                }
            }
            else
            {
                float rotationDirection = 0;
                if (ClientAPI.WorldManager.Player.CanTurn() && (!dead || state == "spirit"))
                {
                    // Apply the left and right as rotate
                    if (movement[(int)MoveEnum.Left])
                    {
                        rotationDirection = -1;
                    }
                    //this.PlayerYaw -= rotateSpeed * timeSinceLastFrame;
                    if (movement[(int)MoveEnum.Right])
                    {
                        rotationDirection = 1;
                    }

                    if (rotationDirection != 0)
                    {
                        target.RotateAround(Vector3.up, rotateSpeed * rotationDirection * timeSinceLastFrame);
                      //  target.Rotate(Vector3.up, rotateSpeed * rotationDirection * timeSinceLastFrame);
                    }
                    //this.PlayerYaw += rotateSpeed * timeSinceLastFrame;
                }
                if (rotateSpeed * rotationDirection != ClientAPI.WorldManager.Player.MobController.RotatingDirection)
                {
                    ClientAPI.WorldManager.SendPlayerRotationDiration(rotateSpeed * rotationDirection);
                    ClientAPI.WorldManager.Player.MobController.RotatingDirection = rotateSpeed * rotationDirection;
                }

                // If the left mouse is not down, rotate the camera, but 
                // otherwise, leave it alone
                if (!leftButtonDown && cameraFollowChar)
                {
                    Camera camera = Camera.main;
                    if (movement[(int)MoveEnum.Left])
                        camera.transform.RotateAround(Vector3.up, -rotateSpeed * timeSinceLastFrame);
                       // camera.transform.Rotate(Vector3.up, -rotateSpeed * timeSinceLastFrame);
                    //this.CameraYaw -= rotateSpeed * timeSinceLastFrame;
                    if (movement[(int)MoveEnum.Right])
                        camera.transform.RotateAround(Vector3.up, rotateSpeed * timeSinceLastFrame);
                     //      camera.transform.Rotate(Vector3.up, rotateSpeed * timeSinceLastFrame);
                    //this.CameraYaw += rotateSpeed * timeSinceLastFrame;
                    // Make a smooth transition back to being behind the player over approximately 1.5 seconds for a full 180 change
                    if (playerAccel == Vector3.zero)
                        return;
                    float targetYaw = PlayerYaw;
                    if (Mathf.Abs(this.CameraYaw - targetYaw) > 1)
                    {
                        float difference = this.CameraYaw - targetYaw;
                        if (difference > 360)
                            difference -= 360;
                        float alteration = timeSinceLastFrame * 120; // 120 degrees per second
                        if (Mathf.Abs(difference) > alteration)
                        {
                            if ((difference > 0 && difference < 180) || difference < -180)
                                this.CameraYaw = this.CameraYaw - alteration;
                            else
                                this.CameraYaw = this.CameraYaw + alteration;
                            if (AtavismLogger.logLevel <= LogLevel.Info)
                                AtavismLogger.LogInfoMessage("Difference between camera and player is: " + difference + " with alteration: " + alteration + " and playerYaw : " + PlayerYaw + " and targetYaw: " + targetYaw);
                        }
                        else
                        {
                            this.CameraYaw = targetYaw;
                        }
                    }
                    // Also try move camera to the ideal pitch
                    if (Mathf.Abs(this.CameraPitch - idealPitch) > 1)
                    {
                        float difference = this.CameraPitch - idealPitch;
                        if (difference > 360)
                            difference -= 360;
                        float alteration = timeSinceLastFrame * 40; // 120 degrees per second
                        if (Mathf.Abs(difference) > alteration)
                        {
                            if ((difference > 0 && difference < 180) || difference < -180)
                                this.CameraPitch = this.CameraPitch - alteration;
                            else
                                this.CameraPitch = this.CameraPitch + alteration;
                            if (AtavismLogger.logLevel <= LogLevel.Info)
                                AtavismLogger.LogInfoMessage("Difference between camera and player is: " + difference + " with alteration: " + alteration + " and playerYaw : " + PlayerYaw + " and targetYaw: " + targetYaw);
                        }
                        else
                        {
                            this.CameraPitch = idealPitch;
                        }
                    }
                }
            }
#endif
        }

        #endregion Movement

        #region Camera

        /// <summary>
        ///   This is called from the MouseMoved handler.  It updates the 
        ///   cameraPitch, cameraYaw and cameraDist variables (which are 
        ///   later used to modify the camera).  It may also update the 
        ///   PlayerYaw.
        /// </summary>
        /// <param name="e"></param>
        protected void UpdateCamera()
        {
            UpdateCursorVisibility(leftButtonDown || rightButtonDown);
            if (rightButtonDown)
            {
                mouseRightButtonClicked = true;
                cameraFollowChar = true;
            }
            if (leftButtonDown && playerAccel == Vector3.zero)
            {
                mouseLeftButtonClicked = true;
                cameraFollowChar = false;
            }
            if (!cameraFollowChar && !leftButtonDown && !AtavismSettings.Instance.GetGeneralSettings().freeCamera)
            {
                // mouseRightButtonClicked = true;
                cameraFollowChar = true;
            }
            //if (playerAccel != Vector3.zero &&  !leftButtonDown)
            if (rotateCharacterToCameraView || (playerAccel != Vector3.zero && ((mouseRightButtonClicked && !rightButtonDown) || (mouseLeftButtonClicked && !leftButtonDown))))
            {
                mouseRightButtonClicked = false;
                mouseLeftButtonClicked = false;
                //     Quaternion q = Quaternion.AngleAxis(Mathf.PI, Vector3.up);
#if !AT_MOBILE
                target.rotation = Camera.main.transform.rotation;// * q;
#endif
            }

            
                if ((!AtavismSettings.Instance.isMenuBarOpened && !AtavismSettings.Instance.isWindowOpened() && blockFollowMouseWhenWindowOpened && mouseLookLocked) ||
                    (!blockFollowMouseWhenWindowOpened && mouseLookLocked)
                    || rightButtonDown || 
                    (!AtavismSettings.Instance.isWindowOpened() && !AtavismSettings.Instance.isMenuBarOpened && blockFollowMouseWhenWindowOpened && cameraFollowMouse) ||
                    (!blockFollowMouseWhenWindowOpened && cameraFollowMouse))
                {
                    // If they are holding down the right mouse button, 
                    // rotate both the camera and the player
                    float mouseX = Input.GetAxis("Mouse X");
                    float mouseY = Input.GetAxis("Mouse Y");
#if !AT_MOBILE
                    ApplyMouseToCamera(mouseX, mouseY);
#endif
                    // Whenever we use second mouse button or movement keys to 
                    // rotate, reset the player's orientation to match the camera
                    // Since the player model's default orientation is facing the 
                    // camera, spin the camera an extra 180 degrees.
                    if ( /*(mouseX != 0 || mouseY != 0) && */playerAccel != Vector3.zero)
                    {
                        //    Quaternion q = Quaternion.AngleAxis(Mathf.PI, Vector3.up);
#if !AT_MOBILE
                        if (ClientAPI.GetPlayerObject().MobController.MovementState == 1)
                            target.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
                        else
                            target.rotation = Camera.main.transform.rotation; // * q;
#endif
                    }

                    //PlayerYaw = CameraYaw; 
                    if (AtavismLogger.logLevel <= LogLevel.Info)
                        AtavismLogger.LogInfoMessage("Setting playerYaw to: " + PlayerYaw + " with cameraYaw: " + CameraYaw);
                }
                else if (leftButtonDown /*&& !mouseOverUI*/)
                {
                    // If they are holding down the left mouse button, 
                    // rotate the camera around the player.
#if !AT_MOBILE
                    ApplyMouseToCamera(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif
                }
            
            // If the mob is to follow the terrain, set their rotation to 0 for x and z axes
            // or is the right mouse button is not down
#if !AT_MOBILE            
            if (ClientAPI.GetPlayerObject() != null)
                if (ClientAPI.GetPlayerObject().MobController.FollowTerrain || !rightButtonDown)
                    target.rotation = Quaternion.Euler(0, PlayerYaw, 0);
#endif
            // Set the lower bound on the treatment of camera distance so we
            // act as though we are at least 10cm awway.
            float mult = Mathf.Max(.1f, cameraDist);
            // a non-linear distance transform here for the scroll wheel
            if (!mouseOverUI && !mouseWheelDisabled)
            {
#if AT_MOBILE
                float d = MouseWheelVelocity * mult * GetZoomValue();
#else
                float d = MouseWheelVelocity * mult * Input.GetAxis("Mouse ScrollWheel");
                if (AtavismLogger.logLevel <= LogLevel.Info)
                    AtavismLogger.LogInfoMessage("Mousewheel: " + d + "; with velocity: " + MouseWheelVelocity + " and input: " + Input.GetAxis("Mouse ScrollWheel"));
#endif                
                cameraDist += d;
            }
            if (cameraDist < 0)
                cameraDist = 0;

            // limit the range of camera movement
            cameraDist = Mathf.Min(cameraMaxDist, cameraDist);
            cameraDist = Mathf.Max(cameraMinDist, cameraDist);

            // Check to see if the player should be visible
            //	playerVisible = cameraDist > minPlayerVisibleDistance;
        }
#if AT_MOBILE
        float GetZoomValue() //PopuGames
        {
            if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
            {
                return HandleTouch();
            }
            else
            {
                return Input.GetAxis("Mouse ScrollWheel");
            }
        }

        float HandleTouch() //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////PopuGames
        {
            float offset = 0f;


            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                    {
                        touch0CanZoom = true;
                    }

                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    touch0CanZoom = false;
                }
                else if (touch.phase == TouchPhase.Canceled)
                {
                    touch0CanZoom = false;
                }
            }

            if (Input.touchCount > 1)
            {
                Touch touch1 = Input.GetTouch(1);
                if (touch1.phase == TouchPhase.Began)
                {
                    if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(1).fingerId))
                    {
                        touch1CanZoom = true;
                    }

                }
                else if (touch1.phase == TouchPhase.Ended)
                {
                    touch1CanZoom = false;
                }
                else if (touch1.phase == TouchPhase.Canceled)
                {
                    touch1CanZoom = false;
                }
            }

            if (Input.touchCount > 2)
            {
                Touch touch2 = Input.GetTouch(2);
                if (touch2.phase == TouchPhase.Began)
                {
                    if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(2).fingerId))
                    {
                        touch2CanZoom = true;
                    }

                }
                else if (touch2.phase == TouchPhase.Ended)
                {
                    touch2CanZoom = false;
                }
                else if (touch2.phase == TouchPhase.Canceled)
                {
                    touch2CanZoom = false;
                }
            }



            if (Input.touchCount <= 0)
            {
                touch0CanZoom = false;
                touch1CanZoom = false;
                touch2CanZoom = false;
                canZoom = true;
            }

            if (canZoom)
            {
                switch (Input.touchCount)
                {
                    case 2: // Zooming

                        if (touch0CanZoom && touch1CanZoom)
                        {
                            Vector2[] newPositions = new Vector2[] {Input.GetTouch(0).position, Input.GetTouch(1).position};
                            if (!wasZoomingLastFrame)
                            {
                                lastZoomPositions = newPositions;
                                wasZoomingLastFrame = true;
                            }
                            else
                            {
                                // Zoom based on the distance between the new positions compared to the 
                                // distance between the previous positions.
                                float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                                float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                                offset = newDistance - oldDistance;
                                lastZoomPositions = newPositions;
                            }
                        }

                        break;
                    case 3: // Zooming

                        if (touch0CanZoom && touch1CanZoom)
                        {
                            Vector2[] newPositions = new Vector2[] {Input.GetTouch(0).position, Input.GetTouch(1).position};
                            if (!wasZoomingLastFrame)
                            {
                                lastZoomPositions = newPositions;
                                wasZoomingLastFrame = true;
                            }
                            else
                            {
                                // Zoom based on the distance between the new positions compared to the 
                                // distance between the previous positions.
                                float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                                float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                                offset = newDistance - oldDistance;
                                lastZoomPositions = newPositions;
                            }
                        }
                        else if (touch0CanZoom && touch2CanZoom)
                        {
                            Vector2[] newPositions = new Vector2[] {Input.GetTouch(0).position, Input.GetTouch(2).position};
                            if (!wasZoomingLastFrame)
                            {
                                lastZoomPositions = newPositions;
                                wasZoomingLastFrame = true;
                            }
                            else
                            {
                                // Zoom based on the distance between the new positions compared to the 
                                // distance between the previous positions.
                                float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                                float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                                offset = newDistance - oldDistance;
                                lastZoomPositions = newPositions;
                            }
                        }
                        else if (touch1CanZoom && touch2CanZoom)
                        {
                            Vector2[] newPositions = new Vector2[] {Input.GetTouch(1).position, Input.GetTouch(2).position};
                            if (!wasZoomingLastFrame)
                            {
                                lastZoomPositions = newPositions;
                                wasZoomingLastFrame = true;
                            }
                            else
                            {
                                // Zoom based on the distance between the new positions compared to the 
                                // distance between the previous positions.
                                float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                                float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                                offset = newDistance - oldDistance;
                                lastZoomPositions = newPositions;
                            }
                        }

                        break;

                    default:
                        wasZoomingLastFrame = false;
                        break;

                }
            }


            if (canZoom)
            {
                offset = offset / 500;
            }

            if (touch0CanZoom && touch1CanZoom || touch0CanZoom && touch2CanZoom || touch1CanZoom && touch2CanZoom)
            {
                transform.parent.GetComponent<VertPivot_Sphere>().isOnZoom = true;
                transform.parent.parent.GetComponent<HoriPivot>().isOnZoom = true;
            }
            else
            {
                transform.parent.GetComponent<VertPivot_Sphere>().isOnZoom = false;
                transform.parent.parent.GetComponent<HoriPivot>().isOnZoom = false;

            }

            return offset;
        }


        void UpdateSmoothedMovementDirection() // PopuGames    move the player in the joystik direction
        {
            //reading the input:
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");

            Transform cameraTransform = Camera.main.transform;

            // Forward vector relative to the camera along the x-z plane   
            Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
            forward.y = 0;
            forward = forward.normalized;
            // Right vector relative to the camera
            // Always orthogonal to the forward vector
            Vector3 right = new Vector3(forward.z, 0, -forward.x);

            // Target direction relative to the camera
            Vector3 targetDirection = h * right + v * forward;

            // We store speed and direction seperately,
            // so that when the character stands still we still have a valid forward direction
            // moveDirection is always normalized, and we only update it if there is user input.
            if (targetDirection != Vector3.zero)
            {
                moveDirection = Vector3.Lerp(moveDirection, targetDirection, 3f * rotationSpeed * Time.deltaTime);
                moveDirection = moveDirection.normalized;
            }
            //  moveSpeed = Mathf.Lerp(moveSpeed, targetDirection.magnitude * 3, 1);

            // if (h != 0  || v != 0 )
            // {

            //     transform.Translate(Vector3.forward * 3 * Time.deltaTime);


            // agent.SetDestination(moveDirection);
            //  }


            // Set rotation to the move direction  
            target.rotation = Quaternion.LookRotation(moveDirection);

        }


#endif
        /// <summary>
        ///   Use the information from the mouse movement to update the camera.
        /// </summary>
        /// <param name="x">the relative x offset of the mouse (in mouse units)</param>
        /// <param name="y">the relative y offset of the mouse (in mouse units)</param>
        protected void ApplyMouseToCamera(float x, float y)
        {
#if AT_MOBILE            
            if (!_activated)
            {
#endif
                if (cameraGrabbed)
                    return;
                if (AtavismSettings.Instance.GetGeneralSettings().invertMouse)
                    y = -y;
                Camera camera = Camera.main;
                camera.transform.RotateAround(Vector3.up, x * MouseVelocity);
                // camera.transform.Rotate(Vector3.up, x * MouseVelocity);
                //this.CameraYaw += x * MouseVelocity;
                //float cameraPitch = camera.transform.eulerAngles.x;
                float cameraPitch = this.CameraPitch;
                cameraPitch -= y * MouseVelocity * 100;
                if (cameraPitch < 0)
                    cameraPitch = 360 + cameraPitch;
                if (AtavismLogger.logLevel <= LogLevel.Info)
                    AtavismLogger.LogInfoMessage("Camera pitch: " + CameraPitch + " and new pitch: " + cameraPitch);
                idealPitch = cameraPitch;
                CameraPitch = cameraPitch;
#if AT_MOBILE
            }
#endif
        }

        protected void UpdateCursorVisibility(bool mouseButtonDown)
        {
            if (cameraGrabbed)
                return;
            if (mouseOverUI)
            {
                if (AtavismLogger.logLevel <= LogLevel.Info)
                    AtavismLogger.LogInfoMessage("Mouse over UI");
                if (!mouseButtonDown)
                    if (!mouseLookLocked)
                    {
                        Cursor.lockState = CursorLockMode.None;
                    }
                //return;
            }
            else if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                mousePosition = UnityEngine.Input.mousePosition;
                //Screen.lockCursor = true;
                if (!mouseLookLocked)
                {
                    ClientAPI.mouseLook = true;
                }
            }

            if (!mouseButtonDown)
            {
                if (!mouseLookLocked)
                {
                    Cursor.lockState = CursorLockMode.None;
                    ClientAPI.mouseLook = false;
                }
            }
        }

        /// <summary>
        ///   Move the camera based on the new position of the camera target (player)
        /// </summary>
        /// <param name="playerPos"></param>
        /// <param name="playerOrient"></param>
        protected void UpdateCamera(Vector3 playerPos, Quaternion playerOrient)
        {
            // Convenience to avoid calling client.Camera everywhere
            Camera camera = Camera.main;

            Vector3 cameraDir = camera.transform.rotation * Vector3.back;
            if (AtavismLogger.logLevel <= LogLevel.Info)
                AtavismLogger.LogInfoMessage("Camera rotation1 :" + camera.transform.rotation);

            // Look at a point that is above the player's base - this should ideally be
            // around the character's head.
            Vector3 cameraTarget = playerPos + playerOrient * cameraTargetOffset;

            
            if (cameraGrabbed)
            {
                // if the camera is grabbed, then dont do anything
            }
            else if (cameraFree)
            {
                // If the camera is free, just set the position and direction
                camera.transform.position = cameraPosition;
                camera.transform.forward = -cameraDir;
                cameraDist = (playerPos - cameraPosition).magnitude;
                //client.Player.SceneNode.IsVisible = cameraDist > minThirdPersonDistance;
                //log.DebugFormat ("Camera is free; cameraPosition = {0}, cameraOrientation = {1}",
                //                    cameraPosition, cameraOrientation.EulerString);
            }
            else
            {
               
                // Put the camera cameraDist behind the player
                Vector3 cameraPos = cameraTarget + cameraDir *(zoomActive ? zoomCameraDist : cameraDist);
//                Vector3 cameraPos = Vector3.Lerp(camera.transform.position,cameraTarget + cameraDir * cameraDist, Time.deltaTime);

                
                Vector3 targetDir = (cameraPos - cameraTarget).normalized;
                float len = FindAcceptableCameraPosition(camera, cameraPos, cameraTarget, targetDir);
                forcePower = Vector3.Lerp(forcePower, Vector3.zero, Time.deltaTime * 5);
                Vector3 cameraChange = new Vector3(Mathf.Cos(Time.time * 80) * forcePower.x, Mathf.Cos(Time.time * 80) * forcePower.y, Mathf.Cos(Time.time * 80) * forcePower.z);

                // The player is visible if the camera is further
                // than the minimum player visible distance
                //       playerVisible = len > minPlayerVisibleDistance;
                // Record if we were first person last frame
                //	bool formerlyFirstPerson = cameraFirstPerson;
                // We shift to first-person mode if the distance
                // is less than the minimum third-person distance
                cameraFirstPerson = len < minThirdPersonDistance;
                // Set the camera position to the target if we're
                // in first person mode, else use the calculated
                // position.
                camera.transform.position = (cameraFirstPerson ? cameraTarget : cameraTarget + cameraDir * len);
                camera.transform.localPosition = camera.transform.localPosition + cameraChange;

                if (cameraFirstPerson)
                    camera.transform.TransformDirection(-cameraDir);
                else
                {
                    camera.transform.LookAt(cameraTarget);
                    camera.transform.position = camera.transform.position + camera.transform.rotation * cameraTargetCenterOffset;
                }

                if (AtavismLogger.logLevel <= LogLevel.Info)
                    AtavismLogger.LogInfoMessage("Camera rotation2 :" + camera.transform.rotation);

                //client.Player.SceneNode.IsVisible = playerVisible;
            }
        }

        protected float FindAcceptableCameraPosition(Camera camera, Vector3 cameraPos, Vector3 cameraTarget, Vector3 cameraDir)
        {
            float len = (cameraPos - cameraTarget).magnitude;
            float alen = (camera.transform.position - cameraPos).magnitude;
           

            // Send a ray from the players head in the direction of the camera
           // Vector3 intersection = Vector3.zero;
            Vector3 newCameraPos = cameraPos;
         //   Vector3 unitTowardCamera = (newCameraPos - cameraTarget).normalized;
            Ray ray = new Ray(cameraTarget, cameraDir);
            RaycastHit hit;
            if (Physics.SphereCast(ray, 0.25f, out hit, len, obstacleLayers | groundLayers))
            {
                if (AtavismLogger.logLevel <= LogLevel.Info)
                    AtavismLogger.LogInfoMessage("Camera ray hit object with distance: " + hit.distance);
                if (hit.distance < len)
                    return hit.distance;
            }

            float nLen = len;
            if (Mathf.Abs(alen - len) > 0.2f)
            {
               // Debug.LogError("FindAcceptableCameraPosition "+len+" "+alen+" cameraPos="+cameraPos+" cameraTarget="+cameraTarget+" cameraDir="+cameraDir);
               // nLen = Mathf.Lerp(alen, len, Time.deltaTime*3f);
            }

            return nLen;
        }

        public bool IsMouseLook()
        {
            return mouseLookLocked;
        }

        #endregion Camera

        public float CameraPitch
        {
            get
            {
                float pitch;
                Camera camera = Camera.main;
                pitch = camera.transform.rotation.eulerAngles.x;
                return pitch;
            }
            set
            {
                Camera camera = Camera.main;
                Vector3 pitchYawRoll = camera.transform.eulerAngles;
                if (value > 180 && minPitch > 180 && value < minPitch)
                    value = minPitch;
                if (value < 90 && minPitch < 90  && value < minPitch)
                    value = minPitch;
                if (value < 180 && value > maxPitch)
                    value = maxPitch;
                pitchYawRoll.x = value;
                camera.transform.eulerAngles = pitchYawRoll;
            }
        }

        public float CameraYaw
        {
            get
            {
                float yaw;
                Camera camera = Camera.main;
                yaw = camera.transform.rotation.eulerAngles.y;
                //yaw = cameraOrientation.eulerAngles.y;
                return yaw;
            }
            set
            {
                Camera camera = Camera.main;
                Vector3 pitchYawRoll = camera.transform.eulerAngles;
                pitchYawRoll.y = value;
                camera.transform.eulerAngles = pitchYawRoll;
                camera .transform.position = camera .transform.position + Quaternion.Euler(0f, pitchYawRoll.y, 0f) * Vector3.forward+ new Vector3(2,0,0) 
                        ;
            }
        }

        public float PlayerYaw
        {
            get
            {
                float yaw;
                yaw = target.rotation.eulerAngles.y;
                return yaw;
            }
            set
            {
                Camera camera = Camera.main;
                Vector3 pitchYawRoll = target.eulerAngles;
                pitchYawRoll.y = value;
                target.eulerAngles = pitchYawRoll;
            }
        }

        public bool IsControllable
        {
            get
            {
                return isControllable;
            }
            set
            {
                isControllable = value;
            }
        }
        public bool IsMovingBack
        {
            get
            {
                return movingBack;
            }

        }
        void OnApplicationFocus(bool focusStatus)
        {
            if (focusStatus == false)
            {
                MoveForward(false);
                MoveBackward(false);
                StrafeLeft(false);
                StrafeRight(false);
                TurnLeft(false);
                TurnRight(false);
            }
        }
        public void Shake(Vector3 power)
        {
            forcePower = -power;
        }
        public void NoMove(bool value)
        {
            noMove = value;
        }
#if AT_MOBILE
        Vector3 moveToPosition()
        {
            if (clickedTarget != null)
            {
                position = clickedTarget.transform.position;
                target.LookAt(clickedTarget.transform);
                Quaternion newRotation = target.rotation;
                newRotation.x = 0f;
                newRotation.z = 0f;
                target.rotation = newRotation;
            }

            //Game Object is moving
            if (Vector3.Distance(target.position, position) > (distanceToStopAt + 0.5f))
            {
                Quaternion newRotation = Quaternion.LookRotation(position - target.position);

                newRotation.x = 0f;
                newRotation.z = 0f;

                target.rotation = newRotation; //Quaternion.Slerp(target.rotation, newRotation, Time.deltaTime * 10);
                return target.forward;
            }
            //Game Object is not moving
            else
            {
                if (clickedTarget != null && !sentStrike)
                {
                    ActivateTarget(clickedTarget);
                }

                return Vector3.zero;
            }
        }

        void ActivateTarget(GameObject tempTarget)
        {
            if (tempTarget.GetComponent<AtavismNode>() != null
                && tempTarget.GetComponent<AtavismNode>().CheckBooleanProperty("attackable"))
            {
                // Send strike command
                NetworkAPI.SendAttackMessage(tempTarget.GetComponent<AtavismNode>().Oid, "strike", true);
                sentStrike = true;
            }
            else if (tempTarget.GetComponent<ClickToMoveTargetableObject>().activateTarget != null)
            {
                GameObject activateTarget = tempTarget.GetComponent<ClickToMoveTargetableObject>().activateTarget;
                if (activateTarget.GetComponent<ResourceNode>() != null)
                {
                    if (activateTime == -1)
                    {
                        activateTime = Time.time + harvestDelay;
                    }
                    else if (Time.time > activateTime)
                    {
                        activateTarget.GetComponent<ResourceNode>().HarvestResource();
                        sentStrike = true;
                        activateTime = -1f;
                    }
                }
                else if (activateTarget.GetComponent<CraftingStation>() != null)
                {
                    if (activateTime == -1)
                    {
                        activateTime = Time.time + harvestDelay;
                    }
                    else if (Time.time > activateTime)
                    {
                        activateTarget.GetComponent<CraftingStation>().ActivateCraftingStation();
                        sentStrike = true;
                        activateTime = -1f;
                    }
                }
            }
        }

        void LocatePosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000, ~ignoreLayers))
            {
                if (hit.collider.GetComponent<ClickToMoveTargetableObject>())
                {
                    GameObject tempTarget = hit.collider.gameObject;
                    float distanceToStop = tempTarget.GetComponent<ClickToMoveTargetableObject>().distanceToStopAt;
                    if (Vector3.Distance(target.position, hit.transform.position) > distanceToStop + 0.25f)
                    {
                        return;
                    }
                }

                clickedTarget = null;
                distanceToStopAt = 0;
                ClientAPI.SetTarget(-1);
                NetworkAPI.SendAttackMessage(ClientAPI.GetTargetOid(), "strike", false);

                if (hit.collider.tag != "Player" && hit.collider.tag != "Enemy" && IsInLayerMask(hit.collider.gameObject, layerMask)
                    || clickedTarget != null)
                {
                    position = hit.point;
                }
            }
        }

        void LocateTarget()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000, ~ignoreLayers))
            {
                // Make sure the target is far enough away to care
                if (hit.collider.GetComponent<ClickToMoveTargetableObject>() && hit.collider.gameObject != ClientAPI.GetPlayerObject().GameObject)
                {
                    float distanceToStop = hit.collider.gameObject.GetComponent<ClickToMoveTargetableObject>().distanceToStopAt;
                    TargetFound(hit.collider.gameObject, distanceToStop);
                }
                else
                {
                    // Check if there is a mob within 2m of the hit 
                    foreach (Collider col in Physics.OverlapSphere(hit.point, targetRadius, ~ignoreLayers))
                    {
                        if (col.gameObject != ClientAPI.GetPlayerObject().GameObject && col.gameObject.GetComponent<AtavismNode>() != null
                                                                                     && col.gameObject.GetComponent<AtavismNode>().GetProperty("attackable") != null)
                        {
                            float distanceToStop = GetDistanceToMob(col.gameObject);
                            TargetFound(col.gameObject, distanceToStop);
                            return;
                        }
                    }

                    clickedTarget = null;
                    distanceToStopAt = 0;
                }
            }
        }

        void TargetFound(GameObject newTarget, float distanceToStop)
        {
            if (newTarget.GetComponent<AtavismNode>() != null && newTarget.GetComponent<AtavismNode>().GetProperty("attackable") != null)
            {
                distanceToStop = GetDistanceToMob(newTarget);
                ClientAPI.SetTarget(newTarget.GetComponent<AtavismNode>().Oid);
            }

            if (Vector3.Distance(target.position, newTarget.transform.position) > distanceToStop + 0.25f)
            {
                clickedTarget = newTarget;
                distanceToStopAt = distanceToStop;
                sentStrike = false;
                activateTime = -1f;
            }
            else
            {
                sentStrike = false;
                activateTime = 1; // Set to 1 so it will trigger right away
                ActivateTarget(newTarget);
                position = target.position;
            }
        }

        private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
        {
            // Convert the object's layer to a bitfield for comparison
            int objLayerMask = (1 << obj.layer);
            if ((layerMask.value & objLayerMask) > 0) // Extra round brackets required!
                return true;
            else
                return false;
        }

        float GetDistanceToMob(GameObject target)
        {
            // Is the target attackable?
            if ((int) target.GetComponent<AtavismNode>().GetProperty("targetType") < 1
                && (bool) target.GetComponent<AtavismNode>().GetProperty("attackable"))
            {
                // Work out range of auto attack ability
                int autoAbilityID = (int) ClientAPI.GetPlayerObject().GetProperty("combat.autoability");
                int distance = Abilities.Instance.GetAbility(autoAbilityID).distance;
                if (distance > 4)
                {
                    return distance;
                }
                else
                {
                    return target.GetComponent<ClickToMoveTargetableObject>().distanceToStopAt;
                }
            }
            else
            {
                // Go to the target
                return target.GetComponent<ClickToMoveTargetableObject>().distanceToStopAt;
            }
        }

#endif
    }
}