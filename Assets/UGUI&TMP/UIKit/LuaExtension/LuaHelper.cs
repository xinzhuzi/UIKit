// using System;
// using System.Collections.Generic;
// using System.IO;
// using LuaInterface;
// using UnityEngine;
//
// namespace UIKit
// {
//     public static class LuaHelper
//     {
//
//         #region 获取 UI 的 Lua 路径,此路径自动化获取,必须保持 Lua 脚本与 Prefab 的名字一致,且不能重复
//         
//         //每一个 lua 脚本的完整路径
//         private static readonly Dictionary<string, string> LuaPath = new Dictionary<string, string>(500);
//
//         private static readonly string luaPrefixPath =
// #if UNITY_EDITOR
//             LuaConst.luaDir
// #else
//                    LuaConst.luaResDir
// #endif
//             + "/";
//         public static void AddUILuaFullFileName()
//         {
//             LuaPath.Clear();
//             string path = 
// #if UNITY_EDITOR
//                 Application.dataPath + "/lua/UI";
// #else    
//                 LuaConst.luaResDir + "/UI";
// #endif
//             GetFile(path);
//         }
//         
//         /// <summary>
//         /// 传入一个 Lua 脚本的名字,得到这个脚本的完整路径
//         /// 脚本路径从 (编辑器下)Application.dataPath + "/lua"  ;  运行时(LuaConst.luaResDir)
//         /// </summary>
//         /// <param name="name"></param>
//         /// <returns></returns>
//         public static string QueryLuaFullFileName(string name)
//         {
//             return LuaPath.TryGetValue(name,out string path) ? path : null;
//         }
//
//         /// <summary>  
//         /// 获取路径下所有文件以及子文件夹中文件  
//         /// </summary>  
//         /// <param name="path">全路径根目录</param>  
//         /// <param name="FileList">存放所有文件的全路径</param>  
//         /// <param name="RelativePath"></param>  
//         /// <returns></returns>  
//         public static void GetFile(string path)  
//         {
//             DirectoryInfo dir = new DirectoryInfo(path);
//             foreach (FileInfo f in dir.GetFiles())
//             {
//                 if (f.Name.EndsWith(".meta")) continue;
//                 var key = Path.GetFileNameWithoutExtension(f.Name);
//                 var value = f.FullName.Replace(Path.GetExtension(f.FullName), "").
//                     Replace(luaPrefixPath,"");
//                 LuaPath.Add(key, value);
//             }
//             //获取子文件夹内的文件列表，递归遍历  
//             foreach (DirectoryInfo d in dir.GetDirectories())  
//             {  
//                 GetFile(d.FullName);  
//             }  
//         }
//         
//
//         #endregion
//         
//     }
// }