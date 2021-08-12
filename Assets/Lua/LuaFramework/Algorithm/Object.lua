LuaObject = {}


local function Pototype(obj,fnName)
	local key = "pototype_"..fnName
	local function __innerCall(t,...)
		local v = t[key]
		if(v)then
			for i = 1, #v do
				local fn = v[i]
				fn(t,unpack(arg))
			end  
		end
	end
	if(not obj[key])then
		obj[key] = {}
	end
	if(obj[fnName])then
		table.insert(obj[key], obj[fnName])
	end
	return __innerCall
end

function LuaObject.create()
	local this = {}
	this.Class = LuaObject
	this.name = "LuaObject"
	this.__index = this
	this.Pototype = Pototype

	function this.instanceOf(Class)
		return this.Class == Class
	end

	function this.Trace(msg)
		require("common/Utils")
		print(this.name .. "::" .. msg)
	end

	return this
end

function LuaObject.interfaceImpl(obj , interface )
	for k,v in pairs(interface) do
		obj[k] = v
	end
end

--[[
	用这个来快速创造singleton方法
	@param Class 类名，接受规范中的OOP框架中的class，需要有create方法，支持参数
	@param fnInit 如果需要额外的初始化处理，可以在这里自定义，否则可以不传递

	用法:
	A = {}
	function A.create(x,y)
		.....
	end
	LuaObject.singletonMake(A)

	A.getInstance(10,12)--这里的参数会传递给构造函数

	------------------

	B = {}
	function B.create()
		.....
	end
	LuaObject.singletonMake(B , function(b)
								 b.name = "TTCat" 
							end)

	B.getInstance().name--"TTCat" got
]]
function LuaObject.singletonMake(Class , fnInit)
	function Class.getInstance(...)
		if(not Class.instance)then
			if(arg)then
				Class.instance = Class.create(unpack(arg))
			else
				Class.instance = Class.create()
			end
			if(fnInit)then
				fnInit(Class.instance)
			end
		end
		return Class.instance
	end
end