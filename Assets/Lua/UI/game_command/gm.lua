



local gm = class("gm",UIController)
local this = gm

function this:Awake()
    this.view  = require("UI/game_command/gm_view")
    this.model = require("UI/game_command/gm_model")
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