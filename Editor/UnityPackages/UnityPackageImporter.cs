using UnityEditor;

namespace Saro.Packages
{
    public static class UnityPackageImporter
    {
        //[MenuItem("MGF Tools/XLua Support (已废弃)/Export")]
        [System.Obsolete("已废弃，后续将使用huatuo", true)]
        public static void ExportMGFXLuaSupport()
        {
            if (EditorUtility.DisplayDialog("模块导出", "将MGF.XLua导出到unitypackage？", "是的", "否，我再想想"))
            {
                var flag = ExportPackageOptions.Recurse | ExportPackageOptions.Interactive;
                AssetDatabase.ExportPackage("Assets/Scripts/MGF.XLua", @"Packages\com.saro.mgf\Editor\Resources\mgf.xlua.unitypackage", flag);
            }
        }

        //[MenuItem("MGF Tools/XLua Support (已废弃)/Import")]
        [System.Obsolete("已废弃，后续将使用huatuo", true)]
        public static void ImportMGFXLuaSupport()
        {
            AssetDatabase.ImportPackage(@"Packages\com.saro.mgf\Editor\Resources\mgf.xlua.unitypackage", true);
        }
    }
}
