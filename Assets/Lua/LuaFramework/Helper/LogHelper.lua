--[[--
 * @Description: 打印的工具方法
 * @Author:      zhuzizheng
 * @FileName:    Global.lua
 * @DateTime:    2020-08-01 14:23:43
 ]]

------------------------------------------------打印工具方法---------------------------------------------
local globalPrint = _G.print

----[[
-- 把表转换成类似这种格式的字符串{a='b';};，字符串用可见文本形式表示，可限制打印层数，nest_Max默认8层
--便于完整打印输出表，方便查看数据
--提交之前都要删除，供本地使用
--使用的时候将(),string_toLuaString(str),lsm_vardump(object, label, nest_Max)3个方法注释解除
--]]
----[[
-- 把表转换成类似这种格式的字符串{a='b';};，字符串用可见文本形式表示，可限制打印层数，nest_Max默认8层
--便于完整打印输出表，方便查看数据
--提交之前都要删除，供本地使用
--使用的时候将(),string.toLuaString(str),lsm_vardump(object, label, nest_Max)3个方法注释解除
--]]
local function lsm_vardump(object, label, nest_Max)
    local lookupTable = {}
    local result = {}
    local nest_Max = nest_Max or 8
    local function _v(v)
        if type(v) == "string" then
            v = v:toLuaString()
        end
        return tostring(v)
    end

    local function _key(k)
        if type(k) == "string" then
            return '["' .. k .. '"]'
        elseif type(k) == "table" then
            return '["' .. tostring(k.name or k.full_name or "table") .. '"]'
        elseif type(k) == "userdata" then
            return "[" .. "userdata" .. "]"
        else
            return "[" .. k .. "]"
        end
    end

    local function _vardump(object, label, indent, nest)
        label = label or "<var>"
        local s_Key = _key(label)
        if SubStringUTF8(s_Key, 3, 3) ~= "_" or s_Key == '["_fields"]' then
            local postfix = ""
            if nest > 1 then
                postfix = ","
            end
            if type(object) ~= "table" then
                result[#result + 1] = string.format("%s%s = %s%s", indent, s_Key, _v(object), postfix)
            elseif not lookupTable[object] then
                lookupTable[object] = true
                if s_Key ~= '["_fields"]' then
                    result[#result + 1] = string.format("%s%s = {", indent, s_Key)
                end
                local indent2 = indent .. "    "
                local keys = {}
                local values = {}
                for k, v in pairs(object) do
                    keys[#keys + 1] = k
                    values[k] = v
                end
                table.sort(
                        keys,
                        function(a, b)
                            if type(a) == "number" and type(b) == "number" then
                                return a < b
                            else
                                return tostring(a) < tostring(b)
                            end
                        end
                )
                if nest <= nest_Max then
                    for i, k in ipairs(keys) do
                        if s_Key ~= '["_fields"]' then
                            _vardump(values[k], k, indent2, nest + 1)
                        else
                            _vardump(values[k], k, indent2, nest)
                        end
                    end
                else
                    result[#result + 1] = "truncated......"
                end
                if s_Key ~= '["_fields"]' then
                    result[#result + 1] = string.format("%s}%s", indent, postfix)
                end
            end
        end
    end
    _vardump(object, label, "", 1)

    return table.concat(result, "\n")
end

local function LuaTableToString(t,nest_Max)
    local oldFunc = string.toLuaString
    string.toLuaString = tostring
    local tableString = lsm_vardump(t, "", nest_Max)
    string.toLuaString = oldFunc
    return tableString:sub(8)
end

_G.print = function(...)
    local f_str = ""
    local num_args = {...}
    for _, v in pairs(num_args) do
        if type(v) == "table" then
            f_str = f_str .. "\n" .. LuaTableToString(v)
        elseif type(v) == "function" then
            f_str = f_str .. "  " .. tostring(v)
        else
            f_str = f_str .."  " .. v
        end
    end
    printStack(f_str)  --此行不能动,行号必须与下面的 noPrint 对应
end

----[[
--	打印堆栈 add by zdj
--	提交之前都要删除，供本地使用
--]]
function _G.printStack(...)
    local out = {}
    local n = select('#', ...)
    for i = 1, n, 1 do
        local v = select(i, ...)
        out[#out + 1] = tostring(v)
    end
    out[#out + 1] = '\n'
    local tr = debug.traceback("", 2)
    local noPrint = "LuaFramework/Helper/LogHelper:120: in function 'print'"
    local s = string.gsub(tr, noPrint, "\b")
    out[#out + 1] = s
    globalPrint(table.concat(out, ' '))
end
