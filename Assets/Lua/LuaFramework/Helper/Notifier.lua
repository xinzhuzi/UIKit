require("LuaFramework/Algorithm/XArray")Notifier = {}--[[	类似于Global里面的事件，不过这个可以支持闭包--]]--发送消息Notifier.dispatchCmd = nil--注册Notifier.regist = nil--取消注册Notifier.remove = nil--哈希表local fnMap = {}--------------华丽的分割线-------------------------[[	创建回调的结构体--]]local CreateDeletegate = function()	local ret = {}	ret.fnCallback = nil	ret.context = nil	ret.key = nil	return retend--[[-- * @Description: 专门提供给C#层调用的消息函数，目前最多支持4个参数   * @param:       cmdName 命令字                 para1    参数1                 para2    参数2                 para3    参数3                 para4    参数4 ]]function Notifier.dispatchCmdForCSharp(cmdName, para1,para2,para3,para4)	local paraTable = {}	paraTable.para1 = para1	paraTable.para2 = para2	paraTable.para3 = para3	paraTable.para4 = para4	Notifier.dispatchCmd(cmdName, paraTable)end--[[	发送指令	@param cmdName	@param param ：参数由具体事件决定--]]function Notifier.dispatchCmd(cmdName , param)	local array = fnMap[cmdName]	if(array == nil)then		return	else		if(array.dirty)then			array:cleanUp()		end		local v = array.pool		local i ,it		for i = 1 , array.size do			it = array:at(i)			if(it and it.fnCallback ~= nil)then				local needRemove = it.fnCallback(param , it.context)				if(needRemove)then					array:removeAt(i)				end			end		end	endend--[[	注册事件	@param cmdName	@param fnCallback function(param , context) --todo end	如果返回值为true，那么会自动被移除，适用于只需要监听一次的事件	@param context--]]function Notifier.regist(cmdName,fnCallback,context)	--print("Notifier.regist: "..cmdName)    _assert(fnCallback,"Notifier.regist fnCallback is nil")    _check_str(cmdName,"Notifier.regist")	local array = fnMap[cmdName]	local it	local needAdd = false	if(array == nil)then		array = XArray.create()		fnMap[cmdName] = array		needAdd = true	else		if(array:findIf(			function(x)				if(x == nil)then					return  false 				end				return x.fnCallback == fnCallback 			end) == nil)then			needAdd = true		end	end	if(needAdd)then		it = CreateDeletegate()		it.fnCallback = fnCallback		it.context = context		it.key = cmdName		array:add(it)	endend--[[	移除事件--]]function Notifier.remove(cmdName,fnCallback)	--print("Notifier.remove: "..cmdName)	_check_str(cmdName,"Notifier.remove")	local array = fnMap[cmdName]	if(array ~= nil)then		local it ,idx = array:findIf(		function(x)			if(x ~= nil)then				return x.fnCallback == fnCallback			else				return false			end		end)		array:removeAt(idx)	endend--[[	移除事件监听--]]function Notifier.removeByName(cmdName)	_check_str(cmdName,"Notifier.removeByName")	fnMap[cmdName] = nilend