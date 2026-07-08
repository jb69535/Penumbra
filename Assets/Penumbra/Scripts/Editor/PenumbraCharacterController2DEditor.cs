using Penumbra.Player;
using UnityEditor;
using UnityEngine;

namespace Penumbra.EditorTools
{
    [CustomEditor(typeof(PenumbraCharacterController2D))]
    public sealed class PenumbraCharacterController2DEditor : UnityEditor.Editor
    {
        SerializedProperty cinderAttackSprites;
        SerializedProperty cinderIdleHandLocal;
        SerializedProperty cinderAttackHandLocals;
        SerializedProperty cinderMirrorHandXOnFlip;
        SerializedProperty cinderHandPoint;

        int previewAttackFrame;

        void OnEnable()
        {
            cinderAttackSprites = serializedObject.FindProperty("cinderAttackSprites");
            cinderIdleHandLocal = serializedObject.FindProperty("cinderIdleHandLocal");
            cinderAttackHandLocals = serializedObject.FindProperty("cinderAttackHandLocals");
            cinderMirrorHandXOnFlip = serializedObject.FindProperty("cinderMirrorHandXOnFlip");
            cinderHandPoint = serializedObject.FindProperty("cinderHandPoint");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(
                serializedObject,
                "m_Script",
                "cinderIdleHandLocal",
                "cinderAttackHandLocals",
                "cinderMirrorHandXOnFlip");

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Hand Point Tuning", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Hand offsets are local to Wanderer Visual.\n" +
                "1) Scrub Preview Frame\n" +
                "2) Move HandPoint in Scene view\n" +
                "3) Capture To Preview Frame\n" +
                "Repeat for each attack frame.",
                MessageType.Info);

            EditorGUILayout.PropertyField(cinderHandPoint);
            EditorGUILayout.PropertyField(cinderIdleHandLocal);
            EditorGUILayout.PropertyField(cinderMirrorHandXOnFlip);
            EditorGUILayout.PropertyField(cinderAttackHandLocals, true);

            var controller = (PenumbraCharacterController2D)target;
            int frameCount = Mathf.Max(
                cinderAttackSprites.arraySize,
                cinderAttackHandLocals.arraySize,
                1);
            previewAttackFrame = EditorGUILayout.IntSlider("Preview Frame", previewAttackFrame, 0, frameCount - 1);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Preview Frame"))
            {
                Undo.RecordObject(controller, "Preview Attack Hand Frame");
                controller.PreviewCinderAttackHandFrame(previewAttackFrame);
                EditorUtility.SetDirty(controller);
            }

            if (GUILayout.Button("Capture To Preview Frame"))
            {
                Undo.RecordObject(controller, "Capture Attack Hand Frame");
                if (controller.TryCaptureCinderHandPointToAttackFrame(previewAttackFrame))
                {
                    serializedObject.Update();
                    EditorUtility.SetDirty(controller);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Fill Attack Frames From Idle"))
            {
                Undo.RecordObject(controller, "Fill Attack Hand Frames");
                controller.FillCinderAttackHandLocalsFromIdle();
                serializedObject.Update();
                EditorUtility.SetDirty(controller);
            }

            if (GUILayout.Button("Select HandPoint"))
            {
                Transform handPoint = controller.transform.Find("Wanderer Visual/HandPoint");
                if (handPoint == null)
                {
                    handPoint = controller.transform.Find("HandPoint");
                }

                if (handPoint != null)
                {
                    Selection.activeTransform = handPoint;
                    EditorGUIUtility.PingObject(handPoint);
                }
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
