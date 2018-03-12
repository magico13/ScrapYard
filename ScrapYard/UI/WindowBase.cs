using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScrapYard.UI
{
    public class WindowBase
    { 
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

        public List<GameScenes> VisibleScenes { get; set; } = new List<GameScenes>() { GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.TRACKSTATION };

        public int ID { get; private set; } = 8234;
        protected Rect _windowRect = new Rect((Screen.width - 300) / 2, (Screen.height / 4), 300, 1);
        public Rect WindowRect
        {
            get { return _windowRect; }
            set { _windowRect = value; }
        }

        public string Title { get; set; } = string.Empty;
        private bool _visible;
        public bool IsVisible
        {
            get { return _visible; }
            set
            {
                if (value != _visible)
                {
                    _visible = value;
                    if (value)
                    {
                        OnShow.Fire();
                    }
                    else
                    {
                        OnClose.Fire();
                    }
                }
            }
        }

        public bool CenterWindow { get; set; }
        public bool Draggable { get; set; } = true; //True by default
        public bool ResizeHeight { get; set; }
        public bool ResizeWidth { get; set; }

        public GUISkin Skin { get; set; }

        public EventVoid OnShow = new EventVoid("OnShow");
        public EventVoid OnClose = new EventVoid("OnClose");

        /// <summary>
        /// Fires when the mouse enters the window
        /// </summary>
        public EventVoid OnMouseOver = new EventVoid("OnMouseOver");
        /// <summary>
        /// Fires when the mouse leaves the window
        /// </summary>
        public EventVoid OnMouseExit = new EventVoid("OnMouseExit");

        protected bool _resizingVertically = false;
        protected bool _resizingHorizontally = false;
        protected bool _resizing = false;
        protected bool _shouldAllowDragging = false;
        protected Vector2 _lastMousePos;

        protected bool _mouseOver;
        /// <summary>
        /// Returns true if the mouse is over the window.
        /// Setting this will fire OnMouseOver and OnMouseExit events.
        /// </summary>
        public bool MouseIsOver
        {
            get { return _mouseOver; }
            protected set
            {
                if (value != _mouseOver)
                {
                    _mouseOver = value;
                    if (value)
                    {
                        OnMouseOver?.Fire();
                    }
                    else
                    {
                        OnMouseExit?.Fire();
                    }
                }
            }
        }

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
            if (ResizeHeight && !_resizingVertically)
            {
                //check if left clicked near bottom edge of window
                _resizingVertically = (Mouse.Left.GetButtonDown() && Mouse.screenPos.x > WindowRect.xMin && Mouse.screenPos.x < WindowRect.xMax
                && Mouse.screenPos.y > WindowRect.yMax - 10 && Mouse.screenPos.y < WindowRect.yMax);
            }
            if (ResizeWidth && !_resizingHorizontally)
            {
                //check if left clicked near right edge of window
                _resizingHorizontally = (Mouse.Left.GetButtonDown() && Mouse.screenPos.x > WindowRect.xMax-10 && Mouse.screenPos.x < WindowRect.xMax
                && Mouse.screenPos.y > WindowRect.yMin && Mouse.screenPos.y < WindowRect.yMax);
            }

            //click near the bottom of the window, then drag
            if ((Mouse.Left.GetButton() && (_resizing || _resizingHorizontally || _resizingVertically)))
            {
                if (!_resizing)
                {
                    _lastMousePos = Mouse.screenPos;
                    _resizing = true;
                    _shouldAllowDragging = Draggable;
                    Draggable = false;
                }

                float dx = _resizingHorizontally ? (Mouse.screenPos.x - _lastMousePos.x) : 0;
                float dy = _resizingVertically ? (Mouse.screenPos.y - _lastMousePos.y) : 0;

                //resize
                Rect oldSize = WindowRect;
                SetSize(oldSize.xMin, oldSize.yMin, oldSize.width + dx, oldSize.height + dy);
                
                _lastMousePos = Mouse.screenPos;
            }
            else if (_resizing)
            {
                _resizing = false;
                _resizingHorizontally = false;
                _resizingVertically = false;
                Draggable = _shouldAllowDragging;
            }

            if (Draggable)
            {
                dragWindow();
            }
        }

        /// <summary>
        /// Handles all drawing of the window and handling of the Rect object. Call from OnGUI and magic happens.
        /// </summary>
        public void OnGUIHandler()
        {
            if (IsVisible && VisibleScenes.Contains(HighLogic.LoadedScene)) //don't draw if not in a supported scene
            {
                _windowRect = GUILayout.Window(ID, _windowRect, Draw, Title);
                if (CenterWindow)
                {
                    centerWindow();
                }
            }
            //trigger mouse events if hovering on/off
            MouseIsOver = checkMouseOver();
        }

        /// <summary>
        /// Sets whether the window can be resized
        /// </summary>
        /// <param name="vertical">Set true to allow vertically resizing</param>
        /// <param name="horizontal">Set true to allow horizontally resizing</param>
        public void SetResizeable(bool vertical, bool horizontal)
        {
            ResizeHeight = vertical;
            ResizeWidth = horizontal;
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
        /// Sets the scenes that this window should be visible in
        /// </summary>
        /// <param name="scenes">Enumerable of scenes</param>
        public void SetVisibleScenes(IEnumerable<GameScenes> scenes)
        {
            VisibleScenes = scenes.ToList();
        }

        /// <summary>
        /// Sets the scenes that this window should be visible in
        /// </summary>
        /// <param name="scenes">Params array of scenes</param>
        public void SetVisibleScenes(params GameScenes[] scenes)
        {
            VisibleScenes = scenes.ToList();
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

        /// <summary>
        /// Checks to see if the mouse is over the window.
        /// Requires the window to be visible
        /// </summary>
        /// <returns>True if over the window, false otherwise.</returns>
        protected bool checkMouseOver()
        {
            if (!IsVisible)
            {
                return false;
            }
            Vector2 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y;
            return WindowRect.Contains(mousePos);
        }
    }
}
