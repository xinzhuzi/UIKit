


local test_module_model = class("test_module_model",UIModel)
local this = test_module_model


-------------------------------------变量-----------------------------------
--声明 local xxx = nil
local _controller = nil



-------------------------------------方法-----------------------------------
--声明 local xxx = nil 此方法是私有方法



function this:initModel()
    --获取控制器
    _controller = lua_data_center.Get("test_module")
    this:refreshModel()
end



--刷新网络数据
function this:refreshModel()

end



--重置数据
function this:resetModel()

end



--数据模块销毁
function this:destroyView()

    this.data = nil

    _controller = nil

end



-------------------------------------public公开方法-----------------------------------
--使用 function this.xxx()  end 方式编写,外部调用使用 xxx.view.xxx() 此方式



-------------------------------------private方法-----------------------------------
--在头部先声明私有方法变量,然后在这个地方使用 function xxx()  end



return this