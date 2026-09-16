using UnityEngine;
using UnityEngine.UI;
using MelonLoader;

[assembly: MelonInfo(typeof(AnimalCompanyMod.SkydlimitsMenu), "skydlimits v1.1", "1.1.0", "Developer")]
[assembly: MelonGame("DefaultCompany", "Animal Company")]

namespace AnimalCompanyMod
{
    public class SkydlimitsMenu : MelonMod
    {
        // VR Menu State & Objects
        private GameObject menuCanvasObject;
        private bool isMenuOpen = false;
        
        // Active Toggles
        private bool toggleFly = false;
        private bool toggleSpeed = false;
        private float speedMultiplier = 1.0f;

        public override void OnUpdate()
        {
            // VR Toggle: Press 'X' on Meta Quest Left Controller to spawn/despawn the menu
            if (Input.GetKeyDown(KeyCode.JoystickButton2) || Input.GetKeyDown(KeyCode.Insert))
            {
                isMenuOpen = !isMenuOpen;
                ToggleVRMenu(isMenuOpen);
            }

            // Keep the menu floating relative to the player's position when open
            if (isMenuOpen && menuCanvasObject != null)
            {
                UpdateMenuPosition();
            }
        }

        private void ToggleVRMenu(bool open)
        {
            if (open)
            {
                if (menuCanvasObject == null)
                {
                    Create3DWorldCanvas();
                }
                menuCanvasObject.SetActive(true);
            }
            else
            {
                if (menuCanvasObject != null)
                {
                    menuCanvasObject.SetActive(false);
                }
            }
        }

        private void Create3DWorldCanvas()
        {
            // 1. Create Main World Space Object
            menuCanvasObject = new GameObject("SkydlimitsCanvasFrame");
            Canvas canvas = menuCanvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            menuCanvasObject.AddComponent<CanvasScaler>();
            menuCanvasObject.AddComponent<GraphicRaycaster>();

            // Resize the floating menu board
            RectTransform rect = menuCanvasObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(4f, 5f);
            rect.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            // 2. Create the Background Board Visual
            GameObject background = new GameObject("BackgroundPanel");
            background.transform.SetParent(menuCanvasObject.transform, false);
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.05f, 0.05f, 0.05f, 0.95f); // Sleek dark aesthetic
            background.GetComponent<RectTransform>().sizeDelta = new Vector2(4f, 5f);

            // 3. Create the Header Text Bar ("skydlimits v1.1")
            GameObject headerObj = new GameObject("HeaderText");
            headerObj.transform.SetParent(menuCanvasObject.transform, false);
            Text headerText = headerObj.AddComponent<Text>();
            headerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            headerText.text = "skydlimits v1.1";
            headerText.fontSize = 32;
            headerText.alignment = TextAnchor.MiddleCenter;
            headerText.color = Color.cyan; // Cyan primary accent theme

            RectTransform headerRect = headerObj.GetComponent<RectTransform>();
            headerRect.anchoredPosition = new Vector2(0, 2.2f);
            headerRect.sizeDelta = new Vector2(4f, 0.5f);

            // Initial positioning in front of the VR camera view layout
            UpdateMenuPosition();
            Object.DontDestroyOnLoad(menuCanvasObject);
        }

        private void UpdateMenuPosition()
        {
            // Finds the VR Main Camera tag and places the board 2 meters directly in front of your face
            Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
            if (cameraTransform != null)
            {
                Vector3 targetPosition = cameraTransform.position + (cameraTransform.forward * 2.0f);
                menuCanvasObject.transform.position = targetPosition;
                
                // Rotates the board so it flatly faces the player's eyes
                menuCanvasObject.transform.lookAt(cameraTransform.position);
                menuCanvasObject.transform.Rotate(0, 180, 0); 
            }
        }
    }
}
