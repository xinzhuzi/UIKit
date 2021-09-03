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
            InvokeLuaTableFunction("Awake");
        }
        
        protected virtual void OnEnable() => InvokeLuaTableFunction("OnEnable");

        protected virtual void Start() => InvokeLuaTableFunction("Start");
        
        //Update,这个地方不采取这种方式调用,请使用 UpdateBeat 中的方式调用
        
        protected virtual void OnDisable() => InvokeLuaTableFunction("OnDisable");

        protected virtual void OnDestroy()
        {
            InvokeLuaTableFunction("OnDestroy");
            _luaTableModule.Dispose();
        }
        
        /// <summary>
        /// 获取当前的 table 表,根据表的方法进行调用
        /// </summary>
        /// <returns></returns>
        public LuaTable QueryLuaTable()
        {
            return _luaTableModule;
        }
        
        /// <summary>
        /// 单纯的执行方法
        /// </summary>
        /// <param name="funcName"></param>
        public virtual void InvokeLuaTableFunction(string funcName)
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

