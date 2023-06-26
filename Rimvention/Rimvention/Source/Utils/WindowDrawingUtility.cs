using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace Rimvention
{
    /// <summary>
    /// Revamped version of my ImmersiveResearch Drawing Util class
    /// Now has capability to handle storage and drawing of all UI elements per "context" - i.e. each instance of this class
    /// Good practice would really be to use this per UI Window though I think.
    /// </summary>
    public class WindowDrawingUtility
    {
       
        private Vector2 _scrollPosition;
        private float _listHeight;
        private UIDrawEntry _selectedEntry;
        private UIDrawEntry _mousedOverEntry;
        private Dictionary<int,List<UIDrawEntry>> _cachedDrawEntries = new Dictionary<int, List<UIDrawEntry>>();

        bool paintable;

        public UIDrawEntry SelectedEntry
        {
            get
            {
                return _selectedEntry;
            }
        }

        public List<UIDrawEntry> GetCachedEntriesByID(int id)
        {
            if (_cachedDrawEntries.ContainsKey(id))
            {
                return _cachedDrawEntries[id];
            }
            else
            {
                return null;
            }
        }

        private List<UIDrawEntry> InitPreDraw(int UIElementID)
        {
            return _cachedDrawEntries[UIElementID];
        }

        private UIDrawEntry CreateNewImageEntry(Rect rect, RimventionUIElement image, bool draggable, bool hasDescription = true)
        {
            if (hasDescription)
            { // MAYBE - change this so partcount is specific to part storage - want this class to be generic as possible for reusability
                UIDrawEntry newEntry = new UIDrawEntry(image.PartName, image.PartCount.ToString(), image.UIIcon, rect, draggable);
                newEntry.EntryUIInfo = image;
                return newEntry;
            }
            else if(image.ImbueInfo != null)
            { 
                var info = image.ImbueInfo;
                UIDrawEntry newEntry = new UIDrawEntry(info.ImbueName, "", image.UIIcon, rect, draggable);
                newEntry.EntryUIInfo = image;
                return newEntry;
            }
            else
            {// MAYBE - pretty sure UIInfo is not needed to be initialised for this - we arent making use of them for anything
                UIDrawEntry newEntry = new UIDrawEntry(image.PartName, "", image.UIIcon, rect, draggable);
                newEntry.EntryUIInfo = image;
                return newEntry;
            }    
        }

        public void InitSingleImage(RimventionUIElement image, int UIElementID, Vector2 startPos, bool hasTextLabels, bool isDraggable = false)
        {
            if(image == null)
            {
                Log.Error("ID " + UIElementID + ": image is null");
                return;
            }

            // setup layout
            float rowHeight = image.UIIcon.height / 3;
            float columnWidth = image.UIIcon.width / 3;

            // MAYBE - change cachedentries to diff structure - think of perf of using List for even single ui elements
            var UIElements = new List<UIDrawEntry>();
            _cachedDrawEntries.Add(UIElementID, UIElements);

            var r1 = new Rect(startPos.x, startPos.y, rowHeight, columnWidth);
            _cachedDrawEntries[UIElementID].Add(CreateNewImageEntry(r1, image, isDraggable, false));
        }

        public void InitImageTable(List<RimventionUIElement> images, int UIElementID, int maxColumns, Vector2 startPos, bool hasTextLabelsNeedOffset, bool isDraggable = false)
        {
            if (images.NullOrEmpty())
            {
                Log.Error("ID " + UIElementID + ": images list is null or empty.");
                return;
            }

            // setup table layout
            float rowHeight = images[0].UIIcon.height / 3;
            float columnWidth = images[0].UIIcon.width / 3;
            // offset for text labels 
            float curY = hasTextLabelsNeedOffset == true ? curY = 22.0f : curY = 0f; 
            int rowNum = images.Count % maxColumns;

            if (!_cachedDrawEntries.ContainsKey(UIElementID))
            {
                var UIElements = new List<UIDrawEntry>();
                _cachedDrawEntries.Add(UIElementID, UIElements);
                int curImageIndex = 0;
                // Looks gross but all it does it assemble a table of images one row at a time
                for (int i = 0; i < images.Count; i += maxColumns)
                {    
                    // create the first row
                    if (_cachedDrawEntries[UIElementID].NullOrEmpty())
                    {
                        var r1 = new Rect(startPos.x, startPos.y, rowHeight, columnWidth);
                        _cachedDrawEntries[UIElementID].Add(CreateNewImageEntry(r1, images[i], isDraggable));
                        curImageIndex++;

                        for (int j = 1; j < maxColumns; j++)
                        {
                            if (j > images.Count-1) // check to stop index errors if num of elements per row goes over column count
                                return;
                            if (j == 1)
                            {
                                var r2 = new Rect(_cachedDrawEntries[UIElementID][0].EntryRect.xMax + 5f, _cachedDrawEntries[UIElementID][0].EntryRect.y, rowHeight, columnWidth);
                                _cachedDrawEntries[UIElementID].Add(CreateNewImageEntry(r2, images[curImageIndex], isDraggable));
                                curImageIndex++;
                            }
                            else
                            {
                                var r3 = new Rect(_cachedDrawEntries[UIElementID][j-1].EntryRect.xMax + 5f, _cachedDrawEntries[UIElementID][0].EntryRect.y, rowHeight, columnWidth);
                                _cachedDrawEntries[UIElementID].Add(CreateNewImageEntry(r3, images[curImageIndex], isDraggable));
                                curImageIndex++;
                            }                           
                        }
                    }
                    else // create any extra rows
                    {
                        for (int j = 0; j < maxColumns; j++)
                        {
                            if (j > images.Count)
                                return;
                            var r3 = new Rect(_cachedDrawEntries[UIElementID][j].EntryRect.x, curY, rowHeight, columnWidth);
                            _cachedDrawEntries[UIElementID].Add(CreateNewImageEntry(r3, images[curImageIndex], isDraggable));
                            curImageIndex++;
                        }
                    }
                    curY = hasTextLabelsNeedOffset == true ? curY += rowHeight + 22f : curY += rowHeight;
                }               
            }
        }

        public void DrawSingleImage(Rect inRect, int UIElementID, string listTitle = "", bool isDraggable = false, bool hasScrollView = true)
        {
            DrawListWorker(inRect, InitPreDraw(UIElementID), listTitle, true, isDraggable, hasScrollView);
        }

        public void DrawStaticTextList(Rect inRect, int UIElementID, List<string> thingList, string listTitle)
        {
            if (!_cachedDrawEntries.ContainsKey(UIElementID))
            {
                var UIElements = new List<UIDrawEntry>();
                _cachedDrawEntries.Add(UIElementID, UIElements);
                foreach (string str in thingList)
                {
                    string label = str;
                    string desc = "";
                    UIDrawEntry newEntry = new UIDrawEntry(label, desc);      
                    _cachedDrawEntries[UIElementID].Add(newEntry);
                }
            }
            DrawListWorker(inRect, InitPreDraw(UIElementID), listTitle, false, false);
        }

        public void DrawDynamicTextList(Rect inRect, int UIElementID, List<string> thingList, string listTitle)
        {
            if (!_cachedDrawEntries.ContainsKey(UIElementID))
            {
                var UIElements = new List<UIDrawEntry>();
                _cachedDrawEntries.Add(UIElementID, UIElements);
                foreach (string str in thingList)
                {
                    string label = str;
                    string desc = "";
                    UIDrawEntry newEntry = new UIDrawEntry(label, desc);
                    _cachedDrawEntries[UIElementID].Add(newEntry);
                }
            }
            else
            {
                _cachedDrawEntries[UIElementID].Clear();
                foreach (string str in thingList)
                {
                    string label = str;
                    string desc = "";
                    UIDrawEntry newEntry = new UIDrawEntry(label, desc);
                    _cachedDrawEntries[UIElementID].Add(newEntry);
                }
            }
            DrawListWorker(inRect, InitPreDraw(UIElementID), listTitle, false, false);
        }

        public void DrawTextListWithAttachedThing(Rect inRect, int UIElementID, List<Tuple<string, Thing>> thingList, string listTitle)
        {
            if (!_cachedDrawEntries.ContainsKey(UIElementID))
            {
                var UIElements = new List<UIDrawEntry>();
                _cachedDrawEntries.Add(UIElementID, UIElements);
                foreach (var str in thingList)
                {
                    string label = str.Item1;
                    UIDrawEntry newEntry = new UIDrawEntry(label, str.Item2);
                    _cachedDrawEntries[UIElementID].Add(newEntry);
                }
            }

            DrawListWorker(inRect, InitPreDraw(UIElementID), listTitle, false, false);
        }

        public void DrawImageList(Rect inRect, int UIElementID, string listTitle, bool isDraggable = false, bool hasScrollView = true, bool isInvisible = false)
        {
            DrawListWorker(inRect, InitPreDraw(UIElementID), listTitle, true, isDraggable, hasScrollView, isInvisible);
        }

       public void DrawDragImageArea(Rect parent, int UIElementID)
        {
            // To handle "draggable" areas, just re-draw a Rect at the original rects absolute coords
            for (int i = 0; i < _cachedDrawEntries[UIElementID].Count; i++)
            {
                var elems = _cachedDrawEntries[UIElementID];
                var temp = parent.position + elems[i].EntryRect.position;

                temp.y += elems[i].EntryRect.height - 2f;
                elems[i].EntryParentRect = new Rect(temp.x, temp.y, elems[i].EntryRect.width, elems[i].EntryRect.height);

                // using for bootleg "highlight on mouseover"
                if (Widgets.ButtonImage(elems[i].EntryParentRect, elems[i].EntryImage))
                {
                }
            }
        }

        // ripped str8 from internal RW class thnx (aka not my code)
        private bool CheckDraggable(Widgets.DraggableResult result)
        {
            if (result != Widgets.DraggableResult.Pressed)
            {
                return result == Widgets.DraggableResult.DraggedThenPressed;
            }
            return true;
        }

        public void CraftingAreaSetter(UIDrawEntry entry)
        {
            if (_cachedDrawEntries.ContainsKey(1)) // key of whatever crafting area is
            {
                // basically check that we are already dragging something, otherwise it would drag whatever new thing mouse goes over
                if (entry != null && _selectedEntry != null && entry.EntryUIInfo.PartName != _selectedEntry.EntryUIInfo.PartName)
                    return;

                if (entry.EntryIsDraggable)
                {
                    var drag = Widgets.ButtonInvisibleDraggable(entry.EntryRect);
                    if (drag == Widgets.DraggableResult.Dragged)
                    {
                        paintable = true;
                    }
                        
                    if (paintable || CheckDraggable(drag))
                    {
                        // handles drag icons next to cursor
                        // RW has this already implemented but wouldnt take context mouse pos into consideration for some reason and couldnt pass in an offset
                        var dragRect = new Rect(Event.current.mousePosition.x + 650f, Event.current.mousePosition.y + 350f, 32f, 32f);
                        Find.WindowStack.ImmediateWindow(34003428, dragRect, WindowLayer.Super, delegate
                        {
                            GUI.DrawTexture(dragRect.AtZero(), entry.EntryImage);
                        });
                        
                        for (int i = 0; i < _cachedDrawEntries[1].Count; i++)
                        {
                            // Insanely hacky impl of drag and drop in rimworld
                            // RW uses Unity editor GUI (it's an old game) which is very finicky to use
                            // it uses a diff coord system (origin is top left) so mouse pos is either to get it normally thru input and have to recalc the Y axis
                            // or use the IMGUI event system to get the mouse pos, which knows its context and works "out of the box"
                            // this means that any UI elements that are 'inside' of another e.g. within a GUI group, the mouse pos will work within that group,
                            // and any mouse pos outside of that group visually will not update (that I know of) until the mouse context changes e.g. a new click event
                            var mouseOffset = Event.current.mousePosition;
                            mouseOffset.y += _cachedDrawEntries[1][i].EntryParentRect.y;
                            mouseOffset.x += _cachedDrawEntries[1][i].EntryParentRect.width * 2;

                            if (_cachedDrawEntries[1][i].EntryParentRect.Contains(mouseOffset) && !Input.GetMouseButton(0))
                            {                               
                                for (int j = 0; j < _cachedDrawEntries[0].Count; j++)
                                {
                                    if(_cachedDrawEntries[1][i].EntryUIInfo.PartName == _cachedDrawEntries[0][j].EntryUIInfo.PartName)
                                    {
                                        _cachedDrawEntries[0][j].EntryUIInfo.PartCount++.ToString();
                                        _cachedDrawEntries[0][j].UpdateElementDescription();
                                    }
                                }
                                entry.EntryUIInfo.PartCount--;
                                _cachedDrawEntries[1][i].EntryUIInfo = entry.EntryUIInfo;
                                _cachedDrawEntries[1][i].EntryImage = entry.EntryImage;                               
                                entry.UpdateElementDescription();
                            }
                        }
                    }
                    if (!Input.GetMouseButton(0) || entry.EntryUIInfo.PartCount <= 0)
                    {
                        paintable = false;
                        _selectedEntry = null;
                    }
                }               
            }
        }

        // Draw our list to the UI
        // originally just ripped from original source code, jsut changed in a few areas for my needs
        private void DrawListWorker(Rect refRect, List<UIDrawEntry> cachedElements, string listTitle, bool hasImage, bool hasDraggableImage, bool hasScrollView = true, bool isInvisible = false)
        {
            Rect titleRect = new Rect(refRect);
            titleRect.x = refRect.xMin + 50f;
            titleRect.y = refRect.yMin;
            Widgets.Label(titleRect, listTitle);

            Rect rect1 = new Rect(refRect);
            rect1.width *= 0.5f;
            rect1.y += 25f;
            Rect rect2 = new Rect(refRect);
            rect2.x = rect1.xMax;
            rect2.width = refRect.xMax - rect2.x;
            Text.Font = GameFont.Small;
            Rect viewRect = new Rect(0.0f, 0.0f, rect1.width - 16f, _listHeight);
            Widgets.DrawMenuSection(rect1);
            if(hasScrollView)
                Widgets.BeginScrollView(rect1, ref _scrollPosition, viewRect, true);

            float curY = 0.0f;
            
            _mousedOverEntry = null;

            if (hasImage)
            {
                for (int i = 0; i < cachedElements.Count; i++)
                {
                    Action<UIDrawEntry> mouseClickEvent = MouseClickCallBackEvent;
                    Action<UIDrawEntry> mouseOverEvent = MouseOverCallBackEvent;

                    if (hasDraggableImage)
                    {
                        curY += cachedElements[i].DrawImage(cachedElements[i].EntryRect.x, cachedElements[i].EntryRect.y, viewRect.width - 8f, cachedElements[i].EntryImage,
                                                            mouseClickEvent, mouseOverEvent, _scrollPosition, rect1);
                        
                        // this crusty code is setting our drag option (i hate unity imgui)
                        // just checks for input over drag element, then makes sure that it doesnt change if you drag over another draggable element
                        if(Input.GetMouseButton(0))
                        {
                            if (Mouse.IsOver(cachedElements[i].EntryRect))
                            {
                                if(_selectedEntry == null)
                                {
                                    _selectedEntry = cachedElements[i];
                                }
                                if(_selectedEntry != cachedElements[i])
                                {
                                    //Log.Error("hovering over when already drag");
                                }
                            }                         
                        }
                        CraftingAreaSetter(cachedElements[i]);
                    }
                    else if (isInvisible)
                    {
                        curY += cachedElements[i].DrawImageInvisible(cachedElements[i].EntryRect.x, cachedElements[i].EntryRect.y, viewRect.width - 8f, cachedElements[i].EntryImage);
                    }
                    else
                    { // default
                        curY += cachedElements[i].DrawImage(cachedElements[i].EntryRect.x, cachedElements[i].EntryRect.y, viewRect.width - 8f, cachedElements[i].EntryImage,
                                                            mouseClickEvent, mouseOverEvent, _scrollPosition, rect1);
                    }
                }
            }
            else // Draw text
            {
                for (int i = 0; i < cachedElements.Count; i++)
                {
                    Action<UIDrawEntry> mouseClickEvent = MouseClickCallBackEvent;
                    Action<UIDrawEntry> mouseOverEvent = MouseOverCallBackEvent;

                    curY += cachedElements[i].Draw(8f, curY, viewRect.width - 8f,
                        false, mouseClickEvent, mouseOverEvent, _scrollPosition, rect1);
                }
            }

            _listHeight = curY;
            if(hasScrollView)
                Widgets.EndScrollView();
        }


        private void SelectEntry(UIDrawEntry entry, bool playSound = true)
        {
            _selectedEntry = entry;
            /*Log.Error("local");
            Log.Error("X: " + (entry.EntryRect.x).ToString() + " Y: " + (entry.EntryRect.y).ToString());
            Log.Error("screen");
            var global = GUIUtility.GUIToScreenRect(entry.EntryRect);
            Log.Error("X: " + (global.x).ToString() + " Y: " + (global.y).ToString());
            Log.Error("Parent Pos");
            Log.Error("X: " + entry.EntryParentRect.x.ToString() + " Y: " + entry.EntryParentRect.y.ToString());
            Log.Error("parent pos - rect pos");
            var test = entry.EntryParentRect.position - entry.EntryRect.position;
            Log.Error("X: " + test.x.ToString() + " Y: " + test.y.ToString());
            Log.Error("mouse pos");
            Log.Error("X: " + Event.current.mousePosition.x.ToString() + " Y: " + Event.current.mousePosition.y.ToString());*/
            //Log.Error(_selectedEntry.EntryUIInfo.PartName);
            if (!playSound)
            {
                return;
            }
            else
            {
                return;
            }
        }

        // Mouse events 
        private void MouseClickCallBackEvent(UIDrawEntry r)
        {
            SelectEntry(r);
        }

        private void MouseOverCallBackEvent(UIDrawEntry r)
        {           
            _mousedOverEntry = r;
            
        }

    }
}
