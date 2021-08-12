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














------------------------------------------------旧存留---------------------------------------------------

this.ServerList = {}			--服务器列表
this.MyServerList = {}			--我的服务器列表

this.PlayerID = nil
this.Name = ""                     --玩家昵称
this.PlayerElo = nil               --PVE elo
this.PlayerColor = nil             --玩家颜色
this.PlayerMoney = 0               --玩家金币
this.PlayerStone = nil             --玩家金币
this.PlayerSteel = nil             --玩家金币
this.PlayerHardCurrency = 0        --玩家钻石
this.PlayerXp = 0                  --玩家经验
this.PlayerDailyRepeatedXp = 0     --玩家每日重复经验
this.PlayerLevel = 1               --玩家等级
this.UnlockedTerritoryMaxID = nil  --当前已经解锁的地块信息
this.RankSection = nil             --排行大段
this.RankSubsection = nil          --排行子段
this.AllianceId = nil              --公会ID
this.AllianceSignet = nil          --公会徽记
this.AllianceMemType = nil         --玩家的公会身份
this.PlayerPower = 0               --体力信息
this.VipID = nil 				   --会员ID
this.VipTime = nil                 --会员剩余时间（单位s）
this.PlayerHo = nil                --PVP荣誉点
this.NameCount= nil                --玩家换名字次数
this.PlayerHead=1                  --玩家头像ID
this.Player3V3Elo = nil			   --3V3 elo 分数
this.PlayerPVPElo = nil			   --1V1 elo 分数
this.PlayerDebris = nil            --玩家符文碎片
this.DayActive = nil			   --活跃度

this.MusicOption=nil               --音乐开关
this.AudioOption=nil               --音效开关
this.TipsOption=nil                --弹窗开关
this.PvpRankSection=nil            --3v3大段位
this.Credit=100                    --信誉值
this.RecordList={}                 --信誉值变化列表
lua_data_center.IsAdult = 2		   --0：已成年，1：未成年, 2: 未实名
this.RoleState = 0				   --创建角色状态,0 :未创建角色, 1:已创建角色
this.TodayOnlineTime = 0           --今天在线时长 
this.AllowdOnlineTime = 0          --总在线时长
this.LeftTime=0
this.RoleLevel =0

local unitList = {}
local cardList = {}

this.baseInfoRes = nil

local UpdateGameStatus = nil

--刷新玩家信息
function this.RefreshBaseInfo(data)
	this.PlayerID = data.BaseInfo.PlayerID										    
	this.Name = data.BaseInfo.Name												    
	this.PlayerElo = data.BaseInfo.PlayerElo									    
	this.PlayerColor = data.BaseInfo.PlayerColor								    
	this.PlayerMoney = data.BaseInfo.PlayerMoney 
	this.PlayerPower=data.BaseInfo.PlayerPower
	this.PlayerStone = data.BaseInfo.PlayerStone
	this.PlayerSteel = data.BaseInfo.PlayerSteel
	this.PlayerHardCurrency = data.BaseInfo.PlayerHardCurrency
	this.PlayerXp = data.BaseInfo.PlayerXp
	this.PlayerLevel = data.BaseInfo.PlayerLevel
	this.UnlockedTerritoryMaxID = data.BaseInfo.UnlockedTerritoryMaxID
	this.RankSection = data.BaseInfo.RankSection
	this.RankSubsection = data.BaseInfo.RankSubsection
	this.RoleLevel =data.BaseInfo.RoleLevel
	this.PlayerHead = data.BaseInfo.PlayerHead
    this.UpdateBaseInfo()
	--local_storage_sys.SetString_Machine("LOADING_LEVEL",this.PlayerLevel)
    --data_function_lock_main.InitServerFunctionList(data.BaseInfo.UnlockFuncInfo)	 

end

function this.UpdateBaseInfo()
    --金钱,钻石
    Notifier.dispatchCmd(cmdName.GOLD_CHANGE)

    Notifier.dispatchCmd(cmdName.HARD_CURRENCY)

end

--音乐设置
function this.SetVideoSetting()
   if GamePref.IsEnableSound then
        GlobalsVariables.Debug.RecordMerceVideo = true
   else
        GlobalsVariables.Debug.ShowDebugCom = true
        GlobalsVariables.Debug.RecordMerceVideo = false
   end
end

--动画
function this.SetSkipAutoSetting()
  if (GlobalsVariables.Debug.IsSkipTuto == false) then
    GlobalsVariables.Debug.IsSkipTuto = true
  else
    GlobalsVariables.Debug.IsSkipTuto = false
  end
end

--更新游戏状态
function UpdateGameStatus(statusList)
	for i,v in ipairs(statusList) do
		Notifier.dispatchCmd(cmdName.MSG_DATACENTER_GAMESTATUS_CHANGED,v)
	end
end

function this.GetPlayerExp()
	return this.PlayerXp
end

function this.GetUnitList()
	return unitList
end

function this.GetCardList()
	return cardList
end

function this.GetRoleID()
	return UtilTools.MyUint64ToStringWithHighLow(this.PlayerID.High,this.PlayerID.Low)
end

function this.GetName()
	return this.Name
end

function this.GetRankSection()
	return this.RankSection
end

function this.GetRankSubsection()
	return this.RankSubsection
end

function this.GetPlayerLevel()
	return this.PlayerLevel
end

function this.GetAllianceId()
	return this.AllianceId
end

function this.GetAllianceMemType()
	return this.AllianceMemType
end

function this.GetPlayerMoney()
	return this.PlayerMoney
end

function this.GetPlayerPower()
	return this.PlayerPower
end

function this.GetPlayerHardCurrency()
	return this.PlayerHardCurrency
end

function this.GetPlayerHead()
	return this.PlayerHead
end

function this.GetDayActive()
	print(this.DayActive)
	return this.DayActive
end