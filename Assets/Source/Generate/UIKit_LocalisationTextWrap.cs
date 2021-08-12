﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class UIKit_LocalisationTextWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(UIKit.LocalisationText), typeof(UnityEngine.MonoBehaviour));
		L.RegFunction("UpdateTheme", UpdateTheme);
		L.RegFunction("__eq", op_Equality);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("Id", get_Id, set_Id);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int UpdateTheme(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			UIKit.LocalisationText obj = (UIKit.LocalisationText)ToLua.CheckObject(L, 1, typeof(UIKit.LocalisationText));
			obj.UpdateTheme();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int op_Equality(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UnityEngine.Object arg0 = (UnityEngine.Object)ToLua.ToObject(L, 1);
			UnityEngine.Object arg1 = (UnityEngine.Object)ToLua.ToObject(L, 2);
			bool o = arg0 == arg1;
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_Id(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UIKit.LocalisationText obj = (UIKit.LocalisationText)o;
			int ret = obj.Id;
			LuaDLL.lua_pushinteger(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index Id on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_Id(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UIKit.LocalisationText obj = (UIKit.LocalisationText)o;
			int arg0 = (int)LuaDLL.luaL_checknumber(L, 2);
			obj.Id = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index Id on a nil value");
		}
	}
}

