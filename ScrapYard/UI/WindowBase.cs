using System;
using UnityEngine;

namespace ScrapYard.UI
{
    public class WindowBase
    { 
        //[Flags]
        //public enum UIScenes
        //{
        //    All = 0,
        //    MainMenu = 1,
        //    SpaceCenter = 2,
        //    Editor = 4,
        //    Flight = 8,
        //    TrackStation = 16,
        //    MapMode = 32

        //} 

        public WindowBase(int id, string title)
        {
            ID = id;
            Title = title;
        }

        public WindowBase(int id, string title, bool draggable, bool centered)
        {
            ID = id;
            Title = title;
            Draggable = draggable;
            CenterWindow = centered;
        }

        //public UIScenes VisibleScenes { get; set; } = UIScenes.All;

        public int ID { get; private set; } = 8234;
        private Rect _windowRect = new Rect((Screen.width - 300) / 2, (Screen.height / 4), 300, 1);
        public Rect WindowRect
        {
            get { return _windowRect; }
            set { _windowRect = value; }
        }

        public string Title { get; set; } = string.Empty;
        public bool IsVisible { get; set; }
        public bool CenterWindow { get; set; }
        public bool Draggable { get; set; } = true; //True by default

        public GUISkin Skin { get; set; }



        /// <summary>
        /// Makes the window not visible
        /// </summary>
        public virtual void Close()
        {
            IsVisible = false;
        }

        /// <summary>
        /// Causes the window to be visible
        /// </summary>
        public virtual void Show()
        {
            IsVisible = true;
        }

        /// <summary>
        /// Draws the window
        /// </summary>
        /// <param name="windowID">The window's id</param>
        public virtual void Draw(int windowID)
        {
            if (Draggable)
            {
                dragWindow();
            }
        }

        /// <summary>
        /// Handles all drawing of the window and handling of the Rect object. Call from OnGUI and magic happens.
        /// </summary>
        /// <param name="id">The (preferably unique) int ID of the window.</param>
        /// <param name="title">The window's title.</param>
        public void OnGUIHandler()
        {
            if (IsVisible)
            {
                _windowRect = GUILayout.Window(ID, _windowRect, Draw, Title);
                if (CenterWindow)
                {
                    centerWindow();
                }
            }
        }

        /// <summary>
        /// Sets the size of the window
        /// </summary>
        /// <param name="left">The left edge of the window</param>
        /// <param name="top">The top edge of the window</param>
        /// <param name="width">The window width</param>
        /// <param name="height">The window height</param>
        public void SetSize(double left, double top, double width, double height)
        {
            //so we don't have to cast ourselves
            _windowRect = new Rect((float)left, (float)top, (float)width, (float)height);
        }


        /// <summary>
        /// Sets the height of the window to 1 so it can automatically resize to the correct height
        /// </summary>
        public void MinimizeHeight()
        {
            _windowRect.height = 1;
        }

        /// <summary>
        /// Makes the window draggable
        /// </summary>
        protected void dragWindow()
        {
            if (!Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2))
            {
                GUI.DragWindow();
            }
        }

        /// <summary>
        /// Centers the window
        /// </summary>
        protected void centerWindow()
        {
            float newX = (Screen.width - WindowRect.width) / 2.0f;
            float newY = (Screen.height - WindowRect.height) / 2.0f;

            WindowRect = new Rect(newX, newY, WindowRect.width, WindowRect.height);
        }

        /// <summary>
        /// Saves the position and visibility to a ConfigNode
        /// </summary>
        /// <returns>A ConfigNode with the position and visibility data</returns>
        public ConfigNode SavePosition(bool saveVisibility = true)
        {
            ConfigNode posNode = new ConfigNode("POSITION");
            posNode.AddValue("name", Title);
            posNode.AddValue("x", WindowRect.x);
            posNode.AddValue("y", WindowRect.y);
            posNode.AddValue("width", WindowRect.width);
            posNode.AddValue("height", WindowRect.height);
            if (saveVisibility)
            {
                posNode.AddValue("visible", IsVisible);
            }
            return posNode;
        }

        /// <summary>
        /// Loads the position and visibility info from a confignode
        /// </summary>
        /// <param name="posNode">The position node</param>
        public void LoadPosition(ConfigNode posNode)
        {
            if (posNode == null || posNode.GetValue("name") != Title)
            {
                return;
            }
            float x, y, width, height;
            float.TryParse(posNode.GetValue("x"), out x);
            float.TryParse(posNode.GetValue("y"), out y);
            float.TryParse(posNode.GetValue("width"), out width);
            float.TryParse(posNode.GetValue("height"), out height);

            SetSize(x, y, width, height);
            bool vis;
            bool.TryParse(posNode.GetValue("visible"), out vis);
            if (vis)
            {
                Show();
            }
        }
    }
}
