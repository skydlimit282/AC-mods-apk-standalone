using UnityEngine;
using MelonLoader;

// Defines this script as an official MelonLoader mod
[assembly: MelonInfo(typeof(AnimalCompanyMod.DevMenu), "Animal Company Dev Menu", "1.0.0", "Developer")]
[assembly: MelonGame("DefaultCompany", "Animal Company")]

namespace AnimalCompanyMod
{
    public class DevMenu : MelonMod
    {
        private bool isMenuOpen = true;
        private Rect windowRect = new Rect(100, 100, 300, 350);

        // Feature Toggles
        private bool toggleFly = false;
        private bool toggleSpeed = false;
        private float speedMultiplier = 1.0f;

        public override void OnUpdate()
        {
            // Toggle menu display via PC/Keyboard check
            if (Input.GetKeyDown(KeyCode.Insert))
            {
                isMenuOpen = !isMenuOpen;
            }
        }

        public override void OnGUI()
        {
            if (!isMenuOpen) return;

            GUI.backgroundColor = Color.black;
            windowRect = GUI.Window(0, windowRect, DrawWindowContent, "Animal Company Debug Canvas");
        }

        private void DrawWindowContent(int windowID)
        {
            GUI.backgroundColor = Color.gray;
            GUILayout.Space(10);
            GUILayout.Label("Sandbox Variable Modifiers:", GUILayout.ExpandWidth(true));
            GUILayout.Space(10);

            // Feature 1: Flight Mode Template
            toggleFly = GUILayout.Toggle(toggleFly, " Enable Flight / Noclip Mode");
            if (toggleFly)
            {
                // Hooks for sandbox adjustments go here
            }

            GUILayout.Space(5);

            // Feature 2: Speed Multiplier Slider
            toggleSpeed = GUILayout.Toggle(toggleSpeed, " Override Base Movement Speed");
            if (toggleSpeed)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Speed: {speedMultiplier:F1}x", GUILayout.Width(80));
                speedMultiplier = GUILayout.HorizontalSlider(speedMultiplier, 1.0f, 10.0f);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Close Menu (Press Insert)", GUILayout.Height(30)))
            {
                isMenuOpen = false;
            }

            // Makes the window draggable by clicking the top banner
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
    }
}
