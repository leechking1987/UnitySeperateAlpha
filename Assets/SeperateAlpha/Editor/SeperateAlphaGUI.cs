using UnityEngine;
using System;

namespace UnityEditor
{
    internal class SeperateAlphaGUI : ShaderGUI
    {
        bool m_FirstTimeApply = true;

        MaterialEditor m_MaterialEditor;

        Material m_material;

        MaterialProperty mainTex = null;
        MaterialProperty alphaTex = null;

        public void FindProperties(MaterialProperty[] props)
        {
            mainTex = FindProperty("_MainTex", props);
            alphaTex = FindProperty("_AlphaTex", props);
        }

        private static class Styles
        {
            public static GUIContent mainTexture = new GUIContent("Texture Main");
            public static GUIContent alphaTexture = new GUIContent("Alpha");
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            //base.OnGUI(materialEditor, properties);
            FindProperties(properties);
            m_MaterialEditor = materialEditor;
            m_material = materialEditor.target as Material;

            if (m_FirstTimeApply)
            {
                MaterialChanged(m_material);
                m_FirstTimeApply = false;
            }

            ShaderPropertiesGUI(m_material);
        }

        public void ShaderPropertiesGUI(Material material)
        {
            EditorGUI.BeginChangeCheck();
            {
                m_MaterialEditor.TextureProperty(mainTex, "Texture Main");
                m_MaterialEditor.TextureProperty(alphaTex, "Alpha");
            }
            if (EditorGUI.EndChangeCheck())
            {
                MaterialChanged(m_material);
            }
        }

        static void SetMaterialKeywords(Material material)
        {
            SetKeyword(material, "ENABLE_SPLITALPHA", material.GetTexture("_AlphaTex"));
        }

        static void MaterialChanged(Material material)
        {
            SetMaterialKeywords(material);
        }

        static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
            {
                m.EnableKeyword(keyword);
            }
            else
            {
                m.DisableKeyword(keyword);
            }
        }
    }
}