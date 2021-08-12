using LuaInterface;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIKit
{
    public class LuaModule : MonoBehaviour
    {
        private LuaState _luaState;
        private LuaTable _luaTableModule;//即当前附加到的一个 UI 模块,UI 模块的名字是
        private void Awake()
        {
            _luaState = LuaClient.GetMainState();
            string path = LuaHelper.QueryLuaFullFileName(this.name.Replace("(Clone)", ""));
            _luaTableModule = _luaState?.Require<LuaTable>(path);
            if (null == _luaTableModule) return;
            _luaTableModule["gameObject"] = gameObject;
            _luaTableModule["transform"] = transform;
            ExecuteLuaTableFunction("Awake");
        }
        
        private void OnEnable() => ExecuteLuaTableFunction("OnEnable");

        private void Start() => ExecuteLuaTableFunction("Start");
        
        //Update,这个地方不采取这种方式调用,请使用 UpdateBeat 中的方式调用
        
        private void OnDisable() => ExecuteLuaTableFunction("OnDisable");

        private void OnDestroy() => ExecuteLuaTableFunction("OnDestroy");
        
        private void ExecuteLuaTableFunction(string funcName)
        {
            if (null == _luaState) return;
            var func = _luaTableModule?.GetLuaFunction(funcName);
            if (null == func) return;
            func.BeginPCall();
            func.Push(_luaTableModule);
            func.PCall();
            func.EndPCall();
            func.Dispose();
        }
    } 
}

