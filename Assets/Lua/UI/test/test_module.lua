


local test_module = class("test_module",UIController)
local this = test_module


-------------------------------------变量-----------------------------------
--声明 local xxx = nil



-------------------------------------方法-----------------------------------
--声明 local xxx = nil 此方法是私有方法



function this:Awake()
    this.view  = require("UI/test/test_module_view")
    this.model = require("UI/test/test_module_model")
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



-------------------------------------public公开方法-----------------------------------
--使用 function this.xxx()  end 方式编写,外部调用也以此方式



-------------------------------------private方法-----------------------------------
--在头部先声明私有方法变量,然后在这个地方使用 function xxx()  end



return this