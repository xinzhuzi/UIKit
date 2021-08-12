local gm_model = class("gm_model",UIModel)
local this = gm_model


-------------------------------------变量-----------------------------------
local _controller = nil -- 控制器

-------------------------------------方法-----------------------------------

function this:initModel()
    _controller = lua_data_center.Get("gm")

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