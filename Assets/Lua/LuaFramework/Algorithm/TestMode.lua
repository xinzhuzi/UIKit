--[[
用来开启调试标记位，目前包括Trace，asset等
]]
TestMode = {}
local  this = TestMode

this.assertEnable = true -- 是否打开assert

this.traceEnable = true    -- 是否打开Trace


--------------------以下是 单机模式的设置和判断
local singleMode = true  --是否单机模式
local directSceneTestFlag = false
local hasNetworkLog = false -- 是否打印网络层LOG
local hasLog = false -- 是否打印LOG(总开关)

--[[--
 * @Description: 配置需要走单机流程的协议发包的ID，这些协议，即使在联网模式下，也会走单机流程
				配置方法：
				singleModeProtocal[2001] = 1
 ]]
local singleModeProtocal = {}
function this.Initialize()
	--singleModeProtocal[CSCMDTYPE_CS_ENTRY_LEVEL_NOTIFY_ENUM.number] = 1
	
	--设置
	this.SetNetworkLog(false)
	this.SetLog(true)
end

function this.SetSingleMode(v)
	singleMode = v
	RunMode.Instance.SingleMode = v
end

function this.SetNetworkLog(b)
	hasNetworkLog = b
	RunMode.Instance.LogNetwork = b
end

function this.filterCmd(val)
	if val ~= nil then
		RunMode.Instance:Add(tonumber(val))
	end
end

function this.clearCmd()
	RunMode.Instance:Clear()
end

function this.onlyshow(val)
	RunMode.Instance:AddShow(val)
end

function this.SetLog(b)
	hasLog = b
end

function this.HasLog()
	return hasLog
end

--指定的协议是否需要走单机模式
function this.IsCmdSingleMode(cmd_id)
	if singleMode or cmd_id == nil then
		return singleMode
	elseif (singleModeProtocal[cmd_id] ~= nil) then
		return true
	else
		return false
	end
end

local objDebugUI = nil
function this.ShowDebugDlg()
	if IsNil(objDebugUI) then
		objDebugUI = newNormalUI("Prefabs/UI/debugui")
	end
end

function this.SetDirectSceneTestFlag( ... )
	directSceneTestFlag = true
end

function this.GetDirectSceneTestFlag( ... )
	return directSceneTestFlag
end