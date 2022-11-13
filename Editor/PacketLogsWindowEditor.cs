using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor;
using UnityEngine;
// ReSharper disable UnusedMember.Local
// ReSharper disable NotAccessedField.Local
// ReSharper disable once CheckNamespace
namespace HttpTransport.Transports.Http.Editor
{
    public class PacketLogsWindowEditor : EditorWindow
    {
        public static void Open(IHttpTransportLogs logs)
        {
            var window = GetWindow<PacketLogsWindowEditor>("RPC");
            window._logs = logs;
            window.Show(true);
        }

        //private PacketLog[] packets;
        private Vector2 _scrollPosition;
        private PathValue[] _path;
        private bool _invalidUpdate;
        private SortBy _sortBy = SortBy.TIME_UP;
        private object _executeResult;
        private int _toggleContent;
        private IHttpTransportLogs _logs;

        private void OnGUI()
        {
            var isWait = false;

            if (_logs == null) return;
            if ((_path == null || _invalidUpdate) && Application.isPlaying)
            {
                if (_executeResult == null)
                {
                    _path = null;
                    EditorApplication.update += Repaint;

                    List<PathValue> list = new List<PathValue>();
                    _executeResult = new object();
                    ThreadPoolExecutor.ExecuteInBackground(
                            inBackground: () =>
                            {
                                if (_logs.Collection.Length != 0)
                                {
                                    foreach (var log in _logs.Collection)
                                    {
                                        list.Add(new PathValue
                                        {
                                            Name = log.Id.ToString("0000") + ": " + log.Key,
                                            Value = this._toggleContent == 0 ? log.Value : log.Content,
                                            Message = this._toggleContent == 0
                                                    ? Substring(ObjectUtils.Inspect(log.Value, true, false))
                                                    : Substring(ObjectUtils.Inspect(log.Content, true, false)),
                                            Time = log.Time,
                                            IO = (PathIO)(int)log.Io
                                        });
                                    }
                                }

                                _invalidUpdate = false;

                            },
                            onCompleteInMainThread: () =>
                            {
                                EditorApplication.update -= Repaint;
                                _path = new[]
                                {
                                    new PathValue
                                    {
                                        Name = "root",
                                        Value = list
                                    }
                                };
                                _sortBy = SortBy.TIME_UP;
                                _executeResult = null;
                            });
                }
                else
                {
                    isWait = true;
                }
            }

            GUI.enabled = _path != null;
            DrawHeader(_logs);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            if (isWait)
            {
                var content = new GUIContent("WAIT...");
                GUIStyle label = EditorStyles.boldLabel;
                var size = label.CalcSize(content);
                var rect = EditorGUILayout.GetControlRect(true, size.y);
                GUI.Label(rect, content);
            }
            else
            {
                DrawContent();
            }

            GUI.enabled = true;
            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader(IHttpTransportLogs logs)
        {
            DrawHeaderControls(logs);
            EditorGUILayout.Space();
            DrawPath();
        }

        private void DrawHeaderControls(IHttpTransportLogs logs)
        {
            var content = new GUIContent("clear");
            var size = EditorStyles.miniButtonMid.CalcSize(content);
            var rect = EditorGUILayout.GetControlRect(true, size.y);
            rect.x = 0;
            rect.width = size.x + 10;
            rect.height = size.y;
            EditorUtils.PushColor();
            GUI.color = Color.red;
            if (GUI.Button(rect, content, EditorStyles.miniButtonLeft))
            {
                logs.Clear();
                _path = null;
                _invalidUpdate = true;
            }

            rect.x += rect.xMax;
            content.text = "update";
            EditorUtils.PopColor();
            EditorUtils.PushColor();
            GUI.color = Color.cyan;
            if (GUI.Button(rect, content, EditorStyles.miniButtonRight))
            {
                _invalidUpdate = true;
            }

            GUI.color = Color.white;
            rect.x += rect.width + 16;
            rect.width += 10f;
            content.text = "content";
            if (GUI.Button(rect, content, EditorStyles.miniButtonLeft))
            {
                this._toggleContent = 0;
                _invalidUpdate = true;
            }

            rect.x += rect.width;
            content.text = "message";

            if (GUI.Button(rect, content, EditorStyles.miniButtonRight))
            {
                this._toggleContent = 1;
                _invalidUpdate = true;
            }

            EditorUtils.PopColor();
        }

        private void DrawPath()
        {
            float posX = 0f;
            var content = new GUIContent("root");
            var size = EditorStyles.miniButtonMid.CalcSize(content);
            var rect = EditorGUILayout.GetControlRect(true, size.y);
            GUI.Box(rect, "");
            if (_path == null)
            {
                rect.x = 0;
                rect.width = size.x;
                rect.height = size.y;
                GUI.Button(rect, content, EditorStyles.miniButtonMid);
                return;
            }

            for (int i = 0; i < _path.Length; i++)
            {
                content.text = "\u25BA " + Regex.Unescape(_path[i].Name.Replace('\t', ' '));
                size = EditorStyles.miniButtonMid.CalcSize(content);
                rect.x = posX;
                rect.width = size.x;
                rect.height = size.y;
                var lastColor = GUI.color;
                if (i == _path.Length - 1)
                {
                    GUI.color = Color.green;
                }
                else if (i == 0)
                {
                    GUI.color = Color.cyan;
                }

                var click = GUI.Button(rect, content, EditorStyles.miniButtonMid);
                GUI.color = lastColor;
                posX += size.x;
                if (click)
                {
                    Array.Resize(ref _path, i + 1);
                    if (_path.Length == 1)
                    {
                        _sortBy = SortBy.TIME_UP;
                    }

                    break;
                }
            }
        }

        private void DrawContentHeader()
        {
            float posX = 0f;
            var content = new GUIContent("");
            GUIStyle flowBackground = EditorUtils.Styles.FlowBackground;
            var size = flowBackground.CalcSize(content);
            var rect = EditorGUILayout.GetControlRect(true, size.y);
            var totalRect = rect;
            rect.x = 0;
            rect.y -= 1;
            rect.width += 4;
            GUI.Box(rect, content, flowBackground);

            GUIStyle toolbarbutton = EditorUtils.Styles.ToolbarButton;
            size = toolbarbutton.CalcSize(content);
            rect = EditorGUILayout.GetControlRect(true, size.y);
            rect.x = 0;
            rect.y -= 2;
            rect.width += 4;
            GUI.Box(rect, content, toolbarbutton);

            content.text = "NAME ";
            if (_sortBy == SortBy.NAME_UP) content.text += "▲";
            if (_sortBy == SortBy.NAME_DOWN) content.text += "▼";
            rect.width = 300f;
            rect.x = posX + 1;
            posX = rect.xMax;
            if (GUI.Button(rect, content, EditorUtils.Styles.MiniToolbarButton))
            {
                if (_sortBy == SortBy.NAME_DOWN) _sortBy = SortBy.NAME_UP;
                else if (_sortBy == SortBy.NAME_UP) _sortBy = SortBy.NAME_DOWN;
                else _sortBy = SortBy.NAME_UP;
            }

            content.text = "VALUE ";
            if (_sortBy == SortBy.VALUE_UP) content.text += "▲";
            if (_sortBy == SortBy.VALUE_DOWN) content.text += "▼";
            rect.width = totalRect.width - rect.xMax + 2;
            rect.x = posX + 1;
            if (GUI.Button(rect, content, EditorUtils.Styles.MiniToolbarButton))
            {
                if (_sortBy == SortBy.VALUE_DOWN) _sortBy = SortBy.VALUE_UP;
                else if (_sortBy == SortBy.VALUE_UP) _sortBy = SortBy.VALUE_DOWN;
                else _sortBy = SortBy.VALUE_UP;
            }
        }

        private void DrawRootContentHeader()
        {
            var content = new GUIContent("");
            GUIStyle flowBackground = EditorUtils.Styles.FlowBackground;
            var size = flowBackground.CalcSize(content);
            var rect = EditorGUILayout.GetControlRect(true, size.y);
            var totalRect = rect;
            rect.x = 0;
            rect.y -= 1;
            rect.width += 4;
            GUI.Box(rect, content, flowBackground);

            GUIStyle toolbarbutton = EditorUtils.Styles.ToolbarButton;
            size = toolbarbutton.CalcSize(content);
            rect = EditorGUILayout.GetControlRect(true, size.y);
            rect.x = 0;
            rect.y -= 2;
            rect.width += 4;
            GUI.Box(rect, content, toolbarbutton);

            content.text = "TIME ";
            if (_sortBy == SortBy.TIME_UP) content.text += "▲";
            if (_sortBy == SortBy.TIME_DOWN) content.text += "▼";
            rect.width = 120f;
            rect.y += 1;
            var posX = rect.xMax;
            if (GUI.Button(rect, content, EditorUtils.Styles.MiniToolbarButton))
            {
                if (_sortBy == SortBy.TIME_DOWN) _sortBy = SortBy.TIME_UP;
                else if (_sortBy == SortBy.TIME_UP) _sortBy = SortBy.TIME_DOWN;
                else _sortBy = SortBy.TIME_UP;
            }

            content.text = "I/O ";
            if (_sortBy == SortBy.TYPE_IN) content.text += "▲";
            if (_sortBy == SortBy.TYPE_OUT) content.text += "▼";
            rect.width = 60f;
            rect.x = posX + 1;
            posX = rect.xMax;
            if (GUI.Button(rect, content, EditorUtils.Styles.MiniToolbarButton))
            {
                if (_sortBy == SortBy.TYPE_OUT) _sortBy = SortBy.TYPE_IN;
                else if (_sortBy == SortBy.TYPE_IN) _sortBy = SortBy.TYPE_OUT;
                else _sortBy = SortBy.TYPE_IN;
            }

            content.text = "NAME ";
            if (_sortBy == SortBy.NAME_UP) content.text += "▲";
            if (_sortBy == SortBy.NAME_DOWN) content.text += "▼";
            rect.width = 300f;
            rect.x = posX + 1;
            posX = rect.xMax;
            if (GUI.Button(rect, content, EditorUtils.Styles.MiniToolbarButton))
            {
                if (_sortBy == SortBy.NAME_DOWN) _sortBy = SortBy.NAME_UP;
                else if (_sortBy == SortBy.NAME_UP) _sortBy = SortBy.NAME_DOWN;
                else _sortBy = SortBy.NAME_UP;
            }

            content.text = "VALUE ";
            if (_sortBy == SortBy.VALUE_UP) content.text += "▲";
            if (_sortBy == SortBy.VALUE_DOWN) content.text += "▼";
            ((GUIStyle)EditorUtils.Styles.MiniToolbarButton).CalcSize(content);
            rect.width = totalRect.width - rect.xMax + 2;
            rect.x = posX + 1;
            if (GUI.Button(rect, content, EditorUtils.Styles.MiniToolbarButton))
            {
                if (_sortBy == SortBy.VALUE_DOWN) _sortBy = SortBy.VALUE_UP;
                else if (_sortBy == SortBy.VALUE_UP) _sortBy = SortBy.VALUE_DOWN;
                else _sortBy = SortBy.VALUE_UP;
            }
        }

        private void DrawContent()
        {
            if (_path == null || _path.Length == 0)
            {
                DrawRootContentHeader();
                return;
            }

            if (_path.Length <= 1)
            {
                DrawRootContentHeader();
            }
            else
            {
                DrawContentHeader();
            }

            EditorGUILayout.BeginVertical();
            if (_path.Length <= 1)
            {
                DrawRootContent();
            }
            else
            {
                DrawValueContent();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawValueContent()
        {
            var current = _path.Last();
            var enumerable = current.Value as IEnumerable<PathValue>;
            if (enumerable != null)
            {
                var sorted = Sort(enumerable, _sortBy);
                var index = true;

                var content = new GUIContent("");
                GUIStyle minibuttonmid = EditorUtils.Styles.MiniButtonMid;
                var size = minibuttonmid.CalcSize(content);
                var rect = EditorGUILayout.GetControlRect(true, size.y);
                var totalRect = rect;
                rect.x = 0;
                rect.y -= 1;
                rect.height += 3;
                rect.width += 4;

                EditorUtils.PushColor();

                var lastAlign = GUI.skin.label.alignment;

                foreach (var value in sorted)
                {
                    if (index) EditorUtils.PopColor();
                    else
                    {
                        EditorUtils.PushColor();
                        GUI.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                    }

                    GUI.Box(rect, content, EditorStyles.helpBox);


                    var labelRect = rect;
                    labelRect.width = 120f;
                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    labelRect.width = 300f;
                    if (GUI.Button(labelRect, "  " + value.Name, EditorStyles.label))
                    {
                        AddPath(value);
                    }

                    var valueRect = labelRect;
                    valueRect.x = labelRect.xMax;
                    valueRect.width = totalRect.width - valueRect.x;
                    if (GUI.Button(valueRect, "  " + value.Message, EditorStyles.label))
                    {
                        PacketLogViewerEditor.Open(value.Value);
                    }

                    rect = EditorGUILayout.GetControlRect(true, size.y);
                    rect.x = 0;
                    rect.y -= 1;
                    rect.height += 3;
                    rect.width += 4;

                    index = !index;
                }

                GUI.skin.label.alignment = lastAlign;
                EditorUtils.PopColor();
            }
        }

        private void DrawRootContent()
        {
            var current = _path.Last();
            var enumerable = current.Value as IEnumerable<PathValue>;
            if (enumerable != null)
            {
                var sorted = Sort(enumerable, _sortBy);
                var index = true;

                var content = new GUIContent("");
                GUIStyle minibuttonmid = EditorUtils.Styles.MiniButtonMid;
                var size = minibuttonmid.CalcSize(content);
                var rect = EditorGUILayout.GetControlRect(true, size.y);
                var totalRect = rect;
                rect.x = 0;
                rect.y -= 1;
                rect.height += 3;
                rect.width += 4;

                EditorUtils.PushColor();

                var lastAlign = GUI.skin.label.alignment;

                foreach (var value in sorted)
                {
                    if (index) EditorUtils.PopColor();
                    else
                    {
                        EditorUtils.PushColor();
                        GUI.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                    }

                    GUI.Box(rect, content, EditorStyles.helpBox);

                    var labelRect = rect;
                    labelRect.width = 120f;
                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUI.Label(labelRect,
                            "  " + value.Time.TimeOfDay.Hours.ToString("00") + ":" +
                            value.Time.TimeOfDay.Minutes.ToString("00") + ":" +
                            value.Time.TimeOfDay.Seconds.ToString("00") +
                            ":" + value.Time.TimeOfDay.Milliseconds.ToString("000"));
                    var labelIoRect = labelRect;
                    labelIoRect.x = labelRect.xMax;
                    labelIoRect.width = 60f;
                    GUI.Label(labelIoRect, value.IO.ToString());
                    var nameRect = labelIoRect;
                    nameRect.x = labelIoRect.xMax;
                    nameRect.width = 300f;
                    // if (GUI.Button(nameRect, "  " + value.Name, EditorStyles.label))
                    // {
                    //     InspectorEditorWindow.Open(value.Value, value.Name, true, true);
                    // }

                    var valueRect = nameRect;
                    valueRect.x = nameRect.xMax;
                    valueRect.width = totalRect.width - valueRect.x;
                    if (GUI.Button(valueRect, "  " + value.Message, EditorStyles.label))
                    {
                        PacketLogViewerEditor.Open(value.Value);
                    }

                    rect = EditorGUILayout.GetControlRect(true, size.y);
                    rect.x = 0;
                    rect.y -= 1;
                    rect.height += 3;
                    rect.width += 4;

                    index = !index;
                }

                GUI.skin.label.alignment = lastAlign;
                EditorUtils.PopColor();
            }
        }

        private void AddPath(PathValue value)
        {
            if (value.Value != null && !value.Value.GetType().IsPrimitive && !(value.Value is string))
            {
                var collection = new List<PathValue>();
                var dictionary = value.Value as IDictionary;
                var pathType = PathType.Object;
                if (dictionary != null)
                {
                    var enumerator = dictionary.GetEnumerator();
                    while (enumerator.MoveNext() && enumerator.Key != null)
                    {
                        collection.Add(new PathValue
                        {
                            Name = Regex.Unescape(enumerator.Key.ToString()),
                            Message = Substring(ObjectUtils.Inspect(enumerator.Value, true)),
                            Value = enumerator.Value
                        });
                    }

                    pathType = PathType.Object;
                }
                else
                {
                    var enumerable = value.Value as IEnumerable;
                    if (enumerable != null)
                    {
                        var enumerator = enumerable.GetEnumerator();
                        var eindex = 0;
                        while (enumerator.MoveNext())
                        {
                            collection.Add(new PathValue
                            {
                                Index = eindex,
                                Name = eindex + GetPostfix(enumerator.Current, new[]
                                {
                                    "id",
                                    "type",
                                    "name"
                                }),
                                Message = Substring(ObjectUtils.Inspect(enumerator.Current, true)),
                                Value = enumerator.Current
                            });
                            eindex++;
                        }

                        pathType = PathType.Array;
                    }
                }

                Array.Resize(ref _path, _path.Length + 1);
                _path[_path.Length - 1] = new PathValue
                {
                    Name = value.Name,
                    Value = collection.ToArray(),
                    Type = pathType
                };
                _sortBy = SortBy.NAME_UP;
            }
        }

        private static IEnumerable<PathValue> Sort(IEnumerable<PathValue> array, SortBy sortBy)
        {
            List<PathValue> sorted = array as List<PathValue>;
            if (sorted == null) sorted = array.ToList();
            sorted.Sort(((p1, p2) =>
            {
                switch (sortBy)
                {
                    case SortBy.TIME_UP:
                        return p1.Time > p2.Time ? 1 : -1;
                    case SortBy.TIME_DOWN:
                        return p1.Time > p2.Time ? -1 : 1;
                    case SortBy.NAME_UP:
                        if (p1.Index != p2.Index) return p1.Index > p2.Index ? 1 : -1;
                        return string.CompareOrdinal(p1.Name, p2.Name);
                    case SortBy.NAME_DOWN:
                        if (p1.Index != p2.Index) return p1.Index > p2.Index ? -1 : 1;
                        return string.CompareOrdinal(p2.Name, p1.Name);
                    case SortBy.TYPE_IN:
                        return string.CompareOrdinal(p1.IO.ToString(), p2.IO.ToString());
                    case SortBy.TYPE_OUT:
                        return string.CompareOrdinal(p2.IO.ToString(), p1.IO.ToString());
                    case SortBy.VALUE_UP:
                        return string.CompareOrdinal(p1.Value.ToString(), p2.Value.ToString());
                    case SortBy.VALUE_DOWN:
                        return string.CompareOrdinal(p2.Value.ToString(), p1.Value.ToString());
                }

                return 0;
            }));
            return sorted;
        }

        private static string GetPostfix(object value, IEnumerable<string> keys)
        {
            var result = "";
            var dictionary = value as IDictionary;
            if (dictionary != null)
            {
                foreach (var key in keys)
                {
                    if (dictionary.Contains(key))
                    {
                        result = Regex.Unescape(string.Concat(result, "\t", key, ":", dictionary[key]));
                    }
                }
            }

            return result;
        }

        private static string Substring(string text, int count = 150)
        {
            if (text.Length <= count) return text;
            return text.Substring(0, count);
            //return Regex.Unescape(text.Substring(0, count));
        }

        private struct PathValue
        {
            public int Index;
            public string Name;
            public object Value;
            public string Message;
            public DateTime Time;
            public PathIO IO;
            public PathType Type;
        }

        private enum SortBy
        {
            TIME_UP,
            TIME_DOWN,
            NAME_UP,
            NAME_DOWN,
            VALUE_UP,
            VALUE_DOWN,
            TYPE_IN,
            TYPE_OUT
        }

        private enum PathIO
        {
            Response,
            Call,
            Notify
        }

        private enum PathType
        {
            Root,
            Array,
            Object
        }
    }

    static class EditorUtils
    {
        struct ColorInfo
        {
            public Color Color;
            public Color BackgroundColor;
            public Color ContentColor;
        }

        private static readonly Stack<ColorInfo> ColorsStack = new Stack<ColorInfo>();

        public static void PushColor()
        {
            ColorsStack.Push(new ColorInfo
            {
                Color = GUI.color,
                BackgroundColor = GUI.backgroundColor,
                ContentColor = GUI.contentColor,
            });
        }

        public static void PopColor()
        {
            if (ColorsStack.Count != 0)
            {
                var colorInfo = ColorsStack.Pop();
                GUI.color = colorInfo.Color;
                GUI.backgroundColor = colorInfo.BackgroundColor;
                GUI.contentColor = colorInfo.ContentColor;
            }
        }
        public class Styles
        {
            public const string FlowBackground = "flow background";
            public const string MiniButtonMid = "minibuttonmid";
            public const string MiniToolbarButton = "MiniToolbarButton";
            public const string ToolbarButton = "toolbarbutton";
        }
    }

    public class PacketLogViewerEditor : EditorWindow
    {
        private static object _inspectObject;
        private static Vector2 _scrollPosition;
        private static int _index = 1;
        private static string _message;

        public static void Open(object inspectObject)
        {
            _inspectObject = inspectObject;
            _message =
                    Regex.Unescape(_index == 0
                            ? ObjectUtils.Inspect(_inspectObject)
                            : ObjectUtils.Inspect(_inspectObject, true));
            EditorGUIUtility.systemCopyBuffer = _message;
            GetWindow<PacketLogViewerEditor>().Show(true);
        }


        private void OnGUI()
        {
            //try
            //{
            var currentIndex = GUILayout.Toolbar(_index, new[]
            {
                "inspect",
                "json"
            });
            if (currentIndex != _index)
            {
                _message =
                        Regex.Unescape(currentIndex == 0
                                ? ObjectUtils.Inspect(_inspectObject)
                                : ObjectUtils.Inspect(_inspectObject, true));
                EditorGUIUtility.systemCopyBuffer = _message;
                _index = currentIndex;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            EditorGUILayout.TextArea(_message);
            EditorGUILayout.EndScrollView();
            //}
            //catch (Exception exception)
            //{
            //  Debug.LogError(exception.Message);
            //}
        }
    }

    static class ThreadPoolExecutor
    {
        private static readonly Queue<Action> Queue = new Queue<Action>();
        private static readonly object QueueSync = new object();

        static ThreadPoolExecutor()
        {
            EditorApplication.update += Update;
        }

        public static void ExecuteInBackground(Action inBackground, Action onCompleteInMainThread)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                inBackground();
                lock (QueueSync)
                {
                    Queue.Enqueue(onCompleteInMainThread);
                }
            });
        }

        private static void Update()
        {
            Action[] actions = null;
            lock (QueueSync)
            {
                if (Queue.Count != 0)
                {
                    actions = Queue.ToArray();
                    Queue.Clear();
                }
            }
            if (actions != null && actions.Length != 0)
            {
                foreach (var action in actions)
                {
                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
        }
    }
}
