--[[--
 * @Description: 全局对象
 * @Author:      zhuzizheng
 * @FileName:    Global.lua
 * @DateTime:    2020-08-01 14:23:43
 ]]

------------------------- 全局变量 ---------------------

_G.String 			        = System.String
_G.Screen			        = UnityEngine.Screen
_G.GameObject 		        = UnityEngine.GameObject
_G.Transform 		        = UnityEngine.Transform
_G.Texture                  = UnityEngine.Texture
_G.Texture2D 		        = UnityEngine.Texture2D
_G.Space			        = UnityEngine.Space
_G.Camera			        = UnityEngine.Camera
_G.QualitySettings          = UnityEngine.QualitySettings
_G.AudioClip		        = UnityEngine.AudioClip
_G.MeshRenderer	            = UnityEngine.MeshRenderer
_G.Application              = UnityEngine.Application
_G.RuntimePlatform          = UnityEngine.RuntimePlatform
_G.Resources                = UnityEngine.Resources
_G.PlayerPrefs              = UnityEngine.PlayerPrefs
_G.Color			        = UnityEngine.Color
_G.WWW				        = UnityEngine.WWW
_G.BoxCollider		        = UnityEngine.BoxCollider
_G.Animator 		        = UnityEngine.Animator
_G.SceneManager             = UnityEngine.SceneManagement.SceneManager
_G.Scene  			        = UnityEngine.SceneManagement.Scene
_G.AnimationEvent           = UnityEngine.AnimationEvent
_G.ColorUtility             = UnityEngine.ColorUtility
_G.AppKernel                = Framework.AppKernel
_G.BusinessServer           = Network.BusinessServer
_G.ListKPStr2Int		    = System.Collections.Generic.ListKPStr2Int
_G.List_string              = System.Collections.Generic.List_string
_G.List_int                 = System.Collections.Generic.List_int
_G.List_EventDelegate       = System.Collections.Generic.List_EventDelegate
_G.List_UnityEngine_Vector3 = System.Collections.Generic.List_UnityEngine_Vector3
_G.Debugger                 = LuaInterface.Debugger
_G.SDKManager               = SDK.SDKManager
_G.PlatformFixed            = SDK.PlatformFixed

------------------------- UI 框架所需要的全局对象 ---------------------

_G.UIManager			    = UIKit.UIManager       -- UIManager 管理所有 UI 的对象
_G.UIAdapter		        = UIKit.UIAdapter       -- UIAdapter 适配器,适配横屏,左右的小组件
_G.UIDoubleClickListener    = UnityEngine.UI.UIDoubleClickListener -- 双击
_G.UIDragListener           = UnityEngine.UI.UIDragListener        -- 拖拽
--_G.UIEventListener          = UnityEngine.UI.UIEventListener       -- 所有的事件
_G.UILongPressListener      = UnityEngine.UI.UILongPressListener   -- 长按事件
_G.UIPointAllListener       = UnityEngine.UI.UIPointAllListener    -- 所有的 Point 事件
_G.UIPointClickListener     = UnityEngine.UI.UIPointClickListener  -- 只有一个 Point 中的点击事件
_G.UIScrollListener         = UnityEngine.UI.UIScrollListener      -- Scroll 事件
_G.PointerEventData         = UnityEngine.EventSystems.PointerEventData -- 事件发送的数据
_G.ScrollView               = UnityEngine.UI.ScrollView
_G.UITableCell              = UnityEngine.UI.UITableCell
_G.UITableView              = UnityEngine.UI.UITableView
_G.UIHelper                 = UIKit.UIHelper
_G.TextMeshProUGUI          = TMPro.TextMeshProUGUI
_G.TMP_InputField           = TMPro.TMP_InputField
_G.CanvasGroup              = UnityEngine.CanvasGroup
_G.Image                    = UnityEngine.UI.Image
_G.Canvas                   = UnityEngine.Canvas
_G.SpriteAtlas              = UnityEngine.U2D.SpriteAtlas  -- 图集
_G.Sprite                   = UnityEngine.Sprite  -- 图集中的精灵
_G.RectTransform            = UnityEngine.RectTransform
_G.LocalisationManager      = UIKit.LocalisationManager
_G.LocalisationText         = UIKit.LocalisationText
_G.EventSystem              = UnityEngine.EventSystems.EventSystem

------------------------- 导入其他头文件需要先执行上面的方法,因为这是全局变量 ---------------------
require("LuaFramework/Helper/LogHelper")
require("LuaFramework/Helper/functions")
require("LuaFramework/Helper/Utils")
require("LuaFramework/Helper/Notifier")
require("LuaFramework/lua_data_center")
require("LuaFramework/UIKit/UIController")
require("LuaFramework/UIKit/UIModel")
require("LuaFramework/UIKit/UIModule")
require("LuaFramework/UIKit/UIView")
--require("LuaFramework/GlobalChecker")  --检查变量是否特殊,此项检查目前不开启

require("sdk/sdk")
require("logic/framework/logicLuaObjMgr")
require("logic/framework/cmdName")
require("logic/common/config_data_center")
require("logic/framework/NetworkMgr")
require("logic/common/f1_uibase")






------------------------- 全局方法 ---------------------



function _G.GetDir(path)
    return string.match(fullpath, ".*/")
end

function _G.GetFileName(path)
    return string.match(fullpath, ".*/(.*)")
end


function table.contains(table, element)
    if table == nil then
        return false
    end

    for _, value in pairs(table) do
        if value == element then
            return true
        end
    end
    return false
end

function table.getCount(self)
    local count = 0

    for k, v in pairs(self) do
        count = count + 1
    end

    return count
end

function _G.PrintLua(name, lib)
    local m
    lib = lib or _G

    for w in string.gmatch(name, "%w+") do
        lib = lib[w]
    end

    m = lib

    if (m == nil) then
        Debugger.Log("Lua Module {0} not exists", name)
        return
    end

    Debugger.Log("-----------------Dump Table {0}-----------------",name)
    if (type(m) == "table") then
        for k,v in pairs(m) do
            Debugger.Log("Key: {0}, Value: {1}", k, tostring(v))
        end
    end

    local meta = getmetatable(m)
    Debugger.Log("-----------------Dump meta {0}-----------------",name)

    while meta ~= nil and meta ~= m do
        for k,v in pairs(meta) do
            if k ~= nil then
                Debugger.Log("Key: {0}, Value: {1}", tostring(k), tostring(v))
            end

        end

        meta = getmetatable(meta)
    end

    Debugger.Log("-----------------Dump meta Over-----------------")
    Debugger.Log("-----------------Dump Table Over-----------------")
end

--通过id读取语言包文本
function _G.GetLanuageTextById(_id)
    local lang_item = config_data_center.getConfigDataByFunc('dataconfig_language',function(item)
        if item.id == _id then
            return true
        else
            return false
        end
    end)
    if lang_item ~= nil then
        return lang_item.text
    else
        return ''
    end
end

--通过id读取网络错误语言包文本
function _G.GetErrorcodeTextById(_id)
    local lang_item = config_data_center.getConfigDataByFunc('dataconfig_errorcode_lan',function(item)
        if item.errorLanguageId == _id then
            return true
        else
            return false
        end
    end)
    if lang_item ~= nil then
        return lang_item.errorText
    else
        return ''
    end
end

--根据错误码Id获得提示信息
function _G.GetErrorTextById( _id)
    local lang_item = config_data_center.getConfigDataByFunc('dataconfig_errorcode',function(item)
        if item.id == _id then
            return true
        else
            return false
        end
    end)
    if lang_item ~= nil then
        return GetErrorcodeTextById(lang_item.text)
    else
        return ''
    end
end



