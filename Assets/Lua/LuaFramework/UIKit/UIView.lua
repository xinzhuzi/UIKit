--所有 UI 的父类,是一个多例,不是唯一的
_G.UIView = class("UIView")
local this = UIView


--[[ 编写 view 的模板
local login_view = class("login_view",UIView)
local this = login_view

-------------------------------------变量-----------------------------------
local _controller

-------------------------------------方法-----------------------------------




function this:initView()
    _controller = lua_data_center.Get("login")


    this:refreshView()
end


function this:refreshView()
    local data = _controller:GetModel():GetData()


end


function this:resetView()

end

function this:destroyView()
    _controller = nil
end


return this

]]--


--可以重写以下 3 个方法,会自动掉入相应的 view 里面
function this:initialize()
    self:initView()
end

function this:refresh()
    local controller = lua_data_center.Get(string.sub(self["name"], 1, -6))
    if controller == nil then return end
    self:refreshView()
end


function this:reset()
    local controller = lua_data_center.Get(string.sub(self["name"], 1, -6))
    if controller == nil then return end
    self:resetView()
end

function this:destroy()
    self:destroyView()
end 