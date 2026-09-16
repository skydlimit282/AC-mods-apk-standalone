using UnityEngine;
using MelonLoader;

[assembly: MelonInfo(typeof(AnimalCompanyMod.SkydlimitsMenu), "skydlimits v1.1", "1.1.0", "Developer")]
[assembly: MelonGame("DefaultCompany", "Animal Company")]

namespace AnimalCompanyMod
{
    public class SkydlimitsMenu : MelonMod
    {
        private GameObject menuFrameObject;
        private GameObject touchSphereObject;
        private bool isMenuOpen = false;

        // Mod Physics States
        private bool isFlyEnabled = false;
        private bool isJoystickFlyEnabled = false;
        private float flySpeed = 10f;

        public override void OnUpdate()
        {
            // VR Toggle: Press 'X' on Quest Left Controller to spawn/despawn the physical board
            if (Input.GetKeyDown(KeyCode.JoystickButton2) || Input.GetKeyDown(KeyCode.Insert))
            {
                isMenuOpen = !isMenuOpen;
                TogglePhysicalMenu(isMenuOpen);
            }

            if (isMenuOpen && menuFrameObject != null)
            {
                UpdateMenuPosition();
            }

            HandleFlightMechanics();
        }

        private void TogglePhysicalMenu(bool open)
        {
            if (open)
            {
                if (menuFrameObject == null) CreatePhysical3DMenu();
                if (touchSphereObject == null) CreateHandTouchSphere();
                
                menuFrameObject.SetActive(true);
                touchSphereObject.SetActive(true);
            }
            else
            {
                if (menuFrameObject != null) menuFrameObject.SetActive(false);
                if (touchSphereObject != null) touchSphereObject.SetActive(false);
            }
        }

        private void CreatePhysical3DMenu()
        {
            // 1. Create the Main Menu Board Container
            menuFrameObject = new GameObject("Skydlimits_3DBoard");
            
            // 2. Create a physical background slate (a thin 3D Cube)
            GameObject boardVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boardVisual.name = "BoardVisual";
            boardVisual.transform.SetParent(menuFrameObject.transform, false);
            boardVisual.transform.localScale = new Vector3(0.6f, 0.8f, 0.02f); // Width, Height, Thickness
            boardVisual.GetComponent<Renderer>().material.color = new Color(0.05f, 0.05f, 0.05f); // Sleek Dark Slate

            // 3. Spawn Physical 3D Buttons (Cubes) with custom trigger behaviors
            CreatePhysicalButton("Btn_HandFly", new Vector3(0, 0.2f, 0.02f), Color.cyan, () => {
                isFlyEnabled = !isFlyEnabled;
                isJoystickFlyEnabled = false;
                MelonLogger.Msg($"[skydlimits] Physical Hand Fly: {isFlyEnabled}");
            });

            CreatePhysicalButton("Btn_JoystickFly", new Vector3(0, 0.0f, 0.02f), Color.blue, () => {
                isJoystickFlyEnabled = !isJoystickFlyEnabled;
                isFlyEnabled = false;
                MelonLogger.Msg($"[skydlimits] Physical Joystick Fly: {isJoystickFlyEnabled}");
            });

            CreatePhysicalButton("Btn_Teleport", new Vector3(0, -0.2f, 0.02f), Color.magenta, () => {
                ExecuteTeleport(Vector3.zero);
            });

            UpdateMenuPosition();
            Object.DontDestroyOnLoad(menuFrameObject);
        }

        private void CreatePhysicalButton(string buttonName, Vector3 localPos, Color buttonColor, System.Action onTouchAction)
        {
            // Create a small 3D Box for the button geometry
            GameObject btnObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            btnObj.name = buttonName;
            btnObj.transform.SetParent(menuFrameObject.transform, false);
            btnObj.transform.localPosition = localPos;
            btnObj.transform.localScale = new Vector3(0.4f, 0.12f, 0.04f); // Dimension of the button box
            btnObj.GetComponent<Renderer>().material.color = buttonColor;

            // Turn its Collider into a Trigger so the hand can pass through it to click it
            BoxCollider collider = btnObj.GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            // Attach a small component to listen for the hand sphere collision intersection
            TouchButtonTrigger listener = btnObj.AddComponent<TouchButtonTrigger>();
            listener.Initialize(onTouchAction, buttonColor);
        }

        private void CreateHandTouchSphere()
        {
            // Spawns a physical tracking sphere on your right hand controller structure
            touchSphereObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            touchSphereObject.name = "Skydlimits_TouchSphere";
            touchSphereObject.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f); // Small finger/hand sized ball
            touchSphereObject.GetComponent<Renderer>().material.color = Color.white;

            // Add a Rigidbody so Unity calculates physics overlap intersections correctly
            Rigidbody rb = touchSphereObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; 
            rb.useGravity = false;

            Object.DontDestroyOnLoad(touchSphereObject);
        }

        private void UpdateMenuPosition()
        {
            Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
            if (cameraTransform != null)
            {
                // Floats the 3D block board 1.2 meters away in front of you
                menuFrameObject.transform.position = cameraTransform.position + (cameraTransform.forward * 1.2f);
                menuFrameObject.transform.lookAt(cameraTransform.position);
                menuFrameObject.transform.Rotate(0, 180, 0);
            }

            // Automatically maps our physical white ball to wherever your Right VR Controller points
            // JoystickButton15/Right Hand Anchor points
            GameObject rightHand = GameObject.Find("RightHand") ?? GameObject.Find("RightController");
            if (rightHand != null && touchSphereObject != null)
            {
                touchSphereObject.transform.position = rightHand.transform.position;
            }
        }

        private void HandleFlightMechanics()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            if (isFlyEnabled && Input.GetKey(KeyCode.JoystickButton15)) 
            {
                player.transform.position += Camera.main.transform.forward * flySpeed * Time.deltaTime;
            }

            if (isJoystickFlyEnabled)
            {
                float verticalInput = Input.GetAxis("Vertical");
                float horizontalInput = Input.GetAxis("Horizontal");
                Vector3 moveDirection = (Camera.main.transform.forward * verticalInput) + (Camera.main.transform.right * horizontalInput);
                player.transform.position += moveDirection * flySpeed * Time.deltaTime;
            }
        }

        private void ExecuteTeleport(Vector3 destination)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) player.transform.position = destination;
        }
    }

    // Helper component attached to each 3D button block to watch for physical touches
    public class TouchButtonTrigger : MonoBehaviour
    {
        private System.Action triggerAction;
        private Color originalColor;
        private float cooldownTimer = 0f;

        public void Initialize(System.Action onTouch, Color col)
        {
            triggerAction = onTouch;
            originalColor = col;
        }

        private void Update()
        {
            if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            // Verify if the object entering our button box space is the white hand touch ball
            if (other.gameObject.name == "Skydlimits_TouchSphere" && cooldownTimer <= 0)
            {
                cooldownTimer = 0.6f; // Prevent rapid clicking flickering anomalies
                GetComponent<Renderer>().material.color = Color.green; // Flash green on click success
                
                triggerAction?.Invoke();
                
                // Return to base coloring after a brief touch confirmation window
                MelonCoroutines.Start(ResetColorRoutine());
            }
        }

        private System.Collections.IEnumerator ResetColorRoutine()
        {
            yield return new WaitForSeconds(0.3f);
            GetComponent<Renderer>().material.color = originalColor;
        }
    }
}
