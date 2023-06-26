using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Rimvention
{
    public class UIDrawEntry
    {
        private string _entryLabel;
        private string _entryDescription;
        private Texture2D _entryImage;
        private Thing _entryAttachedThing;
        private Pawn _entryAttachedPawn;
        private Rect _entryRect;
        private Rect _entryParentRect; // RW UI works where each rect (and every UI element "inside" it) share their vec2 pos relative to the parent Rect
                                         // meaning Rect pos from a different UI element will not match another (due to the parent rect)
        private bool _entryIsDraggable;
        private RimventionUIElement _entryUIInfo;

        public string EntryLabel
        {
            get
            {
                return _entryLabel;
            }
        }
        public string EntryBasicDesc
        {
            get
            {
                return _entryDescription;
            }
        }
        public Thing EntryAttachedThing { get => _entryAttachedThing; }
        public Rect EntryRect { get => _entryRect; }
        public Texture2D EntryImage { get => _entryImage; set => _entryImage = value; }        
        public RimventionUIElement EntryUIInfo { get => _entryUIInfo; set => _entryUIInfo = value; }       
        public Rect EntryParentRect { get => _entryParentRect; set => _entryParentRect = value; }
        public bool EntryIsDraggable { get => _entryIsDraggable; }

        public UIDrawEntry(string label, string basicDescription)
        {
            _entryLabel = label;
            _entryDescription = basicDescription;
        }

        public UIDrawEntry(string label, string basicDesc, Texture2D image)
        {
            _entryLabel = label;
            _entryDescription = basicDesc;
            _entryImage = image;
        }

        public UIDrawEntry(string label, string basicDesc, Texture2D image, Rect inRect, bool isDraggable)
        {
            _entryLabel = label;
            _entryDescription = basicDesc;
            _entryImage = image;
            _entryRect = inRect;
            _entryIsDraggable = isDraggable;
        }

        public UIDrawEntry(string label, Thing thing)
        {
            _entryLabel = label;
            _entryAttachedThing = thing;
        }

        public void UpdateElementDescription()
        {
            _entryDescription = this._entryUIInfo.PartCount.ToString();
        }

        public float DrawImageInvisible(float x, float y, float width, Texture2D image)
        {
            float width1 = width * 0.45f;
            float height1 = y - 22;
            Rect imgRect = new Rect(x, y, image.width / 3, image.height / 3);
            return imgRect.height;
        }

        public float DrawImage(float x, float y, float width, Texture2D image, Action<UIDrawEntry> clickedCallback, Action<UIDrawEntry> mousedOverCallback, Vector2 scrollPosition, Rect scrollOutRect, bool selected = false)
        {
            float width1 = width * 0.45f;
            float height1 = y - 22;
            Rect labelRect = new Rect(x, height1, width, Verse.Text.CalcHeight("test", width1));
            Rect imgRect = new Rect(x, y, image.width / 3, image.height / 3);
            //imgRect.y += labelRect.height;
            //Widgets.ButtonImage(imgRect, image, false); 

            // check for mouse pos and input 
            if ((double)y - (double)scrollPosition.y + (double)imgRect.height >= 0.0 && (double)y - (double)scrollPosition.y <= (double)scrollOutRect.height)
            {
                if (selected)
                    Widgets.DrawHighlightSelected(imgRect);
                else if (Mouse.IsOver(imgRect))
                    Widgets.DrawHighlight(imgRect);
                Rect rect2 = labelRect;
                rect2.width -= width1;
                Widgets.Label(rect2, _entryDescription);

                if (Widgets.ButtonImage(imgRect, image, true))
                {
                    clickedCallback(this);
                }

                if (Mouse.IsOver(imgRect))
                {
                    TooltipHandler.TipRegion(imgRect, this.EntryUIInfo.UIText);
                    mousedOverCallback(this);
                }

            }

            return imgRect.height;
        }
       
        public float Draw(float x, float y, float width, bool selected, Action<UIDrawEntry> clickedCallback, Action<UIDrawEntry> mousedOverCallback, Vector2 scrollPosition, Rect scrollOutRect)
        {
            float width1 = width * 0.45f;
            Rect rect1 = new Rect(x, y, width, Verse.Text.CalcHeight("test", width1));
            if ((double)y - (double)scrollPosition.y + (double)rect1.height >= 0.0 && (double)y - (double)scrollPosition.y <= (double)scrollOutRect.height)
            {
                if (selected)
                    Widgets.DrawHighlightSelected(rect1);
                else if (Mouse.IsOver(rect1))
                    Widgets.DrawHighlight(rect1);
                Rect rect2 = rect1;
                rect2.width -= width1;
                Widgets.Label(rect2, _entryLabel);

                // this 3rd rect is used for entry specific statistics like percentages and in game values.
                /*Rect rect3 = rect1;
                rect3.x = rect2.xMax;
                rect3.width = width1;
                Widgets.Label(rect3, EntryBasicDesc);*/


                if (Widgets.ButtonInvisible(rect1, false))
                {
                    clickedCallback(this);
                }

                if (Mouse.IsOver(rect1))
                {
                    mousedOverCallback(this);
                }
            }
            return rect1.height;
        }
    }
}
