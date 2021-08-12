
--所有 UI 的父类,是一个多例,不是唯一的
_G.UIController = class("UIController")
local this = UIController

this.view = nil 
this.model = nil



--[[ --固定写法,请参考 gm.lua

function this:Awake()
    this.view  = require("UI/game_command/gm_view")
    this.model = require("UI/game_command/gm_model")
end

function this:OnEnable()
    self:refresh()
end

function this:Start()
    self:initialize()
end

function this:OnDisable()
    self:reset()
end

function this:OnDestroy()
    self:destroy()
end
]]--





--由具体的 UI 模块控制器调用
function this:initialize()
    --[[
        在单独的 view 或者 model 使用 local 变量,提高效率
        local _controller
        _controller = lua_data_center.Get("gm")
        这样就将所有的 UI 模块控制器,放入了数据中心的弱表中
    ]]--
    lua_data_center.Set(self["name"],self)
    if self.model then
        self.model:initialize()
    end
    if self.view then
        self.view:initialize()
    end
end

--由具体的 UI 模块控制器调用

function this:refresh()
    if self.model then
        self.model:refresh()
    end
    if self.view then
        self.view:refresh()
    end
end

--由具体的 UI 模块控制器调用

function this:reset()
    if self.model then
        self.model:reset()
    end
    if self.view then
        self.view:reset()
    end
end

function this:destroy()
    if self.model then
        self.model:destroy()
    end
    if self.view then
        self.view:destroy()
    end
end


--获取控制器的 model
function this:GetModel()
    return self.model
end

--获取控制器的 view
function this:GetView()
    return self.view
end