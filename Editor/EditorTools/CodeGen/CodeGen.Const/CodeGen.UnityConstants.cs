using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CodeGen
{
    public class UnityConstantsCodeGen : EditorWindow
    {
        private string _namespaceName = @"UnityConstants";
        private string _folderPath = @"Scripts/Generate/UnityConstants";
        private string _axesFileName = @"Axes.cs";
        private string _tagsFileName = @"Tags.cs";
        private string _sortingLayersFileName = @"SortingLayers.cs";
        private string _layersFileName = @"Layers.cs";
        private bool m_axesSelected;
        private bool m_tagSelected;
        private bool m_sortingLayerSelected;
        private bool m_layerSelected;

        [MenuItem("MGF Tools/CodeGen/Unity Constants")]
        private static void CallCreateWindow()
        {
            // Get existing open window or if none, make a new one:
            var window = (UnityConstantsCodeGen)GetWindow(typeof(UnityConstantsCodeGen));
            window.titleContent.text = "Unity Constants Generator";
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Separator();

            EditorGUILayout.BeginVertical(EditorStyles.inspectorFullWidthMargins);

            EditorGUILayout.LabelField("Generates as Assets/<Folder_Path>/<File_Name>", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(@"Namespace");
            _namespaceName = EditorGUILayout.TextField(_namespaceName, EditorStyles.textField);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(@"Folder Path");
            _folderPath = EditorGUILayout.TextField(_folderPath, EditorStyles.textField);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("File Names", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            m_axesSelected = EditorGUILayout.ToggleLeft(@"Axes", m_axesSelected);
            _axesFileName = EditorGUILayout.TextField(_axesFileName, EditorStyles.textField);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            m_tagSelected = EditorGUILayout.ToggleLeft(@"Tags", m_tagSelected);
            _tagsFileName = EditorGUILayout.TextField(_tagsFileName, EditorStyles.textField);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            m_sortingLayerSelected = EditorGUILayout.ToggleLeft(@"Sorting Layers", m_sortingLayerSelected);
            _sortingLayersFileName = EditorGUILayout.TextField(_sortingLayersFileName, EditorStyles.textField);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            m_layerSelected = EditorGUILayout.ToggleLeft(@"Layers", m_layerSelected);
            _layersFileName = EditorGUILayout.TextField(_layersFileName, EditorStyles.textField);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            if (GUILayout.Button("Generate All"))
            {
                try
                {
                    if (m_axesSelected) GenerateAndImport(false, _namespaceName, Path.Combine("Assets", _folderPath, _axesFileName), GetAllAxisNames);
                    if (m_tagSelected) GenerateAndImport(false, _namespaceName, Path.Combine("Assets", _folderPath, _tagsFileName), GetAllTags);
                    if (m_sortingLayerSelected) GenerateAndImport(false, _namespaceName, Path.Combine("Assets", _folderPath, _sortingLayersFileName), GetAllSortingLayers);
                    if (m_layerSelected) GenerateAndImport(true, _namespaceName, Path.Combine("Assets", _folderPath, _layersFileName), GetAllLayers);
                    GC.Collect();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            //GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        #region code generation

        private static void GenerateAndImport(bool isLayer, string namespaceName, string fullPath, Func<IEnumerable<string>> namesProvider)
        {
            var names = namesProvider();
            if (names.Any())
            {
                GenerateNamesCodeFile(isLayer, namespaceName, fullPath, names);
                AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);
            }
            else
                Debug.Log($"No names found, skipping generation of {fullPath}");
        }

        private static void GenerateNamesCodeFile(bool isLayer, string namespaceName, string fullPath, IEnumerable<string> names)
        {
            var name = Path.GetFileNameWithoutExtension(fullPath);
            var constants = names.ToDictionary(CodeGenHelper.ConvertToValidIdentifier, s => s);
            CodeCompileUnit code = null;
            if (!isLayer) code = CreateStringConstantsClass(namespaceName, name, constants);
            else code = CreateIntConstClass(namespaceName, name, constants);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            using (var stream = new StreamWriter(fullPath, append: false))
            {
                var tw = new IndentedTextWriter(stream);
                tw.WriteLine("//------------------------------------------------------------------------------");
                tw.WriteLine("// file: {0}", Path.GetFileName(fullPath));
                tw.WriteLine("// Author: Saro");
                tw.WriteLine("// Time: {0}", DateTime.Now.ToString());
                tw.WriteLine("//------------------------------------------------------------------------------");
                var codeProvider = new CSharpCodeProvider();
                codeProvider.GenerateCodeFromCompileUnit(code, tw, new CodeGeneratorOptions() { BracingStyle = "C" });
            }
        }

        private static CodeCompileUnit CreateStringConstantsClass(
            string namespaceName,
            string name,
            IDictionary<string, string> constants)
        {
            var compileUnit = new CodeCompileUnit();
            var @namespace = new CodeNamespace(namespaceName);

            var @class = new CodeTypeDeclaration(name);

            ImitateStaticClass(@class);

            foreach (var pair in constants)
            {
                var @const = new CodeMemberField(
                    typeof(string),
                    pair.Key);
                @const.Attributes &= ~MemberAttributes.AccessMask;
                @const.Attributes &= ~MemberAttributes.ScopeMask;
                @const.Attributes |= MemberAttributes.Public;
                @const.Attributes |= MemberAttributes.Const;

                @const.InitExpression = new CodePrimitiveExpression(pair.Value);
                @class.Members.Add(@const);
            }

            @namespace.Types.Add(@class);
            compileUnit.Namespaces.Add(@namespace);

            return compileUnit;
        }

        private static CodeCompileUnit CreateIntConstClass(
            string namespaceName,
            string name,
            IDictionary<string, string> constants)
        {
            var compileUnit = new CodeCompileUnit();
            var @namespace = new CodeNamespace(namespaceName);

            var @class = new CodeTypeDeclaration(name);

            ImitateStaticClass(@class);

            foreach (var pair in constants)
            {
                var @const = new CodeMemberField(
                    typeof(int),
                    pair.Key);
                @const.Attributes &= ~MemberAttributes.AccessMask;
                @const.Attributes &= ~MemberAttributes.ScopeMask;
                @const.Attributes |= MemberAttributes.Public;
                @const.Attributes |= MemberAttributes.Const;

                @const.InitExpression = new CodePrimitiveExpression(UnityEngine.LayerMask.NameToLayer(pair.Value));
                @class.Members.Add(@const);
            }

            @namespace.Types.Add(@class);
            compileUnit.Namespaces.Add(@namespace);

            return compileUnit;
        }

        /// <summary>
        /// Marks class as sealed and adds private constructor to it.
        /// </summary>
        /// <remarks>
        /// It's not possible to create static class using CodeDom.
        /// Creating abstract sealed class instead leads to compilation error.
        /// This method can be used instead to make pseudo-static class.
        /// </remarks>
        private static void ImitateStaticClass(CodeTypeDeclaration type)
        {
            type.TypeAttributes |= TypeAttributes.Sealed;

            // type.Members.Add(new CodeConstructor
            // {
            //     Attributes = MemberAttributes.Private | MemberAttributes.Final
            // });
        }

        #endregion

        #region names providers

        private static IEnumerable<string> GetAllAxisNames()
        {
            var result = new StringCollection();

            var serializedObject =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            var axesProperty = serializedObject.FindProperty("m_Axes");

            axesProperty.Next(true);
            axesProperty.Next(true);

            while (axesProperty.Next(false))
            {
                var axis = axesProperty.Copy();
                axis.Next(true);
                result.Add(axis.stringValue);
            }

            return result.Cast<string>().Distinct();
        }

        private static IEnumerable<string> GetAllTags()
        {
            return new ReadOnlyCollection<string>(InternalEditorUtility.tags);
        }

        private static IEnumerable<string> GetAllSortingLayers()
        {
            var internalEditorUtilityType = typeof(InternalEditorUtility);
            var sortingLayersProperty =
                internalEditorUtilityType.GetProperty("sortingLayerNames",
                    BindingFlags.Static | BindingFlags.NonPublic);
            var sortingLayers = (string[])sortingLayersProperty.GetValue(null, null);

            return new ReadOnlyCollection<string>(sortingLayers);
        }

        private static IEnumerable<string> GetAllLayers()
        {
            return new ReadOnlyCollection<string>(InternalEditorUtility.layers);
        }

        #endregion
    }
}