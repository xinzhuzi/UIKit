--
-- strict.lua
-- checks uses of undeclared global variables
-- All global variables must be 'declared' through a regular assignment
-- (even assigning nil will do) in a main chunk before being used
-- anywhere or assigned to inside a function.
--

local mt = getmetatable(_G)
if mt == nil then
  mt = {}
  setmetatable(_G, mt)
end

__STRICT = true
mt.__declared = {}

mt.__newindex = function (t, n, v)
  if __STRICT and not mt.__declared[n] then
    local w = debug.getinfo(2, "S")
	if w~= nil then
		w = w.what
		if w ~= "main" and w ~= "C" then
			error("assign to undeclared variable '"..n.."'", 2)
		end	
	end
    mt.__declared[n] = true
  end
  rawset(t, n, v)
end
  
mt.__index = function (t, n)
  if not mt.__declared[n] and debug.getinfo(2, "S").what ~= "C" then
    error("variable '"..n.."' is not declared", 2)
  end
  return rawget(t, n)
end

function _G.global(...)
   for _, v in ipairs{...} do mt.__declared[v] = true end
end

----------------------------------全局变量的添加,在执行完毕这个脚本之后,不能再添加了-------------------------
function _G.CheckGlobalVariable()
    setmetatable(_G,
            {
                -- 控制新建全局变量
                __newindex = function(_, k)
                    error("attempt to add a new value to global,key: " .. k, 2)
                end,

                -- 控制访问全局变量
                __index = function(_, k)
                    error("attempt to index a global value,key: "..k,2)
                end
            })
end

---普通类型的检查放在 middleclass.lua 里面,这个里面进行变量检查