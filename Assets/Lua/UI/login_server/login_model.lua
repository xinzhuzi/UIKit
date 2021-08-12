local login_model = class("login_model",UIModel)
local this = login_model


-------------------------------------变量-----------------------------------
local _controller = nil -- 控制器

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