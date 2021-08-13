local main_page_model = class("main_page_model",UIModel)
local this = main_page_model


-------------------------------------变量-----------------------------------
local _controller = nil -- 控制器

-------------------------------------方法-----------------------------------

function this:initModel()
    _controller = lua_data_center.Get("main_page")

    this:refreshModel()
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