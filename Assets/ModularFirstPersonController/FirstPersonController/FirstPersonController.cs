// CHANGE LOG
// 
// CHANGES || version VERSION
//
// "Enable/Disable Headbob, Changed look rotations - should result in reduced camera jitters" || version 1.0.1
// "Removed crouching mechanic and added time slow feature with limited duration" || version 1.0.2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static WeaponAnim;

#if UNITY_EDITOR
using UnityEditor;
using System.Net;
#endif

public class FirstPersonController : MonoBehaviour
{
    private Rigidbody rb;
    private WeaponAnim wp;

    #region Camera Movement Variables

    private float recoilAmount = 0f;
    private float baseRecoilAmount = 0f; // Amount of pitch increase for recoil
    private float recoilResetSpeed = 50f; // Speed at which the pitch resets after recoil

    private float recoilTargetPitch; // Target pitch after applying recoil
    private bool isRecoiling = false; // Flag to check if recoil is active

    public Camera playerCamera;

    public float fov = 60f;
    public bool invertCamera = false;
    public bool cameraCanMove = true;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 50f;

    // Crosshair
    public bool lockCursor = true;
    public bool crosshair = true;
    public Sprite crosshairImage;
    public Color crosshairColor = Color.white;

    // Internal Variables
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private Image crosshairObject;

    #region Camera Zoom Variables

    public bool enableZoom = true;
    public bool holdToZoom = false;
    public KeyCode zoomKey = KeyCode.Mouse1;
    public float zoomFOV = 30f;
    public float zoomStepTime = 5f;

    // Internal Variables
    private bool isZoomed = false;
    private bool lastIsAiming = false;
    private float rifleZoom = 0f;

    #endregion
    #endregion

    #region Movement Variables

    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 10f;
    public float speedReduction = .5f;

    // Internal Variables
    public bool isWalking = false;

    // Coyote time variables
    public float coyoteTime = 0.2f; // Time allowed after leaving ground to still jump
    private float coyoteTimer = 0f; // Timer to track coyote time

    #region Sprint

    public bool enableSprint = true;
    public bool unlimitedSprint = false;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintSpeed = 7f;
    public float sprintDuration = 5f;
    public float sprintCooldown = .5f;
    public float sprintFOV = 80f;
    public float sprintFOVStepTime = 10f;

    // Sprint Bar
    public bool useSprintBar = true;
    public bool hideBarWhenFull = true;
    public Image sprintBarBG;
    public Image sprintBar;
    public float sprintBarWidthPercent = .3f;
    public float sprintBarHeightPercent = .015f;

    // Internal Variables
    private CanvasGroup sprintBarCG;
    public bool isSprinting = false;
    private float sprintRemaining;
    private float sprintBarWidth;
    private float sprintBarHeight;
    private bool isSprintCooldown = false;
    private float sprintCooldownReset;

    #endregion

    #region Jump

    public bool enableJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;

    // Internal Variables
    public bool isGrounded = false;

    #endregion

    #endregion

    #region Time Slow

    public bool enableTimeSlow = true;
    public KeyCode timeSlowKey = KeyCode.LeftControl; // Use the crouch key for time slow
    public float timeSlowDuration = 5f; // Duration for which time slow lasts
    public float timeSlowCooldown = 2f; // Cooldown when time slow bar is empty
    public float timeSlowRegenerateRate = 1f; // Rate at which time slow regenerates
    public Image timeSlowFillImage; // Image to display time slow bar

    // Internal Variables
    private bool isTimeSlowed = false;
    private float timeSlowRemaining;
    private bool isTimeSlowCooldown = false;

    #endregion

    #region Head Bob

    public bool enableHeadBob = true;
    public Transform joint;
    public float bobSpeed = 10f;
    public Vector3 bobAmount = new Vector3(.15f, .05f, 0f);

    // Internal Variables
    private Vector3 jointOriginalPos;
    private float timer = 0;

    public HealthBar healthBar;

    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        crosshairObject = GetComponentInChildren<Image>();

        // Set internal variables
        playerCamera.fieldOfView = fov;
        jointOriginalPos = joint.localPosition;

        timeSlowRemaining = timeSlowDuration; // Initialize time slow remaining
    }

    void Start()
    {
        healthBar = FindObjectOfType<HealthBar>(); // Get reference to HealthBar
    

        wp = FindObjectOfType<WeaponAnim>();
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (crosshair)
        {
            crosshairObject.sprite = crosshairImage;
            crosshairObject.color = crosshairColor;
        }
        else
        {
            crosshairObject.gameObject.SetActive(false);
        }

        #region Sprint Bar

        sprintBarCG = GetComponentInChildren<CanvasGroup>();

        if (useSprintBar)
        {
            sprintBarBG.gameObject.SetActive(true);
            sprintBar.gameObject.SetActive(true);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            sprintBarWidth = screenWidth * sprintBarWidthPercent;
            sprintBarHeight = screenHeight * sprintBarHeightPercent;

            sprintBarBG.rectTransform.sizeDelta = new Vector3(sprintBarWidth, sprintBarHeight, 0f);
            sprintBar.rectTransform.sizeDelta = new Vector3(sprintBarWidth - 2, sprintBarHeight - 2, 0f);

            if (hideBarWhenFull)
            {
                sprintBarCG.alpha = 0;
            }
        }
        else
        {
            sprintBarBG.gameObject.SetActive(false);
            sprintBar.gameObject.SetActive(false);
        }

        #endregion
    }

    float camRotation;

    private void Update()
    {
        #region Camera
        // Control camera movement
        if (cameraCanMove)
        {
            // Control yaw and pitch
            yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;

            if (!invertCamera)
            {
                pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");
            }
            else
            {
                pitch += mouseSensitivity * Input.GetAxis("Mouse Y");
            }

            // Clamp pitch and apply recoil effect
            pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);
            if (isRecoiling)
            {
                // Lerp pitch to recoil target
                pitch = Mathf.Lerp(pitch, recoilTargetPitch, recoilResetSpeed * Time.deltaTime);

                // Check if pitch has reached the target and stop recoil if it has
                if (Mathf.Abs(pitch - recoilTargetPitch) < 0.1f)
                {
                    isRecoiling = false;
                    pitch = recoilTargetPitch;
                }
            }

            // Set rotation for player and camera
            transform.localEulerAngles = new Vector3(0, yaw, 0);
            playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        }

        #region Camera Zoom

        if (enableZoom)
        {
            // Changes isZoomed when key is pressed
            // Behavior for toggle zoom
            if (Input.GetKeyDown(zoomKey) && !holdToZoom && !wp.isKatanaActive)
            {
                if (!isZoomed)
                {
                    isZoomed = true;
                    isSprinting = false;
                }
                else
                {
                    isZoomed = false;
                }
            }

            // Changes isZoomed when key is pressed
            // Behavior for hold to zoom
            if (holdToZoom && !wp.isKatanaActive)
            {
                if (Input.GetKeyDown(zoomKey))
                {
                    isZoomed = true;
                    isSprinting = false;
                }
                else if (Input.GetKeyUp(zoomKey))
                {
                    isZoomed = false;
                }
            }

            // Lerps camera.fieldOfView to allow for a smooth transition
            if (isZoomed)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, zoomFOV - rifleZoom, zoomStepTime * Time.deltaTime);
            }
            else if (!isZoomed && !isSprinting)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, zoomStepTime * Time.deltaTime);
            }
        }

        #endregion
        #endregion

        #region Sprint

        if (enableSprint)
        {
            if (isSprinting && !wp.isAiming)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, sprintFOVStepTime * Time.deltaTime);

                // Drain sprint remaining while sprinting
                if (!unlimitedSprint)
                {
                    sprintRemaining -= 1 * Time.deltaTime;
                    if (sprintRemaining <= 0)
                    {
                        isSprinting = false;
                        isSprintCooldown = true;
                    }
                }
            }
            else
            {
                // Regain sprint while not sprinting
                sprintRemaining = Mathf.Clamp(sprintRemaining += 1 * Time.deltaTime, 0, sprintDuration);
            }

            // Handles sprint cooldown 
            // When sprint remaining == 0 stops sprint ability until hitting cooldown
            if (isSprintCooldown)
            {
                sprintCooldown -= 1 * Time.deltaTime;
                if (sprintCooldown <= 0)
                {
                    isSprintCooldown = false;
                }
            }
            else
            {
                sprintCooldown = sprintCooldownReset;
            }

            // Handles sprintBar 
            if (useSprintBar && !unlimitedSprint)
            {
                float sprintRemainingPercent = sprintRemaining / sprintDuration;
                sprintBar.transform.localScale = new Vector3(sprintRemainingPercent, 1f, 1f);
            }
        }

        #endregion

        #region Jump

        // Gets input and calls jump method
        if (enableJump && Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }

        #endregion

        #region Time Slow 

        if (enableTimeSlow)
        {
            // Toggle time slow on key press
            if (Input.GetKeyDown(timeSlowKey) && !isTimeSlowCooldown)
            {
                isTimeSlowed = !isTimeSlowed; // Toggle state
                Time.timeScale = isTimeSlowed ? 0.5f : 1f; // Set time scale based on state
            }

            // Manage time slow remaining
            if (isTimeSlowed)
            {
                timeSlowRemaining -= Time.deltaTime;

                // Check if time slow duration is exhausted
                if (timeSlowRemaining <= 0)
                {
                    isTimeSlowed = false;
                    Time.timeScale = 1f; // Reset time scale
                    isTimeSlowCooldown = true; // Start cooldown
                }
            }
            else if (!isTimeSlowCooldown)
            {
                // Regenerate time slow while inactive
                timeSlowRemaining = Mathf.Clamp(timeSlowRemaining + timeSlowRegenerateRate * Time.deltaTime, 0, timeSlowDuration);
            }

            // Cooldown management
            if (isTimeSlowCooldown)
            {
                timeSlowCooldown -= Time.deltaTime;

                if (timeSlowCooldown <= 0)
                {
                    isTimeSlowCooldown = false; // Reset cooldown
                    timeSlowCooldown = 2f; // Reset cooldown duration
                }
            }

            // Update the time slow fill image based on remaining time
            if (timeSlowFillImage != null)
            {
                timeSlowFillImage.fillAmount = timeSlowRemaining / timeSlowDuration;
            }
        }

        #endregion

        CheckGround();

        if (enableHeadBob && !wp.isAiming)
        {
            HeadBob();
        }

        if (wp.currentGun == GunType.None)
        {
            baseRecoilAmount = 0; // No recoil if no gun equipped
            rifleZoom = 0;
        }

        if (wp.currentGun == GunType.Pistol)
        {
            baseRecoilAmount = 4; // No recoil if no gun equipped
            rifleZoom = 0;
        }

        if (wp.currentGun == GunType.SMG)
        {
            baseRecoilAmount = 2; // No recoil if no gun equipped
            rifleZoom = 0;
        }

        if (wp.currentGun == GunType.Shotgun)
        {
            baseRecoilAmount = 8; // No recoil if no gun equipped
            rifleZoom = 0;
        }

        if (wp.currentGun == GunType.Rifle)
        {
            baseRecoilAmount = 8; // No recoil if no gun equipped
            rifleZoom = 10;
        }
    }

    void FixedUpdate()
    {
        #region Movement

        if (playerCanMove)
        {
            // Calculate how fast we should be moving
            Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            // Check and update walkSpeed based on aiming state
            if (wp.isAiming != lastIsAiming) // Only update if the aiming state has changed
            {
                if (wp.isAiming)
                {
                    walkSpeed *= speedReduction; // Reduce walk speed when aiming
                }
                else
                {
                    walkSpeed /= speedReduction; // Reset walk speed when not aiming
                }

                // Update the last aiming state
                lastIsAiming = wp.isAiming;
            }

            // Checks if player is walking and isGrounded
            // Will allow head bob
            if ((targetVelocity.x != 0 || targetVelocity.z != 0) && isGrounded)
            {
                isWalking = true;
            }
            else
            {
                isWalking = false;
            }

            // Define a small threshold to treat small velocity changes as zero
            float epsilon = 1e-4f;

            // All movement calculations while sprint is active
            if (enableSprint && Input.GetKey(sprintKey) && sprintRemaining > 0f && !isSprintCooldown && !wp.isAiming && !wp.isShooting && !wp.isAttacking && !wp.isParrying)
            {
                targetVelocity = transform.TransformDirection(targetVelocity) * sprintSpeed;

                // Apply a force that attempts to reach our target velocity
                Vector3 velocity = rb.velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;

                // Only consider sprinting if the velocity change is above the threshold
                if (Mathf.Abs(velocityChange.x) > epsilon || Mathf.Abs(velocityChange.z) > epsilon)
                {
                    isSprinting = true;

                    if (hideBarWhenFull && !unlimitedSprint)
                    {
                        sprintBarCG.alpha += 5 * Time.deltaTime;
                    }
                }

                rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }
            // All movement calculations while walking
            else
            {
                isSprinting = false;

                if (hideBarWhenFull && sprintRemaining == sprintDuration)
                {
                    sprintBarCG.alpha -= 3 * Time.deltaTime;
                }

                targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed;

                // Apply a force that attempts to reach our target velocity
                Vector3 velocity = rb.velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;

                rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }

        }

        #endregion
    }

    public void ApplyRecoil()
    {
        // Determine recoil amount based on conditions
        recoilAmount = baseRecoilAmount;

        if (wp.isAiming)
        {
            recoilAmount *= 0.5f; // Reduce recoil when aiming
        }

        // Set recoil target and enable recoil flag
        recoilTargetPitch = Mathf.Clamp(pitch - recoilAmount, -maxLookAngle, maxLookAngle);
        isRecoiling = true;
    }

    // Sets isGrounded based on a raycast sent straight down from the player object
    // Coyote time variables

    private void CheckGround()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y * .5f), transform.position.z);
        Vector3 direction = transform.TransformDirection(Vector3.down);
        float distance = .75f;

        // Perform the raycast
        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            Debug.DrawRay(origin, direction * distance, Color.red);
            isGrounded = true;
            coyoteTimer = coyoteTime; // Reset coyote timer when grounded
        }
        else
        {
            // If we were grounded but lost contact, start the coyote time countdown
            if (coyoteTimer > 0)
            {
                isGrounded = true; // Keep isGrounded true during the coyote time window
                coyoteTimer -= Time.deltaTime;
            }
            else
            {
                isGrounded = false; // Set isGrounded to false after coyote time expires
            }
        }
    }

    public void ApplyDamage(float damage)
    {   
         healthBar.TakeDamage(damage); // Call TakeDamage whenever the player is hit
    }

    private void Jump()
    {
        // Adds force to the player rigidbody to jump
        if (isGrounded)
        {
            rb.AddForce(0f, jumpPower, 0f, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void HeadBob()
    {
        if (isWalking)
        {
            // Calculates HeadBob speed during sprint
            if (isSprinting)
            {
                timer += Time.deltaTime * (bobSpeed + sprintSpeed);
            }
            // Calculates HeadBob speed during walking
            else
            {
                timer += Time.deltaTime * bobSpeed;
            }
            // Applies HeadBob movement
            joint.localPosition = new Vector3(jointOriginalPos.x + Mathf.Sin(timer) * bobAmount.x, jointOriginalPos.y + Mathf.Sin(timer) * bobAmount.y, jointOriginalPos.z + Mathf.Sin(timer) * bobAmount.z);
        }
        else
        {
            // Resets when player stops moving
            timer = 0;
            joint.localPosition = new Vector3(Mathf.Lerp(joint.localPosition.x, jointOriginalPos.x, Time.deltaTime * bobSpeed), Mathf.Lerp(joint.localPosition.y, jointOriginalPos.y, Time.deltaTime * bobSpeed), Mathf.Lerp(joint.localPosition.z, jointOriginalPos.z, Time.deltaTime * bobSpeed));
        }
    }
}

// Custom Editor
#if UNITY_EDITOR
[CustomEditor(typeof(FirstPersonController)), InitializeOnLoadAttribute]
public class FirstPersonControllerEditor : Editor
{
    FirstPersonController fpc;
    SerializedObject SerFPC;

    private void OnEnable()
    {
        fpc = (FirstPersonController)target;
        SerFPC = new SerializedObject(fpc);
    }

    public override void OnInspectorGUI()
    {
        SerFPC.Update();

        EditorGUILayout.Space();
        GUILayout.Label("Modular First Person Controller", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 16 });
        GUILayout.Label("By Jess Case", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = 12 });
        GUILayout.Label("version 1.0.2", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = 12 });
        EditorGUILayout.Space();

        #region Camera Setup

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Camera Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        fpc.playerCamera = (Camera)EditorGUILayout.ObjectField(new GUIContent("Camera", "Camera attached to the controller."), fpc.playerCamera, typeof(Camera), true);
        fpc.fov = EditorGUILayout.Slider(new GUIContent("Field of View", "The camera’s view angle. Changes the player camera directly."), fpc.fov, fpc.zoomFOV, 179f);
        fpc.cameraCanMove = EditorGUILayout.ToggleLeft(new GUIContent("Enable Camera Rotation", "Determines if the camera is allowed to move."), fpc.cameraCanMove);

        GUI.enabled = fpc.cameraCanMove;
        fpc.invertCamera = EditorGUILayout.ToggleLeft(new GUIContent("Invert Camera Rotation", "Inverts the up and down movement of the camera."), fpc.invertCamera);
        fpc.mouseSensitivity = EditorGUILayout.Slider(new GUIContent("Look Sensitivity", "Determines how sensitive the mouse movement is."), fpc.mouseSensitivity, .1f, 10f);
        fpc.maxLookAngle = EditorGUILayout.Slider(new GUIContent("Max Look Angle", "Determines the max and min angle the player camera is able to look."), fpc.maxLookAngle, 40, 90);
        GUI.enabled = true;

        fpc.lockCursor = EditorGUILayout.ToggleLeft(new GUIContent("Lock and Hide Cursor", "Turns off the cursor visibility and locks it to the middle of the screen."), fpc.lockCursor);

        fpc.crosshair = EditorGUILayout.ToggleLeft(new GUIContent("Auto Crosshair", "Determines if the basic crosshair will be turned on, and sets is to the center of the screen."), fpc.crosshair);

        // Only displays crosshair options if crosshair is enabled
        if (fpc.crosshair)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Crosshair Image", "Sprite to use as the crosshair."));
            fpc.crosshairImage = (Sprite)EditorGUILayout.ObjectField(fpc.crosshairImage, typeof(Sprite), false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            fpc.crosshairColor = EditorGUILayout.ColorField(new GUIContent("Crosshair Color", "Determines the color of the crosshair."), fpc.crosshairColor);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        #region Camera Zoom Setup

        GUILayout.Label("Zoom", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));

        fpc.enableZoom = EditorGUILayout.ToggleLeft(new GUIContent("Enable Zoom", "Determines if the player is able to zoom in while playing."), fpc.enableZoom);

        GUI.enabled = fpc.enableZoom;
        fpc.holdToZoom = EditorGUILayout.ToggleLeft(new GUIContent("Hold to Zoom", "Requires the player to hold the zoom key instead if pressing to zoom and unzoom."), fpc.holdToZoom);
        fpc.zoomKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Zoom Key", "Determines what key is used to zoom."), fpc.zoomKey);
        fpc.zoomFOV = EditorGUILayout.Slider(new GUIContent("Zoom FOV", "Determines the field of view the camera zooms to."), fpc.zoomFOV, .1f, fpc.fov);
        fpc.zoomStepTime = EditorGUILayout.Slider(new GUIContent("Step Time", "Determines how fast the FOV transitions while zooming in."), fpc.zoomStepTime, .1f, 10f);
        GUI.enabled = true;

        #endregion

        #endregion

        #region Movement Setup

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Movement Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        fpc.playerCanMove = EditorGUILayout.ToggleLeft(new GUIContent("Enable Player Movement", "Determines if the player is allowed to move."), fpc.playerCanMove);

        GUI.enabled = fpc.playerCanMove;
        fpc.walkSpeed = EditorGUILayout.Slider(new GUIContent("Walk Speed", "Determines how fast the player will move while walking."), fpc.walkSpeed, .1f, fpc.sprintSpeed);
        GUI.enabled = true;

        EditorGUILayout.Space();

        #region Sprint

        GUILayout.Label("Sprint", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));

        fpc.enableSprint = EditorGUILayout.ToggleLeft(new GUIContent("Enable Sprint", "Determines if the player is allowed to sprint."), fpc.enableSprint);

        GUI.enabled = fpc.enableSprint;
        fpc.unlimitedSprint = EditorGUILayout.ToggleLeft(new GUIContent("Unlimited Sprint", "Determines if 'Sprint Duration' is enabled. Turning this on will allow for unlimited sprint."), fpc.unlimitedSprint);
        fpc.sprintKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Sprint Key", "Determines what key is used to sprint."), fpc.sprintKey);
        fpc.sprintSpeed = EditorGUILayout.Slider(new GUIContent("Sprint Speed", "Determines how fast the player will move while sprinting."), fpc.sprintSpeed, fpc.walkSpeed, 20f);

        fpc.sprintDuration = EditorGUILayout.Slider(new GUIContent("Sprint Duration", "Determines how long the player can sprint while unlimited sprint is disabled."), fpc.sprintDuration, 1f, 20f);
        fpc.sprintCooldown = EditorGUILayout.Slider(new GUIContent("Sprint Cooldown", "Determines how long the recovery time is when the player runs out of sprint."), fpc.sprintCooldown, .1f, fpc.sprintDuration);

        fpc.sprintFOV = EditorGUILayout.Slider(new GUIContent("Sprint FOV", "Determines the field of view the camera changes to while sprinting."), fpc.sprintFOV, fpc.fov, 179f);
        fpc.sprintFOVStepTime = EditorGUILayout.Slider(new GUIContent("Step Time", "Determines how fast the FOV transitions while sprinting."), fpc.sprintFOVStepTime, .1f, 20f);

        fpc.useSprintBar = EditorGUILayout.ToggleLeft(new GUIContent("Use Sprint Bar", "Determines if the default sprint bar will appear on screen."), fpc.useSprintBar);

        // Only displays sprint bar options if sprint bar is enabled
        if (fpc.useSprintBar)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            fpc.hideBarWhenFull = EditorGUILayout.ToggleLeft(new GUIContent("Hide Full Bar", "Hides the sprint bar when sprint duration is full, and fades the bar in when sprinting. Disabling this will leave the bar on screen at all times when the sprint bar is enabled."), fpc.hideBarWhenFull);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Bar BG", "Object to be used as sprint bar background."));
            fpc.sprintBarBG = (Image)EditorGUILayout.ObjectField(fpc.sprintBarBG, typeof(Image), true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Bar", "Object to be used as sprint bar foreground."));
            fpc.sprintBar = (Image)EditorGUILayout.ObjectField(fpc.sprintBar, typeof(Image), true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            fpc.sprintBarWidthPercent = EditorGUILayout.Slider(new GUIContent("Bar Width", "Determines the width of the sprint bar."), fpc.sprintBarWidthPercent, .1f, .5f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            fpc.sprintBarHeightPercent = EditorGUILayout.Slider(new GUIContent("Bar Height", "Determines the height of the sprint bar."), fpc.sprintBarHeightPercent, .001f, .025f);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }
        GUI.enabled = true;

        EditorGUILayout.Space();

        #endregion

        #region Jump

        GUILayout.Label("Jump", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));

        fpc.enableJump = EditorGUILayout.ToggleLeft(new GUIContent("Enable Jump", "Determines if the player is allowed to jump."), fpc.enableJump);

        GUI.enabled = fpc.enableJump;
        fpc.jumpKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Jump Key", "Determines what key is used to jump."), fpc.jumpKey);
        fpc.jumpPower = EditorGUILayout.Slider(new GUIContent("Jump Power", "Determines how high the player will jump."), fpc.jumpPower, .1f, 20f);
        GUI.enabled = true;

        EditorGUILayout.Space();

        #endregion

        #region Time Slow

        GUILayout.Label("Time Slow", new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold,
            fontSize = 13
        }, GUILayout.ExpandWidth(true));

        fpc.enableTimeSlow = EditorGUILayout.ToggleLeft(
            new GUIContent("Enable Time Slow", "Determines if the time slow feature is enabled."),
            fpc.enableTimeSlow
        );

        if (fpc.enableTimeSlow)
        {
            GUI.enabled = true;
            fpc.timeSlowKey = (KeyCode)EditorGUILayout.EnumPopup(
                new GUIContent("Time Slow Key", "Determines what key is used to activate the time slow."),
                fpc.timeSlowKey
            );
            fpc.timeSlowDuration = EditorGUILayout.Slider(
                new GUIContent("Time Slow Duration", "Determines how long the time slow lasts."),
                fpc.timeSlowDuration, 1f, 10f
            );
            fpc.timeSlowCooldown = EditorGUILayout.Slider(
                new GUIContent("Cooldown Time", "Determines how long until the time slow can be used again after depleting."),
                fpc.timeSlowCooldown, 0f, fpc.timeSlowDuration
            );
            fpc.timeSlowRegenerateRate = EditorGUILayout.Slider(
                new GUIContent("Regenerate Rate", "Rate at which the time slow regenerates when not in use."),
                fpc.timeSlowRegenerateRate, 0f, 5f
            );
            fpc.timeSlowFillImage = (Image)EditorGUILayout.ObjectField(
                new GUIContent("Time Slow Fill Image", "UI Image component to display time slow status."),
                fpc.timeSlowFillImage, typeof(Image), true
            );
            GUI.enabled = true;
        }

        #endregion


        #endregion

        #region Head Bob

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Head Bob Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        fpc.enableHeadBob = EditorGUILayout.ToggleLeft(new GUIContent("Enable Head Bob", "Determines if the camera will bob while the player is walking."), fpc.enableHeadBob);


        GUI.enabled = fpc.enableHeadBob;
        fpc.joint = (Transform)EditorGUILayout.ObjectField(new GUIContent("Camera Joint", "Joint object position is moved while head bob is active."), fpc.joint, typeof(Transform), true);
        fpc.bobSpeed = EditorGUILayout.Slider(new GUIContent("Speed", "Determines how often a bob rotation is completed."), fpc.bobSpeed, 1, 20);
        fpc.bobAmount = EditorGUILayout.Vector3Field(new GUIContent("Bob Amount", "Determines the amount the joint moves in both directions on every axes."), fpc.bobAmount);
        GUI.enabled = true;

        #endregion

        //Sets any changes from the prefab
        if (GUI.changed)
        {
            EditorUtility.SetDirty(fpc);
            Undo.RecordObject(fpc, "FPC Change");
            SerFPC.ApplyModifiedProperties();
        }
    }
}
#endif
