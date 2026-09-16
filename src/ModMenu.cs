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

        // Soundboard Configuration Data
        private AudioSource audioSourceLink;

        public override void OnUpdate()
        {
            // Toggle physical layout presence
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
            menuFrameObject = new GameObject("Skydlimits_3DBoard");
            
            // Build the main dark physical panel backing board
            GameObject boardVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boardVisual.name = "BoardVisual";
            boardVisual.transform.SetParent(menuFrameObject.transform, false);
            boardVisual.transform.localScale = new Vector3(0.8f, 1.0f, 0.02f); 
            boardVisual.GetComponent<Renderer>().material.color = new Color(0.05f, 0.05f, 0.05f);

            // Row 1: Core Physics & Positioning Modifiers
            CreatePhysicalButton("Btn_HandFly", new Vector3(-0.2f, 0.3f, 0.02f), Color.cyan, () => {
                isFlyEnabled = !isFlyEnabled;
                isJoystickFlyEnabled = false;
            });

            CreatePhysicalButton("Btn_JoystickFly", new Vector3(0.2f, 0.3f, 0.02f), Color.blue, () => {
                isJoystickFlyEnabled = !isJoystickFlyEnabled;
                isFlyEnabled = false;
            });

            // Row 2 & 3: Local Soundboard Panel (Queries internal asset clips at runtime)
            CreatePhysicalButton("Sfx_Boombox", new Vector3(-0.2f, 0.0f, 0.02f), Color.yellow, () => {
                TriggerPreloadedGameSound("Boombox"); // Audio string identifier tag
            });

            CreatePhysicalButton("Sfx_MonsterRoar", new Vector3(0.2f, 0.0f, 0.02f), Color.red, () => {
                TriggerPreloadedGameSound("Banshee"); // Creature vocalization tag
            });

            CreatePhysicalButton("Sfx_LaserTool", new Vector3(-0.2f, -0.3f, 0.02f), Color.green, () => {
                TriggerPreloadedGameSound("MiningLaser"); // Item effect asset tag
            });

            CreatePhysicalButton("Btn_TeleportOrigin", new Vector3(0.2f, -0.3f, 0.02f), Color.magenta, () => {
                ExecuteTeleport(Vector3.zero);
            });

            UpdateMenuPosition();
            Object.DontDestroyOnLoad(menuFrameObject);
        }

        private void CreatePhysicalButton(string buttonName, Vector3 localPos, Color buttonColor, System.Action onTouchAction)
        {
            GameObject btnObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            btnObj.name = buttonName;
            btnObj.transform.SetParent(menuFrameObject.transform, false);
            btnObj.transform.localPosition = localPos;
            btnObj.transform.localScale = new Vector3(0.35f, 0.15f, 0.04f); 
            btnObj.GetComponent<Renderer>().material.color = buttonColor;

            BoxCollider collider = btnObj.GetComponent<BoxCollider>();
            if (collider != null) collider.isTrigger = true;

            TouchButtonTrigger listener = btnObj.AddComponent<TouchButtonTrigger>();
            listener.Initialize(onTouchAction, buttonColor);
        }

        private void CreateHandTouchSphere()
        {
            touchSphereObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            touchSphereObject.name = "Skydlimits_TouchSphere";
            touchSphereObject.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
            touchSphereObject.GetComponent<Renderer>().material.color = Color.white;

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
                menuFrameObject.transform.position = cameraTransform.position + (cameraTransform.forward * 1.2f);
                menuFrameObject.transform.lookAt(cameraTransform.position);
                menuFrameObject.transform.Rotate(0, 180, 0);
            }

            GameObject rightHand = GameObject.Find("RightHand") ?? GameObject.Find("RightController");
            if (rightHand != null && touchSphereObject != null)
            {
                touchSphereObject.transform.position = rightHand.transform.position;
            }
        }

        private void TriggerPreloadedGameSound(string assetKeyword)
        {
            // Locate local client layout voice output pipeline components
            if (audioSourceLink == null)
            {
                audioSourceLink = GameObject.FindGameObjectWithTag("Player")?.GetComponent<AudioSource>();
            }

            if (audioSourceLink == null) return;

            // Queries active internal Unity runtime assets to match sample names
            AudioClip[] objectsInMemory = Resources.FindObjectsOfTypeAll<AudioClip>();
            foreach (AudioClip sampleClip in objectsInMemory)
            {
                if (sampleClip.name.Contains(assetKeyword))
                {
                    audioSourceLink.PlayOneShot(sampleClip);
                    MelonLogger.Msg($"[skydlimits] Found and executed clip file matching: {sampleClip.name}");
                    break;
                }
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
            if (other.gameObject.name == "Skydlimits_TouchSphere" && cooldownTimer <= 0)
            {
                cooldownTimer = 0.5f; 
                GetComponent<Renderer>().material.color = Color.green; 
                
                triggerAction?.Invoke();
                
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
