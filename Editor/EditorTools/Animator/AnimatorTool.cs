//using UnityEngine;
//using UnityEditor;
//using System.Collections.Generic;
//using System.IO;
//using UnityEditor.Animations;


//namespace EditorTools.Animation
//{
//    public class AnimatorBuilderTool : EditorWindow
//    {

//        [MenuItem("MGF Tools/Animation/AnimatorBuilder")]
//        private static void ShowWindow()
//        {
//            var window = GetWindow<AnimatorBuilderTool>();
//            window.titleContent = new GUIContent("AnimatorBuilder");
//            window.Show();
//        }

//        public List<AnimationClip> animClips = new List<AnimationClip>();

//        public string savePath = "Assets/Animator/";
//        public string animatorName = "Test.controller";

//        private Vector2 m_scrollPos;
//        private void OnGUI()
//        {
//            EditorGUILayout.HelpBox("1. Drag folder(s) to paths label.\n2. Click process button.", MessageType.Info);

//            savePath = EditorGUILayout.TextField(savePath);
//            animatorName = EditorGUILayout.TextField(animatorName);

//            var dragArea = GUILayoutUtility.GetRect(0f, 35f, GUILayout.ExpandWidth(true));
//            EditorGUI.LabelField(dragArea, "Animation Clips : drag animation(s) here.");

//            m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
//            for (int i = 0; i < animClips.Count; i++)
//            {
//                EditorGUILayout.BeginHorizontal();
//                EditorGUILayout.LabelField($"{i.ToString()}. ", GUILayout.Width(18));
//                EditorGUILayout.ObjectField(animClips[i], typeof(AnimationClip), false);
//                EditorGUILayout.EndHorizontal();
//            }
//            EditorGUILayout.EndScrollView();

//            if (Event.current.type == EventType.DragUpdated && dragArea.Contains(Event.current.mousePosition))
//            {
//                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
//            }

//            else if (Event.current.type == EventType.DragExited && dragArea.Contains(Event.current.mousePosition))
//            {
//                var dragPaths = DragAndDrop.objectReferences;
//                if (DragAndDrop.paths != null && dragPaths.Length > 0)
//                {
//                    for (int i = 0; i < dragPaths.Length; i++)
//                    {
//                        var animClip = dragPaths[i] as AnimationClip;
//                        if (animClip)
//                        {
//                            if (!animClips.Contains(animClip))
//                            {
//                                animClips.Add(animClip);
//                            }
//                        }
//                    }
//                }
//            }

//            if (GUILayout.Button("Clear all animations"))
//            {
//                animClips.Clear();
//            }

//            EditorGUILayout.Space();
//            if (GUILayout.Button("Process"))
//            {
//                BuildAnimator(savePath + animatorName);
//            }
//        }

//        private void BuildAnimator(string controllerPath)
//        {
//            /**************************************************************
//            /* create animator controller
//            /**************************************************************/
//            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

//            var baseLayer = controller.layers[0].stateMachine;

//            baseLayer.entryPosition = Vector3.zero;
//            baseLayer.exitPosition = new Vector3(400f, 200f);
//            baseLayer.anyStatePosition = new Vector3(0f, 200f);

//            /**************************************************************
//            /* create parameters
//            /**************************************************************/
//            controller.AddParameter("H", AnimatorControllerParameterType.Float);
//            controller.AddParameter("V", AnimatorControllerParameterType.Float);
//            controller.AddParameter("Melee", AnimatorControllerParameterType.Trigger);

//            /**************************************************************
//            /* add state
//            /**************************************************************/
//            // create a tree
//            BlendTree tree = new BlendTree();

//            // Set blendtree parameters
//            tree.name = "Move";
//            tree.blendType = BlendTreeType.FreeformDirectional2D;
//            tree.useAutomaticThresholds = false;
//            tree.minThreshold = 0f;
//            tree.maxThreshold = 1f;
//            tree.blendParameter = "H";
//            tree.blendParameterY = "V";

//            // Add clip to BlendTree
//            tree.AddChild(null, new Vector2(0f, 0f));
//            tree.AddChild(null, new Vector2(0f, 1f));
//            tree.AddChild(null, new Vector2(0f, -1f));
//            tree.AddChild(null, new Vector2(1f, 0f));
//            tree.AddChild(null, new Vector2(-1f, 0f));

//            // Add tree to controller asset
//            AssetDatabase.AddObjectToAsset(tree, savePath + animatorName);


//            // add tree state & set state motion
//            var move = baseLayer.AddState(tree.name, new Vector3(300f, -100f));
//            move.motion = tree;

//            var fire = baseLayer.AddState("fire");
//            fire.motion = null;

//            /**************************************************************
//            /* create transitions
//            /**************************************************************/
//            var transition = move.AddTransition(fire);
//            transition.hasExitTime = false;
//            transition.exitTime = 1f;
//            transition.duration = 0f;
//            transition.AddCondition(AnimatorConditionMode.If, 0f, "Melee");

//            transition = fire.AddTransition(move);
//            transition.hasExitTime = true;
//            transition.exitTime = 1f;
//            transition.duration = 0f;

//            /**************************************************************
//            /* add animator component to prefab's root
//            /**************************************************************/
//        }

//    }

//}
