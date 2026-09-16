using UnityEngine;
using MelonLoader;

[assembly: MelonInfo(typeof(AnimalCompanyMod.DevMenu), "Animal Company Custom Canvas", "1.1.0", "Developer")]
[assembly: MelonGame("DefaultCompany", "Animal Company")]

namespace AnimalCompanyMod
{
    public class DevMenu : MelonMod
    {
        // UI Layout Configuration
        private bool isMenuOpen = true;
        private Rect windowRect = new Rect(80, 80, 450, 400); // Widened layout for columns
        private int activeTab = 0; // Tracks which category tab is selected

        // Feature Configurations
        private bool toggleFly = false;
        private bool toggleSpeed = false;
        private float speedMultiplier = 1.0f;
        
        // Soundboard References
        private AudioSource modAudioSource;
        private AudioClip sampleClip;

        public override void OnUpdate()
        {
            // VR Binding: KeyCode.JoystickButton2 is the 'X' Button on the Meta Quest Left Controller
            // Desktop Binding: 'Insert' key remains as a laptop backup option
            if (Input.GetKeyDown(KeyCode.JoystickButton2) || Input.GetKeyDown(KeyCode.Insert))
            {
                isMenuOpen = !isMenuOpen;
            }
        }

        public override void OnGUI()
        {
            if (!isMenuOpen) return;

            // Apply custom dark theme aesthetic colors
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f); 
            windowRect = GUI.Window(0, windowRect, DrawWindowContent, "Animal Company Custom Controller Layout");
        }

        private void DrawWindowContent(int windowID)
        {
            // Reset content color for clear text reading
            GUI.backgroundColor = Color.gray;
            GUILayout.Space(5);

            // Horizontal Tab Bar: Replicating structured tab navigation styles
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(activeTab == 0, "Movement Mods", "Button", GUILayout.Height(30))) activeTab = 0;
            if (GUILayout.Toggle(activeTab == 1, "Soundboard", "Button", GUILayout.Height(30))) activeTab = 1;
            if (GUILayout.Toggle(activeTab == 2, "Utility Options", "Button", GUILayout.Height(30))) activeTab = 2;
            GUILayout.EndHorizontal();

            GUILayout.Space(15);

            // Dynamically render layout content based on the selected tab
            switch (activeTab)
            {
                case 0:
                    DrawMovementTab();
                    break;
                case 1:
                    DrawSoundboardTab();
                    break;
                case 2:
                    DrawUtilityTab();
                    break;
            }

            // Pin the exit option to the baseline of the window frame
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Hide Menu Layout", GUILayout.Height(25)))
            {
                isMenuOpen = false;
            }

            // Allows dragging the panel by interacting with the title bar layout frame
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void DrawMovementTab()
        {
            GUILayout.Label("--- Physics & Positioning Toggles ---", GUILayout.ExpandWidth(true));
            GUILayout.Space(10);

            toggleFly = GUILayout.Toggle(toggleFly, " Enable Workspace Flight Mode");
            GUILayout.Space(5);

            toggleSpeed = GUILayout.Toggle(toggleSpeed, " Enable Velocity Override");
            if (toggleSpeed)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Multiplier: {speedMultiplier:F1}x", GUILayout.Width(110));
                speedMultiplier = GUILayout.HorizontalSlider(speedMultiplier, 1.0f, 12.0f);
                GUILayout.EndHorizontal();
            }
        }

        private void DrawSoundboardTab()
        {
            GUILayout.Label("--- Local Audio Output Soundboard ---", GUILayout.ExpandWidth(true));
            GUILayout.Space(10);

            // Audio trigger panel buttons arranged vertically
            if (GUILayout.Button("🔊 Trigger Playback Effect 1", GUILayout.Height(35)))
            {
                PlaySoundboardEffect();
            }
            GUILayout.Space(5);
            if (GUILayout.Button("🔊 Trigger Playback Effect 2", GUILayout.Height(35)))
            {
                PlaySoundboardEffect();
            }
        }

        private void DrawUtilityTab()
        {
            GUILayout.Label("--- Administrative Tool Utilities ---", GUILayout.ExpandWidth(true));
            GUILayout.Space(10);

            if (GUILayout.Button("Reset Transform Positions", GUILayout.Height(30)))
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) player.transform.position = Vector3.zero;
            }
        }

        private void PlaySoundboardEffect()
        {
            if (modAudioSource == null)
            {
                modAudioSource = GameObject.FindGameObjectWithTag("Player")?.GetComponent<AudioSource>();
            }

            if (modAudioSource != null && sampleClip != null)
            {
                modAudioSource.PlayOneShot(sampleClip);
            }
        }
    }
}
