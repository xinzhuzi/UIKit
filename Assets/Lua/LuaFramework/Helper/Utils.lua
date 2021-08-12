--require("common/XArray")
--require("common/TestMode")
require("LuaFramework/Algorithm/XArray")
--require("LuaFramework/Algorithm/TestMode")

Utils = {}

_assert = nil
_traceProtobuf = nil

local hex2Binary = {
    ["0"] = "0000",
    ["1"] = "0001",
    ["2"] = "0010",
    ["3"] = "0011",
    ["4"] = "0100",
    ["5"] = "0101",
    ["6"] = "0110",
    ["7"] = "0111",
    ["8"] = "1000",
    ["9"] = "1001",
    ["A"] = "1010",
    ["B"] = "1011",
    ["C"] = "1100",
    ["D"] = "1101",
    ["E"] = "1110",
    ["F"] = "1111"
}

local isStar=102

--[[断言]]
function _assert(condition, msg)
    if (not TestMode.assertEnable) then
        return
    end

    if (not condition) then
        fatal("assert fail description:")
        fatal(msg)
    end
end

--[[警告]]
function _warm(condition, msg)
    if (not TestMode.assertEnable) then
        return
    end
    if (not condition) then
        _trace("assert fail description:")
        _trace(msg)
    end
end

--[[抛出异常]]
function _throw(msg)
    if (not TestMode.assertEnable) then
        return
    end
    error(msg)
end

--[[
	清空数组的所有元素
	@param t 要清空的顺序表
--]]
Utils.clear =
    function(t)
    local n = #t
    while (n > 0) do
        table.remove(t, n) --从后往前删可以提高性能
        n = n - 1
    end
end

--[[清空protobuf的repeated 字段table]]
Utils.clearProtobufTable = function(t)
    local n = #t
    while (n > 0) do
        t:remove(n)
        n = n - 1
    end
end

--[[得到任意一个table的长度]]
Utils.get_length_from_any_table = function(tableValue)
    local tableLength = 0

    for k, v in pairs(tableValue) do
        tableLength = tableLength + 1
    end

    return tableLength
end

--[[
	clone对象，针对protobuf的地方使用
	@param objLeft	左值
	@param objRight 右值
--]]
Utils.tclone = function(objLeft, objRight)
    local strData = objRight:SerializeToString()
    objLeft:ParseFromString(strData)
end

--[[
	对table里面的每一项做值拷贝，只有一层有效
	@param objLeft	左值
	@param objRight 右值
--]]
Utils.copy = function(objLeft, objRight)
    for k, v in pairs(objRight) do
        objLeft[k] = v
    end
end

--[[
	根据分隔符分割字符串，返回分割后的table
--]]
Utils.split =
    function(s, delim)
    assert(type(delim) == "string" and string.len(delim) > 0, "bad delimiter")
    local start = 1
    local t = {} -- results table

    -- find each instance of a string followed by the delimiter
    while true do
        local pos = string.find(s, delim, start, true) -- plain find
        if not pos then
            break
        end

        table.insert(t, string.sub(s, start, pos - 1))
        start = pos + string.len(delim)
    end -- while

    -- insert final one (after last delimiter)
    table.insert(t, string.sub(s, start))
    return t
end

Utils.int2ip = function(intIP)
    local retIP = ""
    local leftValue = intIP
    for i = 0, 3 do
        local temp = math.pow(256, 3 - i)
        local sectionValue = math.floor(leftValue / temp)
        leftValue = leftValue % temp
        retIP = sectionValue .. retIP
        if (i ~= 3) then
            retIP = "." .. retIP
        end
    end
    return retIP
end

--[[
	打印调试信息
--]]
_trace = function(msg)
    if (not TestMode.traceEnable or msg == nil) then
        return
    end

    print(msg)
end

--分割字符串
function Utils.SplitStr(str, sep)
    local sep, fields = sep or ":", {}
    local pattern = string.format("([^%s]+)", sep)
    str:gsub(
        pattern,
        function(c)
            fields[#fields + 1] = c
        end
    )
    return fields
end

--[[字符长度]]
Utils.length = function(str)
    return #(str:gsub("[\128-\255][\128-\255]", " "))
end

--[[子字符串]]
Utils.sub = function(str, s, e)
    str = str:gsub("([\001-\127])", "\000%1")
    str = str:sub(s * 2 + 1, e * 2)
    str = str:gsub("\000", "")
    return str
end
--[[
	将文本碎片
	@param str 原文本
	@return XArray<string>
]]
function Utils.splitWord(str)
    local ret = XArray.create()
    local n = string.len(str)
    local str2 = ""
    local i = 1
    while i <= n do
        local flag = string.byte(str, i)
        if (flag < 128) then --1byte
            str2 = string.sub(str, i, i)
            ret:add(str2)
            i = i + 1
        elseif (flag >= 128 + 64 and flag < 128 + 64 + 32) then --2byte
            str2 = string.sub(str, i, i + 1)
            ret:add(str2)
            i = i + 2
        elseif (flag < 128 + 64 + 32 + 16) then --3byte
            str2 = string.sub(str, i, i + 2)
            ret:add(str2)
            i = i + 3
        elseif (flag < 128 + 64 + 32 + 16 + 8) then --4byte
            str2 = string.sub(str, i, i + 3)
            ret:add(str2)
            i = i + 4
        elseif (flag < 128 + 64 + 32 + 16 + 8 + 4) then --5byte
            str2 = string.sub(str, i, i + 4)
            ret:add(str2)
            i = i + 5
        else --6byte
            str2 = string.sub(str, i, i + 5)
            ret:add(str2)
            i = i + 6
        end
    end
    return ret
end

function Utils.makeWorld(xarray, n)
    local str1, str2 = "", ""
    local i = 1
    while i <= xarray.size do
        if (i <= n) then
            str1 = str1 .. xarray:at(i)
        else
            str2 = str2 .. xarray:at(i)
        end
        i = i + 1
    end
    return str1, str2
end

Utils.trans = function(str)
    local n = string.len(str)
    local str2 = ""
    local i = 1
    while i <= n do
        local flag = string.byte(str, i)
        if (flag < 128) then --1byte
            str2 = str2 .. string.sub(str, i, i)
            str2 = str2 .. "\n"
            i = i + 1
        elseif (flag >= 128 + 64 and flag < 128 + 64 + 32) then --2byte
            str2 = str2 .. string.sub(str, i, i + 1)
            str2 = str2 .. "\n"
            i = i + 2
        elseif (flag < 128 + 64 + 32 + 16) then --3byte
            str2 = str2 .. string.sub(str, i, i + 2)
            str2 = str2 .. "\n"
            i = i + 3
        elseif (flag < 128 + 64 + 32 + 16 + 8) then --4byte
            str2 = str2 .. string.sub(str, i, i + 3)
            str2 = str2 .. "\n"
            i = i + 4
        elseif (flag < 128 + 64 + 32 + 16 + 8 + 4) then --5byte
            str2 = str2 .. string.sub(str, i, i + 4)
            str2 = str2 .. "\n"
            i = i + 5
        else --6byte
            str2 = str2 .. string.sub(str, i, i + 5)
            str2 = str2 .. "\n"
            i = i + 6
        end
    end
    return str2
end

--[[--
 * @Description: 得到字符串（utf8编码）的字节长度
 ]]
Utils.utf8CharsLen = function(str)
    local n = string.len(str)
    local i = 1
    local count = 0
    while i <= n do
        local flag = string.byte(str, i)
        if (flag < 128) then --1byte
            count = count + 1
            i = i + 1
        elseif (flag >= 192 and flag < 224) then --2byte
            count = count + 1
            i = i + 2
        elseif (flag < 240) then --3byte
            count = count + 1
            i = i + 3
        elseif (flag < 248) then --4byte
            count = count + 1
            i = i + 4
        elseif (flag < 252) then --5byte
            count = count + 1
            i = i + 5
        else --6byte
            count = count + 1
            i = i + 6
        end
    end
    return count
end

Utils.transSubString = function(str, num)
    local arr = Utils.splitWord(str)
    str = Utils.makeWorld(arr, num) .. "..."
    return str
end

Utils.CHINESE_MAP = {
    [0] = "零",
    [1] = "一",
    [2] = "二",
    [3] = "三",
    [4] = "四",
    [5] = "五",
    [6] = "六",
    [7] = "七",
    [8] = "八",
    [9] = "九"
}
--[[数字转中文]]
Utils.toChinese = function(num, isIncludeTen)
    --assert(num<99)
    if (num > 99) then
        num = 99
    end
    if (num < 10) then
        return Utils.CHINESE_MAP[num]
    else
        local a = math.floor(num / 10)
        local b = num % 10
        if (b == 0) then
            if (isIncludeTen and a == 1) then
                return "十"
            end
            return Utils.CHINESE_MAP[a] .. "十"
        else
            return Utils.CHINESE_MAP[a] .. "十" .. Utils.CHINESE_MAP[b]
        end
    end
    return ""
end

--[[
	排序的算法是选择排序，并非高效的排序方法，但是是稳定的
	对数组排序，lua中提供了table.sort方法，但是该方法有时候会出错，而且是莫名其妙的错误

	如果需要对数组乱序，则可以这样用
	Utils.sort(t , function () return math.random(1000) > 500 end)
--]]
function Utils.sort(t, sort_reg)
    local len = #t
    for k = 1, len do
        local pos = k
        for i = k + 1, len do
            if sort_reg(t[pos], t[i]) then
                pos = i
            end
        end
        local tmp = t[pos]
        t[pos] = t[k]
        t[k] = tmp
    end
end

-----------下面的是类型检测函数，在有的地方可以作为排除的处理或者断言使用----------------------

function Utils.isBoolean(value)
    local t = type(value)
    return t == "boolean"
end

function Utils.isNumber(value)
    local t = type(value)
    return t == "number"
end

function Utils.isString(value)
    local t = type(value)
    return t == "string"
end

function Utils.isTable(value)
    local t = type(value)
    return t == "table"
end

function Utils.isFunction(value)
    local t = type(value)
    return t == "function"
end

function _check_fn(fn, where)
    if(Utils.isFunction(fn))then
        return
    end
    local msg = where.." type error,function value required"
    error(msg)
end

function _check_n(n, where)
    if (Utils.isNumber(n)) then
        return
    end
    local msg = where .. " type error,number value required"
    error(msg)
end

function _check_str(str, where)
    if (Utils.isString(str)) then
        return
    end
    local msg = where .. " type error,string value required"
    error(msg)
end

function _check_table(t, where)
    if (Utils.isTable(t)) then
        return
    end
    local msg = where .. " type error,table value required"
    error(msg)
end

function Utils.checkHasBattleInfo(...)
    local playerInfo = LuaGlobal.myPlayer
    local ret = false
    if (playerInfo) then
        ret = playerInfo:HasField("battle_info")
        ret = ret and playerInfo.battle_info:HasField("is_empty")
        ret = ret and (not playerInfo.battle_info.is_empty)
    end
    return ret
end

local __inner_weakTable_key = {__mode = "k"}
local __inner_weakTable_value = {__mode = "v"}
local __inner_weakTable_keyvalue = {__mode = "kv"}

--[[
	将数据表的Key设置为弱引用
]]
function useWeakKey(t)
    setmetatable(t, __inner_weakTable_key)
end
--[[
	将数据表的value设置为弱引用
]]
function useWeakValue(t)
    setmetatable(t, __inner_weakTable_value)
end
--[[
	将数据表的key和value设置为弱引用
]]
function useWeakKeyValue(t)
    setmetatable(t, __inner_weakTable_keyvalue)
end

--[[
	交换protocol 数组
]]
function Utils.swapProtoTable(t1, t2)
    if (not XArray) then
        require("XArray")
    end
    local xa1 = XArray.create()
    xa1:build(t1)
    local xa2 = XArray.create()
    xa1:build(t2)

    local sa1 = XArray.create()
    local sa2 = XArray.create()

    local ser =
        function(xa, sa)
        xa:forEach(
            nil,
            function(it)
                sa:add(it:SerializeToString())
            end
        )
    end

    ser(xa1, sa1)
    ser(xa2, sa2)

    Utils.clear(t1)
    Utils.clear(t2)

    local fn =
        function(a, t)
        a:forEach(
            nil,
            function(str)
                local newItem = t:add()
                newItem:ParseFromString(str)
            end
        )
    end

    fn(sa1, t2)
    fn(sa2, t1)
end
--[[
	安全地删除ccnode
]]
function Utils.safe_deleteCCNode(ccnode)
    if (ccnode and CCObject:safe_check(ccnode)) then
        local parentNode = ccnode:getParent()
        if (parentNode and CCObject:safe_check(parentNode)) then
            ccnode:removeFromParentAndCleanup(true)
        end
    end
end

--[[

]]
function Utils.getLegalValue(v1, v2)
    if (not v1 or v1 == "") then
        return v2
    end
    return v1
end
--[[
	由于某些用户名字比较奇葩会破坏我们的格式，所以决定先截断名字
	*originName 字符串
	*curLen 截取长度
	*needStrFix 是否需要省略号补正
]]
function Utils.getLegalName(originName, cutLen, needStrFix)
    local newName = nil

    local len = Utils.getUtf8Len(originName)
     ----string.len
    -- _trace("lance test 原有字符串长度："..len)
    if (len > cutLen) then
        newName = Utils.subUtf8(originName, cutLen)
        -- _trace("lance test 截取字符："..cutLen..":"..newName)

        if (needStrFix) then
            newName = newName .. "..."
        end
    else
        newName = originName
    end

    return newName
end
--[[
	获取字符串长度
]]
function Utils.getUtf8Len(str)
    local len = #str
    local left = len
    local cnt = 0
    local arr = {0, 0xc0, 0xe0, 0xf0, 0xf8, 0xfc}
    while left ~= 0 do
        local tmp = string.byte(str, -left)
        local i = #arr
        while arr[i] do
            if tmp >= arr[i] then
                left = left - i
                break
            end
            i = i - 1
        end
        cnt = cnt + 1
    end
    return cnt
end
--[[
	截取字符串
]]
function Utils.subUtf8(str, len)
    local newStr = ""
    local xName = Utils.splitWord(str)
    if (xName.size >= len) then
        for i = 1, len do
            newStr = newStr .. xName:at(i)
        end
    else
        newStr = str
    end
    return newStr
end

function table.find_if(t, cond)
    for k, v in pairs(t) do
        if cond(v) then
            return k, v
        end
    end
    return nil
end

Utils.table_find_if = table.find_if

function table.remove_if(t, cond)
    for i = #t, 1, -1 do
        if cond(t[i]) then
            return table.remove(t, i), i
        end
    end
    return nil
end

Utils.table_remove_if = table.remove_if

function table.removeAll_if(t, cond)
    local ret = {}
    for i = #t, 1, -1 do
        if cond(t[i]) then
            local o = table.remove(t, i)
            table.insert(ret, o)
        end
    end
    return ret
end

Utils.table_removeAll_if = table.removeAll_if

function Utils.getArg(...)
    local arg = {}
    for i = 1, select("#", ...) do
        local v = select(i, ...)
        table.insert(arg, v)
    end
    return arg
end

--Utils.createDictionary({{"key", "value"}})
function Utils.createDictionary(t)
    local dictData = CCDictionary:create()
    if t then
        for k, v in pairs(t) do
            local value = CCString:create("" .. v[2])
            dictData:setObject(value, "" .. v[1])
        end
    end
    return dictData
end

--[[
	将 Lua 对象及其方法包装为一个匿名函数

	许多功能需要传入一个 Lua 函数做参数，然后在特定事件发生时就会调用传入的函数。

	~~~ lua

	function Class:init()
	    Global:RegNetworkCmdWithFuncHandler(CARD_CMD_PROTOCOLID_ZONE_PVE_DUNGEON_INFO_RSP_ENUM.number, self.onRsp)
	end

	function Class:onRsp(pkgData)
	    self.pkgData = pkgData
	end

	~~~

	上述代码执行时将出错，报告"Invalid self" ，这就是因为 C++ 无法识别 Lua 对象方法。因此在调用我们传入的 self.onRsp 方法时没有提供正确的参数。

	要让上述的代码正常工作，就需要使用 Utils.handler() 进行一下包装：

	~~~ lua

	function Class:init()
	    Global:RegNetworkCmdWithFuncHandler(CARD_CMD_PROTOCOLID_ZONE_PVE_DUNGEON_INFO_RSP_ENUM.number, Utils.handler(self, self.onRsp))
	end

	~~~

	实际上，除了 C++ 回调 Lua 函数之外，在其他所有需要回调的地方都可以使用 handler()。

	@param mixed obj Lua 对象
	@param function method 对象方法

]]
function Utils.handler(obj, method)
    return function(...)
        return method(obj, ...)
    end
end

--[[
	将table序列化成string
]]
function Utils.tableToString(t)
    local mark = {}
    local assign = {}

    local function ser_table(tbl, parent)
        mark[tbl] = parent
        local tmp = {}
        for k, v in pairs(tbl) do
            local key = type(k) == "number" and "[" .. k .. "]" or k
            if type(v) == "table" then
                local dotkey = parent .. (type(k) == "number" and key or "." .. key)
                if mark[v] then
                    table.insert(assign, dotkey .. "=" .. mark[v])
                else
                    table.insert(tmp, key .. "=" .. ser_table(v, dotkey))
                end
            elseif (type(v) == "string") then
                table.insert(tmp, key .. "=" .. '"' .. v .. '"')
            else
                table.insert(tmp, key .. "=" .. v)
            end
        end
        return "{" .. table.concat(tmp, ",") .. "}"
    end

    return "do local ret=" .. ser_table(t, "ret") .. table.concat(assign, " ") .. " return ret end"
end

--[[
	将string反序列化成table
]]
function Utils.stringToTable(stringValue)
    local func = loadstring(stringValue)
    return func()
end

function Utils.getProtobufTableLength(protobufTable)
    local len = 0
    for k, v in ipairs(protobufTable) do
        len = len + 1
    end

    return len
end

function Utils.FloatTrunscate_02(v)
end

function Utils.IsFloatZero_5(v)
    return v >= -0.00001 and v <= 0.00001
end

function Utils.IsFloatZero_2(v)
    return v >= -0.01 and v <= 0.01
end

--将时间转化成小时分秒
function Utils.TimeToString(num)
    if num > 3600 then
        local a = math.floor(num / 3600)
        local b = math.floor(num / 60) % 60
        local c = num % 60
        return string.format("%02d:%02d:%02d", a, b, c)
    elseif num > 60 then
        local a = math.floor(num / 60)
        local b = num % 60
        return string.format("%02d:%02d", a, b)
    end
    if num < 0 then
        num = 0
    end
    num = math.floor(num)
    return string.format("00:%d", num)
end

--将时间转化成分秒 
function Utils.TimeToTwoString(num)
    if num < 0 then
        num = 0
    end
    if num > 3600 then
        local a = math.floor(num / 3600)
        local b = math.floor(num / 60) % 60
        local c = num % 60
        return string.format("%02d:%02d:%02d", a, b, c)
    else
        local a = math.floor(num / 60)
        local b = num % 60
        return string.format("%02d:%02d", a, b)
    end
end

function Utils.GetTalbeLength(_table)
    local length = 0
    if (_table ~= nil) then
        for i, v in pairs(_table) do
            length = length + 1
        end
    end

    return length
end

--[[--
 * @Description: 将一个十进制数转换成二进制字符串
 * @param:       number 十进制数
 * @return:      二进制字符串
 ]]
function Utils.GetBinaryFromNumber(number)
    local ret = ""
    local hexString = string.format("%x", number)
    for k = 1, string.len(hexString) do
        local charValue = string.sub(hexString, k, k)
        print("charValue: " .. charValue)
        local binStr = hex2Binary[tostring(string.upper(charValue))]
        if (binStr ~= nil) then
            ret = ret .. binStr
        else
            print("binStr is nil")
        end
    end

    local start, endPos = string.find(ret, "1")
    ret = string.sub(ret, start)
    return ret
end

--掩码检测
function Utils.CheckMask(mask, myMask)
    if (mask == nil or myMask == nil) then
        return false
    end

    if (bit.band(mask, myMask) == myMask) then
        return true
    else
        return false
    end
end

--获取语言包
function Utils.GetText(text)
    -- body
    local config = config_data_center.getConfigDataByID("dataconfig_language", "id", text)
    if config ~= nil then
        return config.text
    else
        if text == nil then
            print("找不到????????" .. tostring(text))
        end
    end
    return text
end

--获取字典表
function Utils.GetDicConfigById(id_str)
    local conf = config_data_center.getConfigDataByID("dataconfig_dictionary", "id", id_str)
    if conf ~= nil then
        return conf.text
    else
        return 0
    end
end

--根据数字返回缩写后的单位 100000 -> 10.0万
function Utils.GetAbStringByNumber(num)
    if num >= 100000 then
        if num % 10000 >= 1000 then
            return string.format("%.1f万", num / 10000)
        else
            return string.format("%d万", num / 10000)
        end
    end

    return tostring(num)
end

--多层table深拷贝
function Utils.DeepCopy(object)
    local lookup_table = {}
    local function _copy(object)
        if type(object) ~= "table" then
            return object
        elseif lookup_table[object] then
            return lookup_table[object]
        end
        local new_table = {}
        lookup_table[object] = new_table
        for index, value in pairs(object) do
            new_table[_copy(index)] = _copy(value)
        end
        return setmetatable(new_table, getmetatable(object))
    end
    return _copy(object)
end

--清空子物体
function Utils.CleanItemTable(_CleanGameobject)
    if _CleanGameobject.transform.childCount > 0 then
        local childTable = {}
        for i = 1, _CleanGameobject.transform.childCount do
            table.insert(childTable, _CleanGameobject.transform:GetChild(i - 1).gameObject)
        end
        for k, v in pairs(childTable) do
            GameObject.DestroyImmediate(v)
        end
    end
end

--通过秒数获取分和秒表示的字符串
function Utils.GetFormatTimeBySecond(s)
    local time_str = "00:00"
    local hour = 0
    local min = 0
    local sec = 0
    if s ~= nil and s > 0 then
        local str1, str2, str3
        if s > 3600 then
            hour = (s - s % 3600) / 3600
        end
        min = (s % 3600 - s % 60) / 60
        sec = s % 60
        if hour > 0 then
            if hour < 10 then
                str1 = "0" .. hour
            else
                str1 = tostring(hour)
            end
        else
            str1 = ""
        end
        if min < 10 then
            str2 = "0" .. min
        else
            str2 = tostring(min)
        end
        if sec < 10 then
            str3 = "0" .. sec
        else
            str3 = tostring(sec)
        end
        if hour > 0 then
            time_str = str1 .. ":" .. str2 .. ":" .. str3
        else
            time_str = str2 .. ":" .. str3
        end
    end
    return time_str
end

--[[--
 * @Description: 将数字转换为固定小位数的浮点数(四舍五入)，并去掉0尾数,
 主要用于解决lua读取Excel中float型数字有时候会出错的问题
 * @param:       number 浮点数 index 保留位数
 * @return:      字符串
 ]]
function Utils.NumberToString(number, index)
    local div = 1
    for i = 1, index do
        div = div * 10
    end
    local Number = number * div * 10
    if Number % 10 > 4 then
        Number = Number / 10 + 1
    else
        Number = Number / 10
    end
    Number = Number - Number % 1
    Number = Number / div
    return tostring(Number)
end

--根据时间戳返回字符串
function Utils.string2time(str)
    local t = Utils.split(str, "-")
    return os.time(
        {
            year = tonumber(t[1]),
            month = tonumber(t[2]),
            day = tonumber(t[3]),
            hour = tonumber(t[4]),
            min = tonumber(t[5]),
            sec = tonumber(t[6])
        }
    )
end

---@param second number 秒数
---@return string 返回 时:分:秒,例如 00:00:00
function Utils.timestampToDate(second)
    if second == nil or second == "" then
        return second
    end
    local timeHour    = math.fmod(math.floor(second/3600), 24)
    local timeMinute  = math.fmod(math.floor(second/60), 60)
    local timeSecond  = math.fmod(second, 60)
    if timeMinute<=9 then
        timeMinute = "0"..timeMinute
    end
    if timeSecond<=9 then
        timeSecond = "0"..timeSecond
    end
    return timeHour..":"..timeMinute..":"..timeSecond
end

--通过语言表转换成对应文本
function toText(str)
    if str == nil then
        return ""
    end
    local s = config_data_center.getConfigDataByID("dataconfig_language", "id", str)
    if s then
        return s.text
    end
    return str
end

--将 json 字符串转成 table
function toJsonDecode(stringJson)
    local cjson = require 'cjson'
	local ok, t = pcall(cjson.decode,stringJson)
    if not ok then
      return nil
    end
    return t
end

--将 table 转成 json 字符串
function toJsonEncode(_table)
    local cjson = require 'cjson'
	local ok, t = pcall(cjson.encode,_table)
    if not ok then
      return nil
    end
    return t
end

--四舍五入，取小数点后n位，默认取整
function OmitFloat(fl, num)
    if fl == nil or fl == 0 then
        return 0
    end
    local h = 1
    if num then
        h = math.pow(10, num)
    end
    fl = fl * h
    if fl % 1 >= 0.5 then
        fl = math.ceil(fl)
    else
        fl = math.floor(fl)
    end
    return fl / h
end

--等级格式化
function ToLevelFormat(level)
    if level == nil then
        return ""
    end
    if type(level) ~= "number" then
        return
    end
    return string.format("Lv.%d", level)
end

--不要用于protolbuff数据结构
function table.copy(object)
    local lookup_table = {}
    local function copy(object)
        if type(object) ~= "table" then
            return object
        elseif lookup_table[object] then
            return lookup_table[object]
        end
        local new_table = {}
        lookup_table[object] = new_table
        for key, value in pairs(object) do
            new_table[copy(key)] = copy(value)
        end
        return setmetatable(new_table, getmetatable(object))
    end
    return copy(object)
end

--拷贝proto数据
--function table.copy_proto(object)
--	local lookup_table = {}
--	local function copy(object)
--		if type(object) ~= "table" then
--			return object
--		elseif lookup_table[object] then
--			--if key~="_listener" and key~="_is_present_in_parent" and key~="_cached_byte_size_dirty" and key~="_listener_for_children" and key~="_cached_byte_size" then
--				return lookup_table[object]
--			--end
--		end
--		local new_table = {}
--		lookup_table[object] = new_table
--		for key, value in pairs(object) do
--			--if key~="_listener" and key~="_is_present_in_parent" and key~="_cached_byte_size_dirty" and key~="_listener_for_children" and key~="_cached_byte_size" then
--				new_table[copy(key)] = copy(value)
--			--end
--		end
--		return setmetatable(new_table, getmetatable(object))
--	end
--	return copy(object)
--end

--清空子节点
function Utils.ClearTransformChild(tra)
    if tra == nil then
        return
    end
    tra = tra.transform -- 容错处理,传入当前物体的组件亦可得到transform
    local count = tra.childCount
    for i = 0, count - 1 do
        GameObject.DestroyImmediate(tra:GetChild(0).gameObject, true)
    end
end

--隐藏子节点
function Utils.HideTransformChild(tra)
    if tra == nil then
        return
    end
    tra = tra.transform -- 容错处理,传入当前物体的组件亦可得到transform
    local count = tra.childCount
    for i = 0, count - 1 do
        tra:GetChild(i).gameObject:SetActive(false)
    end
end

--显示子节点
function Utils.ShowTransformChild(tra)
    if tra == nil then
        return
    end
    tra = tra.transform -- 容错处理,传入当前物体的组件亦可得到transform
    local count = tra.childCount
    for i = 0, count - 1 do
        tra:GetChild(i).gameObject:SetActive(true)
    end
end

--[[--
 * @Description:   创建一个GoodItem
    id          :   物品id
    num         :   右下角的数字
    depth       :   显示层级
    itemtype    :   icon 的展示类型，参考控件统一效果图
    isShow      :   是否显示获得特效
    data        :   数据
    callback    :   回调函数
  ]]
function Utils.CreateGoodItem(id, num, depth, itemtype,isShow,data,callback)
    local obj = Utils.CreateGoodItemGameobject()
    logicLuaObjMgr.getLuaObjByGameObj(obj):SetShow(id, num, itemtype, depth,isShow,data,callback)
    obj.name = tostring(id)
    return obj
end

--创建一个GoodItem
function Utils.CreateGoodItemGameobject()
    local obj = GameObject.Instantiate(UISys.Instance:LoadResSync("Prefabs/UI/good_item_ui/good_item", typeof(GameObject)))
    return obj
end

--[[--
 * @Description:
					gooditem    :   传入的变量，用于保存生成的item
					itemtype	:	物品类型,星陨还是物品,如果是星陨，则显示像商店一样的简要信息,没有红点，全部走读表
					parent		:	父节点，transform
					itemid		:	物品id
					num			:	右下角的数字
					depth		:	
					itemtype	:	icon 的展示类型，参考控件统一效果图
					data        :   数据
					callback    :   回调函数
					isbg        :   是否要底框
  ]]
function Utils.CreateItemObject(gooditem,starType,parent,itemid,num,itemtype,depth,data,callback,isbg,scale)
    local item=nil
    if starType == isStar then 
        print("参数错误 星陨已经删除")
    else
        if gooditem==nil then
            item=Utils.CreateGoodItem(itemid,num,depth,itemtype,nil,data,callback)
        else
            item = gooditem
            logicLuaObjMgr.getLuaObjByGameObj(item):SetShow(itemid,num,itemtype,nil,nil, data,callback)
            item.name = tostring(itemid)
        end
    end
    item:SetActive(true)
    item.transform.parent = parent
    item.transform.localScale = scale==nil and Vector3.one or Vector3.one*scale
    item.transform.localPosition = Vector3.zero
    return item
end




function Utils.CreateWarehouseItemGameobject()
    local obj = GameObject.Instantiate(UISys.Instance:LoadResSync("Prefabs/UI/GoodUI/warehouse_item", typeof(GameObject)))
    return obj
end

--创建一个战斗单位Item
function Utils.CreateFightItem(id, scale, depth)
    local obj = GameObject.Instantiate(UISys.Instance:LoadResSync("Prefabs/UI/GoodUI/fight_item", typeof(GameObject)))
    logicLuaObjMgr.getLuaObjByGameObj(obj):RefreshFightData(id, scale, depth)
    return obj
end

--创建一个技能Item（技能信息，缩放比）
function Utils.CreateSkillItem(data, scale)
    local obj = GameObject.Instantiate(UISys.Instance:LoadResSync("Prefabs/UI/GoodUI/skill_item", typeof(GameObject)))
    logicLuaObjMgr.getLuaObjByGameObj(obj):RefreshData(data, scale)
    return obj
end

--创建一个防御单元
function Utils.CreateDefenseItem(data, scale)
    local obj =
        GameObject.Instantiate(UISys.Instance:LoadResSync("Prefabs/UI/GoodUI/defense_item", typeof(GameObject)))
    logicLuaObjMgr.getLuaObjByGameObj(obj):RefreshData(data, scale)
    return obj
end

--通用接口，返回主界面
function Utils.BackToMainUI()
    UIPageManager.BacktoMainUI()   
    homeland_main_ui.ShowPage()
end