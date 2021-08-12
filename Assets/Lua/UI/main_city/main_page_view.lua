local main_page_view = class("main_page_view",UIView)
local this = main_page_view

-------------------------------------变量-----------------------------------
local _controller = nil --控制器


-------------------------------------方法-----------------------------------


function this:initView()
    --获取控制器
    _controller = lua_data_center.Get("main_page")
    
    --获取具体的 UI 节点
    


    --添加事件
    

    --适配
    

    --初始逻辑
    
    --xxx
    
    
    
    this:refreshView()
end

--刷新数据
function this:refreshView()
    
    
end


function this:resetView()
    
end

function this:destroyView()
    
end



return this