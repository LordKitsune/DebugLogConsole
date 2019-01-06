using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Harmony;
using UnityEngine;
using System.Reflection;
using System.IO;
using System.Linq;

namespace Kitsu.Debug
{
    public class KitsuLog : MonoBehaviour
    {
        public static AssetBundle bundle;
        private GUISkin mySkin;
        public static KitsuLog instance;
        private Texture2D arrow;
        private bool isVisible = false;
        public static List<Message> lines = new List<Message>();
        private Vector2 scrollPosition = new Vector2();
        private Vector2 clickPosition = new Vector2();
        private Rect windowRect = new Rect(0, 0, Screen.width / 3, Screen.height / 2);
        private Rect originalRect;
        private int side;
        private bool autoScroll = true;
        private bool autoError = true;
        private bool collapse = false;
        private bool resizable = false;
        private bool draggable = true;
        private string lastLine = string.Empty;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            UnityEngine.Debug.Log("Ingame Log Initiated!");
            instance = this;
            autoScroll = Main.Config["AutoScroll"];
            autoError = Main.Config["AutoError"];
            resizable = Main.Config["ResizableLog"];
            draggable = Main.Config["DraggableLog"];
            arrow = new Texture2D(50, 50);
            byte[] data = File.ReadAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\arrow.png");
            arrow.LoadImage(data);
            arrow.Apply();
            StartCoroutine(loadAssets());
        }

        private IEnumerator loadAssets()
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace('\\', '/') + "/debug.unity3d");
            yield return request;
            bundle = request.assetBundle;
            mySkin = bundle.LoadAsset<GUISkin>("scifi_skin");
        }

        public static void Log(string line, LogType type = LogType.Log)
        {
            string color = "white";
            string prefix = "[LOG]";
            if (type == LogType.Warning)
            {
                color = "yellow";
                prefix = "[WARNING]";
            }
            else if (type == LogType.Exception)
            {
                color = "red";
                prefix = "[ERROR]";
            }
            string msg = string.Format("<size=13><color={0}>{1}{2}</color></size>", color, prefix, line);
            if (msg == instance.lastLine)
            {
                Message item = lines[lines.Count - 1];
                item.Count++;
            }
            else
            {
                lines.Add(new Message(msg));
            }
            
            if (instance.autoScroll)
            {
                instance.scrollPosition.y = Mathf.Infinity;
            }
            if (instance.autoError && type == LogType.Error)
                instance.isVisible = true;
            instance.lastLine = msg;
        }

        private bool ToggleButton(bool input, string text, float size = 150f)
        {
            string subtext = string.Format("\n<color=#{0}><b>{1}</b></color>", input ? "004b00" : "ff0000", input.ToString());
            if (GUILayout.Button(text + subtext, GUILayout.Width(size)))
                input = !input;
            return input;
        }

        private void OnGUI()
        {
            if (mySkin)
                GUI.skin = mySkin;
            if (isVisible)
            {
                windowRect = GUILayout.Window(200, windowRect, (id) =>
                {
                    GUILayout.Space(5f);
                    GUILayout.BeginHorizontal();
                    autoError = ToggleButton(autoError, "Auto Error");
                    GUILayout.FlexibleSpace();
                    resizable = ToggleButton(resizable, "Resizable");
                    GUILayout.FlexibleSpace();
                    draggable = ToggleButton(draggable, "Draggable");
                    GUILayout.FlexibleSpace();
                    collapse = ToggleButton(collapse, "Collapse Messages");
                    GUILayout.EndHorizontal();
                    GUI.skin.label.wordWrap = true;
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("<size=20>Log Feed</size>");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Height(windowRect.height / 5 * 3));
                    if (scrollPosition.y < Mathf.Infinity)
                        autoScroll = false;
                    
                    foreach (Message msg in lines)
                    {
                        if (msg.Count > 1 & collapse)
                        {
                            GUILayout.Label(msg.message + $"({msg.Count})");
                        }
                        else
                        {
                            for (int i = 0; i < msg.Count; i++)
                            {
                                GUILayout.Label(msg.message);
                            }
                        }
                        
                    }
                    GUILayout.EndScrollView();
                    GUILayout.FlexibleSpace();
                    if (draggable)
                        GUI.DragWindow();
                }, "Options");
                if (resizable)
                {
                    resizeWindow();
                }
            }
        }

        private bool isOnLeft(Vector2 pos) => (pos.x - 10f < windowRect.x & pos.x + 10f > windowRect.x);

        private bool isOnRight(Vector2 pos) => (pos.x - 10f < windowRect.x + windowRect.width & pos.x + 10f > windowRect.x + windowRect.width);

        private bool isOnTop(Vector2 pos) => (pos.y - 10f < windowRect.y & pos.y + 10f > windowRect.y);

        private bool isOnBottom(Vector2 pos) => (pos.y - 10f < windowRect.y + windowRect.height & pos.y + 10f > windowRect.y + windowRect.height);

        private bool isOnWindowSides(Vector2 pos) => (isOnRight(pos) | isOnLeft(pos));

        private bool isOnWindowTops(Vector2 pos) => (isOnBottom(pos) | isOnTop(pos));

        private bool isOnAnySide(Vector2 pos) => (isOnTop(pos) | isOnWindowSides(pos));

        private void resizeWindow()
        {
            if (Input.GetMouseButtonDown(0))
            {
                clickPosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                originalRect = windowRect;
                if (isOnRight(clickPosition))
                    side = 1;
                if (isOnBottom(clickPosition))
                    side = 2;
                if (isOnRight(clickPosition) && isOnBottom(clickPosition))
                    side = 3;
            }
            Vector2 pos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            if (isOnRight(pos) && (pos.y > windowRect.y && pos.y < windowRect.y + windowRect.height) || (side == 1 && clickPosition.x != -1f))
            {
                GUI.Label(new Rect(pos.x - 25f, pos.y - 25f, 50f, 50f), arrow);
            }
            if (isOnBottom(pos) && (pos.x > windowRect.x && pos.x < windowRect.x + windowRect.width) || (side == 2))
            {
                GUIUtility.RotateAroundPivot(90f, new Vector2(pos.x, pos.y));
                GUI.Label(new Rect(pos.x - 25f, pos.y - 25f, 50f, 100f), arrow);
                GUIUtility.RotateAroundPivot(-90f, new Vector2(pos.x, pos.y));
            }
            if (Input.GetMouseButton(0))
            {
                if (clickPosition.x != -1f)
                {
                    if (side == 1 | side == 3)
                    {
                        Vector2 offset = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                        windowRect.width = originalRect.width + (offset.x - clickPosition.x);
                    }
                    if (side == 2 | side == 3)
                    {
                        Vector2 offset = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                        windowRect.height = originalRect.height + (offset.y - clickPosition.y);
                    }
                }
            }
            else
            {
                side = 0;
                clickPosition.x = -1f;
            }
                
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                isVisible = !isVisible;
                autoScroll = true;
            }
        }


        public class Message
        {
            public string message;
            public int Count = 1;
            public Message(string msg)
            {
                message = msg;
            }
        }

    }

    [HarmonyPatch(typeof(DevConsole), "Awake")]
    public static class DebugConsolePatch
    {
        [HarmonyPostfix]
        public static void PostFix(DevConsole __instance)
        {
            new GameObject("ConsoleCommands").AddComponent<KitsuLog>();
        }
    }
}
