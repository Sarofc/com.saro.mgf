using UnityEditor;

namespace Saro.Packages
{
    public static class UnityPackageImporter
    {
        [MenuItem("MGF Tools/XLua Support/Export")]
        public static void ExportMGFXLuaSupport()
        {
            if (EditorUtility.DisplayDialog("模块导出", "将MGF.XLua导出到unitypackage？", "是的", "否，我再想想"))
            {
                var flag = ExportPackageOptions.Recurse | ExportPackageOptions.Interactive;
                AssetDatabase.ExportPackage("Assets/Scripts/MGF.XLua", @"Packages\com.saro.mgf\Resources\mgf.xlua.unitypackage", flag);
            }
        }

        [MenuItem("MGF Tools/XLua Support/Import")]
        public static void ImportMGFXLuaSupport()
        {
            AssetDatabase.ImportPackage(@"Packages\com.saro.mgf\Resources\mgf.xlua.unitypackage", true);
        }
    }
}
