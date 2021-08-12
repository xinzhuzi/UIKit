---@class lua_data_center
_G.lua_data_center = { ["name"] = "lua_data_center"}
local this = lua_data_center

this.strongTable = {}
this.weakTable = {}
setmetatable(this.weakTable, {__mode = "kv"}) --设置弱表


function this.initialize()
	
end


function this.refresh()
	
end


function this.reset()
	lua_data_center.weakTable = nil
	collectgarbage()
end

function this:destroy()
	lua_data_center.strongTable = nil
	lua_data_center.weakTable = nil
	lua_data_center = nil
	collectgarbage()
end


--设置临时数据,一个key 和 value
function this.Set(key,value)
	if value == nil or key == nil then return end
	this.weakTable[key] = value
end

--得到一个临时数据
function this.Get(key)
	return this.weakTable[key]
end

--设置一些不容易被改变的数据
function this.SetStrong(key,value)
	if value == nil or key == nil then return end
	this.strongTable[key] = value
end

--获取永驻数据
function this.GetStrong(key)
	return this.strongTable[key]
end


--[[
弱表使用范例,常用于临时处理,调用方法,传输数据,例如将一个 UI 模块的 lua 对象存入弱表中,将 UI 模块的函数/方法存入弱表中等等.
local key = "aa"
lua_data_center.Set(key,{})

local t = lua_data_center.Get("aa")
key = nil --这个 key 置为 nil,就表示弱表中删除了这个对象
t = nil   --这个 t 置为 nil,就表示弱表中删除了这个对象


强表使用范例
lua_data_center.SetStrong(key,{})
local t = lua_data_center.GetStrong("key")
]]--
