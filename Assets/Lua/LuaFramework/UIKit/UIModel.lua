--[[--
 * @Description: UI 的数据类,每一个模块都有一个 UI 的数据类
 * @Author:      zhuzizheng
 * @FileName:    UIModel.lua
 * @DateTime:    2020-08-01 14:23:43
 ]]


--所有 UI 的父类,是一个多例,不是唯一的
_G.UIModel = class("UIModel")
local this = UIModel

this.data = nil -- 当前 UIModel 中的数据



--[[编写 Model 的模板
local login_model = class("login_model",UIModel)
local this = login_model
-------------------------------------变量-----------------------------------
local _controller


-------------------------------------方法-----------------------------------

function this:initModel()
    _controller = lua_data_center.Get("login")
    
end


function this:refreshModel()

end


function this:resetModel()

end

function this:destroyModel()
    this.data = nil
    _controller = nil
end


return this

]]--

--可以重写以下 3 个方法,会自动掉入相应的 Model 里面

function this:initialize()
    self:initModel()
end

function this:refresh()
    if self.controller == nil then return end
    self:refreshModel()
end


function this:reset()
    if self.controller == nil then return end
    self:resetModel()
end

function this:destroy()
    self:destroyModel()
    self.controller = nil
end

function this:GetData()
    return self.data
end 