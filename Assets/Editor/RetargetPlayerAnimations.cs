using UnityEditor;
using UnityEngine;

public class RetargetPlayerAnimations
{
    [MenuItem("Tools/Point Animation Clips To Art")]
    public static void PointToArt()
    {
        foreach (Object selected in Selection.objects)
        {
            AnimationClip clip = selected as AnimationClip;

            if (clip == null)
                continue;

            int changed = 0;

            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
            {
                if (binding.type != typeof(SpriteRenderer) || binding.path != "")
                    continue;

                var keys = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                var newBinding = binding;
                newBinding.path = "Art";

                AnimationUtility.SetObjectReferenceCurve(clip, newBinding, keys);
                AnimationUtility.SetObjectReferenceCurve(clip, binding, null);
                changed++;
            }

            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                if (binding.type != typeof(SpriteRenderer) || binding.path != "")
                    continue;

                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                var newBinding = binding;
                newBinding.path = "Art";

                AnimationUtility.SetEditorCurve(clip, newBinding, curve);
                AnimationUtility.SetEditorCurve(clip, binding, null);
                changed++;
            }

            EditorUtility.SetDirty(clip);
            Debug.Log(clip.name + ": змінено доріжок — " + changed);
        }

        AssetDatabase.SaveAssets();
    }
}