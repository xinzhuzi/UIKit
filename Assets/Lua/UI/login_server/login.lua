



local login = class("login",UIController)
local this = login

function this:Awake()
    this.view  = require("UI/login_server/login_view")
    this.model = require("UI/login_server/login_model")
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