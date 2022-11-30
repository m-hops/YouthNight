using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Character Controls")]
    [SerializeField] float mouseSensitivity = 3.5f;
    [SerializeField] float walkSpeed = 10f;
    [SerializeField] float jumpHeight = 1.0f;
    [SerializeField] float gravity = -13.0f;
    [SerializeField, Range(0.0f, 0.5f)] float moveSmoothTime = 0.15f;
    [SerializeField, Range(0.0f, 0.3f)] float mouseSmoothTime = 0.01f;

    [SerializeField] GameObject playerFlashlight;

    public bool flashlightOn = true;

    // Character values
    GameObject playerCamera;
    float cameraPitch;
    float velocityY;
    CharacterController controller;
    Light playerLight;
    bool isCrouched = false;
    bool isRunning = false;
    float maxPlayerHeight;
    float startWalkSpeed;
    AudioManagerMenu audioManagerMenu;

    // Used to create character smoothing movement
    Vector2 currentDir = Vector2.zero;
    Vector2 currentDirVelocity = Vector2.zero;
    Vector2 currentMouseDelta = Vector2.zero;
    Vector2 currentMouseDeltaVelocity = Vector2.zero;

    void Start()
    {
        // Locks cursor position at startup 
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Get child objects at start up
        playerCamera = gameObject.transform.Find("MainCamera").gameObject;
        controller = GetComponent<CharacterController>();
        playerLight = playerFlashlight.GetComponent<Light>();
        maxPlayerHeight = controller.height;
        startWalkSpeed = walkSpeed;
        audioManagerMenu = GameObject.FindGameObjectWithTag("audioManager").gameObject.GetComponent<AudioManagerMenu>();
    }
    
    void Update()
    {
        RunControls();
        UpdateMouseLook();
        UpdatePlayerMovement();
        FlashlightControls();
    }

    /// <summary>
    /// Updates mouse controls
    /// </summary>
    void UpdateMouseLook()
    {
        // Gets new location every frame
        Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDelta, mouseSmoothTime);

        // Adjusts camera vertical with mouse sensitivity
        cameraPitch -= currentMouseDelta.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -90.0f, 90.0f);
        playerCamera.transform.localEulerAngles = Vector3.right * cameraPitch;

        // Adjusts camera horizontal with mouse sensitivity
        transform.Rotate(Vector3.up * (currentMouseDelta.x * mouseSensitivity));
    }
    
    /// <summary>
    /// Updates player movement
    /// </summary>
    void UpdatePlayerMovement()
    {
        // Gets new location every frame and normalizes values
        Vector2 targetDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        targetDir.Normalize();

        currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, moveSmoothTime);

        // Checks to see if player grounded and stops them from falling further
        if (controller.isGrounded && velocityY < 0)
        {
            velocityY = 0.0f;
        }
        
        velocityY += gravity * Time.deltaTime;

        // Applies movement through character controller
        Vector3 velocity = (transform.forward * currentDir.y + transform.right * currentDir.x) * walkSpeed + Vector3.up * velocityY;
        controller.Move(velocity * Time.deltaTime);

        // Jump Action
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocityY += Mathf.Sqrt(jumpHeight * -3.0f * gravity);
        }
    }
    
    /// <summary>
    /// Controls light component inside PlayerCharacter
    /// </summary>
    void FlashlightControls()
    {
        if (Input.GetKeyDown("f"))
        {
            audioManagerMenu.flashlightButtonSFX.PlayOneShot(audioManagerMenu.flashlightButtonSFX.clip);
            flashlightOn = !flashlightOn;
            SetFlashlight(flashlightOn);
        }
    }

    ///Player Crouch controls
    void CrouchControl()
    {
        if (Input.GetKeyDown("left ctrl"))
        {
            if (!isCrouched)
            {
                controller.height = maxPlayerHeight /2;
                isCrouched = true;
            }
            else
            {
                controller.height = maxPlayerHeight;
                isCrouched = false;
            }
        }

    }

    void RunControls()
    {
        if (Input.GetKeyDown("left shift"))
        {  
             walkSpeed = startWalkSpeed * 2;
             isRunning = true;
        }
        else if (Input.GetKeyUp("left shift"))
        {
            walkSpeed = startWalkSpeed;
            isRunning = false;
        }
    }

    public void SetFlashlight(bool isOn)
    {
        
        var lightFX = playerLight.GetComponent<VLB.VolumetricLightBeam>();

        lightFX.enabled = isOn;
        playerLight.enabled = isOn;

    }
}
