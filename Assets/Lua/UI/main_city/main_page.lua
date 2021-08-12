



local main_page = class("main_page",UIController)
local this = main_page

function this:Awake()
    this.view  = require("UI/main_city/main_page_view")
    this.model = require("UI/main_city/main_page_model")
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

return this