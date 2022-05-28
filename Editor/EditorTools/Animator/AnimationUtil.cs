using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorTools.Animation
{
    public static class AnimationHelper
    {
        public static AnimationClip LoadAnimationAtPath(string path)
        {
            return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        }

        public static AnimationClip LoadAnimationAtPath(string path, string clipName)
        {
            var objs = AssetDatabase.LoadAllAssetsAtPath(path);
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i] is AnimationClip clip)
                {
                    if (clip.name == clipName) return clip;
                }
            }
            return null;
        }

        public static List<AnimationClip> LoadAllAnimationClipsAtPath(string path)
        {
            var objs = AssetDatabase.LoadAllAssetsAtPath(path);
            var results = new List<AnimationClip>();
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i] is AnimationClip clip)
                {
                    results.Add(clip);
                }
            }

            return results;
        }
    }
}