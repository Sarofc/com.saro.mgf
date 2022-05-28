// using System;
// using System.Linq;
// using UnityEditor;
// using UnityEngine;

// internal class EventManipulationHandler
// {
//     private Rect[] m_EventRects = new Rect[0];
//     private static AnimationEvent[] m_EventsAtMouseDown;
//     private static float[] m_EventTimes;
//     private int m_HoverEvent = -1;

//     private string m_InstantTooltipText = null;
//     private Vector2 m_InstantTooltipPoint = Vector2.zero;

//     private bool[] m_EventsSelected;
//     private AnimationWindowEvent[] m_Events;

//     private TimeArea m_Timeline;

//     public EventManipulationHandler(TimeArea timeArea)
//     {
//         m_Timeline = timeArea;
//     }

//     public void SelectEvent(AnimationEvent[] events, int index, AnimationClip clip)
//     {
//         m_EventsSelected = new bool[events.Length];
//         m_EventsSelected[index] = true;

//         EditEvents(clip, m_EventsSelected);
//     }

//     public bool HandleEventManipulation(Rect rect, ref AnimationEvent[] events, AnimationClip clipInfo, float currentTime)
//     {
//         Texture eventMarker = EditorGUIUtility.IconContent("Animation.EventMarker").image;

//         bool hasChanged = false;

//         // Calculate rects
//         Rect[] hitRects = new Rect[events.Length];
//         Rect[] drawRects = new Rect[events.Length];
//         int shared = 1;
//         int sharedLeft = 0;
//         for (int i = 0; i < events.Length; i++)
//         {
//             AnimationEvent evt = events[i];

//             if (sharedLeft == 0)
//             {
//                 shared = 1;
//                 while (i + shared < events.Length && events[i + shared].time == evt.time)
//                     shared++;
//                 sharedLeft = shared;
//             }
//             sharedLeft--;

//             // Important to take floor of positions of GUI stuff to get pixel correct alignment of
//             // stuff drawn with both GUI and Handles/GL. Otherwise things are off by one pixel half the time.
//             float keypos = Mathf.Floor(m_Timeline.TimeToPixel(evt.time, rect));
//             int sharedOffset = 0;
//             if (shared > 1)
//             {
//                 float spread = Mathf.Min((shared - 1) * (eventMarker.width - 1), (int)(1.0f / m_Timeline.PixelDeltaToTime(rect) - eventMarker.width * 2));
//                 sharedOffset = Mathf.FloorToInt(Mathf.Max(0, spread - (eventMarker.width - 1) * (sharedLeft)));
//             }

//             Rect r = new Rect(
//                 keypos + sharedOffset - eventMarker.width / 2,
//                 (rect.height - 10) * (float)(sharedLeft - shared + 1) / Mathf.Max(1, shared - 1),
//                 eventMarker.width,
//                 eventMarker.height);

//             hitRects[i] = r;
//             drawRects[i] = r;
//         }

//         // Store rects used for tooltip testing
//         m_EventRects = new Rect[hitRects.Length];
//         for (int i = 0; i < hitRects.Length; i++)
//             m_EventRects[i] = new Rect(hitRects[i].x + rect.x, hitRects[i].y + rect.y, hitRects[i].width, hitRects[i].height);

//         // Selection control
//         if (m_EventsSelected == null || m_EventsSelected.Length != events.Length || m_EventsSelected.Length == 0)
//         {
//             m_EventsSelected = new bool[events.Length];
//             m_Events = null;
//         }

//         Vector2 offset = Vector2.zero;
//         int clickedIndex;
//         float startSelection, endSelection;

//         // TODO: GUIStyle.none has hopping margins that need to be fixed
//         HighLevelEvent hEvent = EditorGUIExt.MultiSelection(
//             rect,
//             drawRects,
//             new GUIContent(eventMarker),
//             hitRects,
//             ref m_EventsSelected,
//             null,
//             out clickedIndex,
//             out offset,
//             out startSelection,
//             out endSelection,
//             GUIStyle.none
//         );


//         if (hEvent != HighLevelEvent.None)
//         {
//             switch (hEvent)
//             {
//                 case HighLevelEvent.BeginDrag:
//                     m_EventsAtMouseDown = events;
//                     m_EventTimes = new float[events.Length];
//                     for (int i = 0; i < events.Length; i++)
//                         m_EventTimes[i] = events[i].time;
//                     break;
//                 case HighLevelEvent.SelectionChanged:
//                     EditEvents(clipInfo, m_EventsSelected);
//                     break;
//                 case HighLevelEvent.Delete:
//                     hasChanged = DeleteEvents(ref events, m_EventsSelected);
//                     break;
//                 case HighLevelEvent.Copy:
//                     AnimationWindowEventsClipboard.CopyEvents(events, m_EventsSelected);
//                     break;
//                 case HighLevelEvent.Paste:
//                     hasChanged = PasteEvents(ref events, ref m_EventsSelected, currentTime);
//                     if (hasChanged)
//                         EditEvents(clipInfo, m_EventsSelected);
//                     break;
//                 case HighLevelEvent.Drag:
//                     for (int i = events.Length - 1; i >= 0; i--)
//                     {
//                         if (m_EventsSelected[i])
//                         {
//                             AnimationEvent evt = m_EventsAtMouseDown[i];
//                             evt.time = Mathf.Clamp01(m_EventTimes[i] + (offset.x / rect.width));
//                         }
//                     }
//                     int[] order = new int[m_EventsSelected.Length];
//                     for (int i = 0; i < order.Length; i++)
//                     {
//                         order[i] = i;
//                     }
//                     System.Array.Sort(m_EventsAtMouseDown, order, new AnimationEventTimeLine.EventComparer());
//                     bool[] selectedOld = (bool[])m_EventsSelected.Clone();
//                     float[] timesOld = (float[])m_EventTimes.Clone();
//                     for (int i = 0; i < order.Length; i++)
//                     {
//                         m_EventsSelected[i] = selectedOld[order[i]];
//                         m_EventTimes[i] = timesOld[order[i]];
//                     }

//                     events = m_EventsAtMouseDown;
//                     hasChanged = true;
//                     break;

//                 case HighLevelEvent.ContextClick:
//                     CreateContextMenu(clip, events[clickedIndex].time, clickedIndex, m_EventsSelected);
//                     // Mouse may move while context menu is open - make sure instant tooltip is handled
//                     m_InstantTooltipText = null;
//                     break;
//             }
//         }

//         // Bring up menu when context-clicking on an empty timeline area (context-clicking on events is handled above)
//         if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition))
//         {
//             Event.current.Use();
//             float mousePosTime = Mathf.Max(m_Timeline.PixelToTime(Event.current.mousePosition.x, rect), 0.0f);
//             CreateContextMenu(clip, mousePosTime, -1, m_EventsSelected);
//             // Mouse may move while context menu is open - make sure instant tooltip is handled
//             m_InstantTooltipText = null;
//         }

//         CheckRectsOnMouseMove(rect, events, hitRects);

//         return hasChanged;
//     }

//     void CreateContextMenu(AnimationClip info, float time, int eventIndex, bool[] selectedEvents)
//     {
//         GenericMenu menu = new GenericMenu();
//         var ctx = new EventModificationContextMenuObject(info, time, eventIndex, selectedEvents);
//         var selectedCount = selectedEvents.Count(selected => selected);


//         menu.ShowAsContext();
//     }

//     private class EventModificationContextMenuObject
//     {
//         public AnimationClip m_Info;
//         public float m_Time;
//         public int m_Index;
//         public bool[] m_Selected;

//         public EventModificationContextMenuObject(AnimationClip info, float time, int index, bool[] selected)
//         {
//             m_Info = info;
//             m_Time = time;
//             m_Index = index;
//             m_Selected = selected;
//         }
//     }

//     public void EventLineContextMenuAdd(object obj)
//     {

//     }

//     public void EventLineContextMenuDelete(object obj)
//     {

//     }

//     static void EventLineContextMenuCopy(object obj)
//     {

//     }

//     void EventLineContextMenuPaste(object obj)
//     {

//     }

//     private void CheckRectsOnMouseMove(Rect eventLineRect, AnimationEvent[] events, Rect[] hitRects)
//     {
//         Vector2 mouse = Event.current.mousePosition;
//         bool hasFound = false;
//         m_InstantTooltipText = "";

//         if (events.Length == hitRects.Length)
//         {
//             for (int i = hitRects.Length - 1; i >= 0; i--)
//             {
//                 if (hitRects[i].Contains(mouse))
//                 {
//                     hasFound = true;
//                     if (m_HoverEvent != i)
//                     {
//                         m_HoverEvent = i;
//                         m_InstantTooltipText = events[m_HoverEvent].functionName;
//                         m_InstantTooltipPoint = new Vector2(mouse.x, mouse.y);
//                     }
//                 }
//             }
//         }
//         if (!hasFound)
//             m_HoverEvent = -1;
//     }

//     public void Draw(Rect window)
//     {
//         EditorGUI.indentLevel++;
//         // if (m_Events != null && m_Events.Length > 0)
//         //     AnimationWindowEventInspector.OnEditAnimationEvents(m_Events);
//         // else
//         //     AnimationWindowEventInspector.OnDisabledAnimationEvent();

//         EditorGUI.indentLevel--;

//         if (!string.IsNullOrEmpty(m_InstantTooltipText))
//         {
//             // Draw body of tooltip
//             GUIStyle style = (GUIStyle)"AnimationEventTooltip";
//             Vector2 size = style.CalcSize(new GUIContent(m_InstantTooltipText));
//             Rect rect = new Rect(window.x + m_InstantTooltipPoint.x, window.y + m_InstantTooltipPoint.y, size.x, size.y);

//             // Right align tooltip rect if it would otherwise exceed the bounds of the window
//             if (rect.xMax > window.width)
//                 rect.x = window.width - rect.width;

//             GUI.Label(rect, m_InstantTooltipText, style);
//         }
//     }

//     public bool DeleteEvents(ref AnimationEvent[] eventList, bool[] deleteIndices)
//     {
//         bool deletedAny = false;

//         for (int i = eventList.Length - 1; i >= 0; i--)
//         {
//             if (deleteIndices[i])
//             {
//                 ArrayUtility.RemoveAt(ref eventList, i);
//                 deletedAny = true;
//             }
//         }

//         if (deletedAny)
//         {
//             m_EventsSelected = new bool[eventList.Length];
//             m_Events = null;
//         }

//         return deletedAny;
//     }

//     static bool PasteEvents(ref AnimationEvent[] eventList, ref bool[] selected, float time)
//     {

//         return true;
//     }

//     public void EditEvents(AnimationClip clipInfo, bool[] selectedIndices)
//     {

//     }

//     public void UpdateEvents(AnimationClip clipInfo)
//     {

//     }

// }