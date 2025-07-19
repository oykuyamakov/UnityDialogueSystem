using System;
using System.Collections.Generic;
using System.Reflection;
using DialogueManagement.Actions;
//using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using DialogueManagement.Data;
//using Sirenix.OdinInspector;

namespace DialogueManagement.Editor
{
      public class NpcLineDrawer 
          //: OdinValueDrawer<NpcLine>
    {
        // protected override void DrawPropertyLayout(GUIContent label)
        // {
        //     // Grab NpcNodeData owner
        //     var parent = this.Property.Parent;
        //     NpcNodeData npcNode = null;
        //
        //     while (parent != null)
        //     {
        //         if (parent.ValueEntry?.TypeOfValue == typeof(NpcNodeData))
        //         {
        //             npcNode = parent.ValueEntry.WeakSmartValue as NpcNodeData;
        //             break;
        //         }
        //         parent = parent.Parent;
        //     }
        //
        //     var npcName = npcNode?.OwnerNpc ?? NpcName.Any;
        //     //var npcColor = ColorLibrary.Get().GetNpcColor(npcName);
        //     var npcColor = new Color();
        //     int index = this.Property.Index;
        //
        //     // Draw background
        //     Rect backgroundRect = GUILayoutUtility.GetRect(0f, 0f, GUILayout.Height(0f));
        //     backgroundRect.y -= 4;
        //     backgroundRect.height += this.Property.Children.Count * EditorGUIUtility.singleLineHeight + 20;
        //     backgroundRect.xMin += 2;
        //     backgroundRect.xMax -= 2;
        //     EditorGUI.DrawRect(backgroundRect, new Color(npcColor.r, npcColor.g, npcColor.b, 0.15f));
        //
        //     GUILayout.BeginVertical(GUIStyle.none);
        //     {
        //         GUILayout.Space(2);
        //
        //         // DialogueLine field
        //         var lineProp = this.Property.FindChild(p => p.Name == "m_DialogueLine", true);
        //         string lineText = lineProp?.ValueEntry?.WeakSmartValue as string ?? "";
        //
        //         GUILayout.BeginHorizontal();
        //         GUILayout.Label($"<b>#{index + 1} {npcName } :</b>", new GUIStyle(GUI.skin.label)
        //         {
        //             richText = true,
        //             fontSize = 12,
        //             alignment = TextAnchor.MiddleLeft,
        //             margin = new RectOffset(4, 4, 0, 0)
        //         }, GUILayout.Width(85));
        //
        //         GUI.enabled = true; // Keep line editable
        //         EditorGUI.BeginChangeCheck();
        //         string newLine = EditorGUILayout.TextField(lineText);
        //         if (EditorGUI.EndChangeCheck() && lineProp != null)
        //         {
        //             lineProp.ValueEntry.WeakSmartValue = newLine;
        //         }
        //         GUI.enabled = true;
        //         GUILayout.EndHorizontal();
        //
        //         GUILayout.Space(4);
        //
        //         // AnimationAction field
        //         var animProp = this.Property.FindChild(p => p.Name == "m_AnimationAction", true);
        //         if (animProp != null)
        //         {
        //             GUILayout.BeginHorizontal();
        //             GUILayout.Label("Animation", GUILayout.Width(70));
        //             GUI.enabled = false; // Read-only
        //             EditorGUILayout.EnumPopup((AnimationAction)animProp.ValueEntry.WeakSmartValue, GUILayout.Width(100));
        //             GUI.enabled = true;
        //             GUILayout.EndHorizontal();
        //         }
        //         
        //         // Draw rest (e.g., Requirements)
        //         foreach (var child in this.Property.Children)
        //         {
        //             if (child.Name != "m_DialogueLine" && child.Name != "m_AnimationAction" )
        //             {
        //                 //else
        //                 {
        //                     child.Draw();
        //                 }
        //             }
        //         }
        //     }
        //     GUILayout.EndVertical();
        // }
    }
    
    public class NpcLineProcessor 
        //: OdinAttributeProcessor<NpcLine>
    {
        // public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        // {
        //     attributes.Add(new InlinePropertyAttribute());
        // }
        //
        // public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        // {
        //     if (member.Name == "DialogueLine")
        //     {
        //         attributes.Add(new LabelTextAttribute("Dialogue Line"));
        //     }
        //     else if (member.Name == "AnimationAction")
        //     {
        //         attributes.Add(new LabelTextAttribute("Animation Action"));
        //     }
        // }
    }
}
