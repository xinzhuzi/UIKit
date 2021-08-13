


local test_module_view = class("test_module_view",UIView)
local this = test_module_view


-------------------------------------变量-----------------------------------
--声明 local xxx = nil
local _controller = nil


--View 变量对象
local _horizontal_table_view = nil
local _image = nil
local _text = nil
local _dropdown = nil
local _input_field = nil
local _panel_canvas = nil



-------------------------------------方法-----------------------------------
--声明 local xxx = nil 此方法是私有方法
local _updateCellData0 = nil -- 在此方法里面刷新 cell 的数据



function this:initView()
    --获取控制器
    _controller = lua_data_center.Get("test_module")
    --获取具体的 UI 节点
    _horizontal_table_view = child(_controller.transform,"HorizontalTableView"):GetComponent(typeof(UITableView))
    _image = child(_controller.transform,"Image"):GetComponent(typeof(Image))
    _text = child(_controller.transform,"Text"):GetComponent(typeof(TextMeshProUGUI))
    _dropdown = child(_controller.transform,"Dropdown"):GetComponent(typeof(TMP_Dropdown))
    _input_field = child(_controller.transform,"InputField"):GetComponent(typeof(TMP_InputField))
    _panel_canvas = child(_controller.transform,"PanelCanvas")



    --添加事件 addClick(xxx,xxx)



    --适配 adapter(xxx)



    --初始逻辑 SetScale(xxx,false/true)



    this:refreshView()
end



--刷新数据
function this:refreshView()




   _horizontal_table_view.QueryCellTemplateId = function() return "CellTemplate" end  -- 设置UITableView刷新的方法
   _horizontal_table_view.UpdateCellData = _updateCellData0  -- 设置UITableView刷新的方法
   _horizontal_table_view.CellTotalCount = 0  -- 设置UITableView的子 cell 总数
   _horizontal_table_view:RefillCells() -- 充满整个UITableView
   _horizontal_table_view:RefreshCells() -- 刷新之前必须要有方法
end



--重置View
function this:resetView()

end



--界面销毁
function this:destroyView()

end



-------------------------------------public公开方法-----------------------------------
--使用 function this.xxx()  end 方式编写,外部调用使用 xxx.view.xxx() 此方式



-------------------------------------private方法-----------------------------------
--在头部先声明私有方法变量,然后在这个地方使用 function xxx()  end



function _updateCellData0(index,cell)
    local cIndex = index + 1
    local data = nil
end



return this