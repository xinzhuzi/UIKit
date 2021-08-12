XTree = {}

require("common/Object")
--[[
	红黑树，用来实现有序的map
	支持的操作为：
	put
	get
	find
	for_each
	for_each_if
	remove
	erase
	clear
]]




XTreeNode = {}

local c_black = true
local c_red = false

function XTreeNode.create(key,value)
	local this = LuaObject.create()
	this.Class = XTreeNode
	this.name = "XTreeNode"

	this.key = nil
	this.value = value
	this.left = this
	this.right = this
	this.parent = this
	this.color = c_red

	function this:reset()
		this.key = nil
		this.value = nil
		this.left = this
		this.right = this
		this.parent = this
		this.color = c_black
	end

	return this
end


local pool_v = {}
local pool_n = 0

local function allocate(key,value)
	if(pool_n == 0)then
		return XTreeNode.create(key,value)
	else
		local node = pool_v[pool_n]
		pool_n = pool_n - 1
		node.key = key
		node.value = value
		return node
	end	
end

local function realloc(node )
	pool_n = pool_n + 1
	pool_v[pool_n] = node
	node:reset()
end

--[[
	红黑树，用来实现有序的map
]]
function XTree.create(cmp)
	local this = LuaObject.create()
	this.Class = XTree
	this.name = "XTree"
	-------------------------------------------
	this.cmp = cmp
	--[[
		放元素
	]]
	function this:put(key,value)
		-- body
	end
	--[[
		获取元素
	]]
	function this:get(key)
		-- body
	end
	--[[
		是否包含元素
	]]
	function this:contains(key)
		-- body
	end
	--[[
		删除元素
	]]
	function this:remove(key)
		-- body
	end
	--[[
		查找元素所在的节点
	]]
	function this:find(key )
		-- body
	end
	--[[
		移除节点
	]]
	function this:erase(node)
		-- body
	end

	--[[
		遍历所有元素
	]]
	function this:for_each(fn)
		-- body
	end
	--[[
		遍历所有的元素，如果回调函数返回值为true,则移除元素
	]]
	function this:for_each_if(fn)
		-- body
	end
	--[[
		清除所有的元素
	]]
	function this:clear( )
		-- body
	end
	--[[
		创建根节点
	]]
	this.root = allocate()



	-------------------------------------------
	return this
end