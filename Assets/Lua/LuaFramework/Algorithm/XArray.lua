require("LuaFramework/Algorithm/Object")

XArray = {}


function XArray.contains(t,v)
	for k,val in pairs(t)do
		if(val == v)then
			return true
		end
	end
	return false
end

function XArray.contains_if(t,v,fn)
	for k,val in pairs(t)do
		if(fn(val , v))then
			return true , val
		end
	end
	return false
end
--[[
	XArray : Object
	创建新的实例
--]]
XArray.create = function()
	local this = LuaObject.create()
	this.Class = XArray
	this.name = "XArray"

	this.pool = {}
	this.size = 0
	this.dirty = false
	--[[
		在数组尾部添加元素
	]]
	function this:add(item)
		this.size = this.size + 1
		this.pool[this.size] = item 
	end
	--[[
		设置元素
		@param idx 下标
		@param item 元素的值
	]]
	function this:set(idx , item )
		if(idx > this.size + 1)then
			this:add(item)
		else
			this.pool[idx] = item
		end
	end
	--[[
		根据Lua表构建数组，不清除旧的数据
	]]
	function this:build( _table)
		for i = 1 , #_table do
			this:add(_table[i])
		end
	end
	--[[
		根据Lua表构建数组，但是会清楚旧的元素
	]]
	function this:build_clean(_table )
		this.pool = {}
		this.size = 0
		this:build(_table)
	end
	--[[
		根据条件构建
	]]
	function this:build_if(_table, reg)
		local n = #_table
		for i = 1 , n do
			local it = _table[i]
			if(reg(it))then
				this:add(it)
			end
		end
	end

	function this:build_if_item(_table,reg , getter)
		local n = #_table
		for i = 1 , n do
			local it = _table[i]
			if(reg(it))then
				this:add(getter(it))
			end
		end
	end
	--[[
		清除数组中为nil的元素
	]]
	function this:cleanUp()
		if(not this.dirty)then
			return
		end
		--浅表拷贝算法

		local v2 = {}
		local v1 = this.pool
		local n = 0
		for i = 1 , this.size do
			if(v1[i] ~= nil)then
				n = n + 1
				v2[n] = v1[i]
			end
		end
		this.pool = v2
		this.size = n
		this.dirty = false
		
		--移动算法
		--[[
		local v = this.pool
		local n = this.size
		local i,r,c = 1,2,0
		while i <= n do
			if(v[i] == nil)then
				while r <= n do
					if(v[r] ~= nil)then
						v[i] = v[r]
						v[r] = nil
						r = r + 1
						c = c + 1
						break
					else
						r = r + 1
					end
					if(r > n)then--打破外循环
						i = n
					end
				end
			else
				c = c + 1
			end
			i = i + 1
		end
		this.size = c
		this.dirty = false
		-------]]
	end
	--[[
		对数组逆序操作
	]]
	function this:reverse()
		local tmp = this.pool
		this.pool = {}
		local n = this.size
		for i = 1 ,n do
			this.pool[i] = tmp[n+1-i]
		end
		return tmp
	end
	--[[
		便利数组
		@param context上下文
		@param fnCall 回调方法
	]]
	function this:forEach(context , fnCall)
		this:cleanUp()
		local v = this.pool
		for i = 1 , this.size do
			if(fnCall(v[i] , context))then
				v[i] = nil
				this.dirty = true
			end
		end
	end
	--[[
		查找元素的下标，如果不存在，这返回0
	]]
	function this:indexOf( item)
		local v = this.pool
		local n = this.size
		local k = 0
		for i = 1 , n do
			if(v[i] == item)then
				k = i
				break
			end
		end
		return k
	end
	--[[
		移除某个位置的元素，并且将它置为nil
		lazy回收该空间
	]]
	function this:removeAt( idx)
		if(idx == nil or idx < 1 or idx > this.size)then
			return
		end
		local v = this.pool
		v[idx] = nil
		this.dirty = true
	end
	--[[
		移除某个元素，并且将它所在位置设置为nil
	]]
	function this:remove( item )
		this:removeAt(this:indexOf(item))
	end
	--[[
		将另外一个数据添加到这个数组的尾部
	]]
	function this:join(xarray )
		if(xarray.Class ~= XArray)then
			_throw("XArray.join error type error XArray required")
		end
		local n = xarray.size
		local v = xarray.pool
		for i = 1 , n do
			this:add(v[i])
		end
	end
	--[[
		创建一个新的数组，将当前数组和目标数组合并，添加到新数组中返回
		当前数组不受到影响
		@return XArray
	]]
	function this:merge(xarray )
		if(xarray.Class ~= XArray)then
			_throw("XArray.join error type error XArray required")
		end
		local ret = XArray.create()
		ret:join(this)
		ret:join(xarray)
		return ret
	end
	--[[
		根据条件移除元素
	]]
	function this:removeIf(fnCondition ,cleanUp)
		local v = this.pool
		for i = 1, this.size do
			local it = v[i]
			if(it ~= nil and fnCondition(it))then
				v[i] = nil
				this.dirty = true
			end
		end
		if(cleanUp)then
			this:cleanUp()
		end
	end
	--[[
		获取某个位置的元素
	]]
	function this:at( idx)
		local v = this.pool
		return v[idx]
	end
	--[[
		获取元素的尾节点	
	]]
	function this:back( )
		if(this.size == 0)then
			return nil
		end
		return this.pool[this.size]
	end
	--[[
		从尾部添加元素
	]]
	function this:push( item)
		local v = this.pool
		--this:cleanUp()
		this.size = this.size + 1
		v[this.size] = item
	end
	
	function this:pop()
		this.pool[this.size] = nil
		this.size = this.size - 1
	end

	function this:pop_value()
		local val = this.pool[this.size]
		this.pool[this.size] = nil
		this.size = this.size - 1
		return val
	end

	function this:empty( )
		return this.size == 0
	end
			
	function this:findIf(fnCondition)
		--this:cleanUp()
		local v = this.pool
		for i = 1 , this.size do
			if(fnCondition(v[i]))then
				return v[i] , i
			end
		end
		return nil , 0
	end
		
	function this:find( item)
		local v = this.pool
		for i = 1, this.size do
			if(v[i] == item)then
				return true , i  
			end
		end
		return false , 0
	end
		
	function this:clear()
		this.pool = {}
		this.size = 0
	end
	
	function this:delete( item)	
		local v = this.pool
		for i = 1 , this.size do
			if(v[i] == item)then
				table.remove(v, i)
				this.size = this.size - 1
				return
			end
		end
	end

	
	function this:insert( idx , item)
		local v = this.pool
		table.insert(v , idx , item)
		this.size = this.size + 1
	end
	--[[
		这里只使用了简单排序，如果追求更高的排序性能，可以选择归并排序
	]]
	function this:sort(sort_reg )
		local n = this.size
		local t = this.pool
	    local i = 1
	    local j = 1
	    while (i <= n) do
	    	j = i + 1
	    	while (j <= n) do
	    		if(sort_reg(t[i], t[j]))then
	    			local tmp = t[j]
	    			t[j] = t[i]
	    			t[i] = tmp
	    		end
	    		j = j + 1
	    	end
	    	i = i + 1
	    end
	end
	--[[
		利用table的排序来排序
		数量级大的时候必须使用这个
		这个的要求比较苛刻，必须是稳定的排序条件，即a,b如果匹配条件相等，交换以后必须依然是a,b的顺序
	]]
	function this:table_sort(sort_reg)
		local t = {}
		local v = this.pool
		for i = 1, this.size do
			table.insert(t,v[i])
		end
		table.sort(t,sort_reg)
		this:build_clean(t)
	end
	--[[
		这里只使用了简单排序，如果追求更高的排序性能，可以选择归并排序
		这个可以局部排序
	]]
	function this:sort_n(sort_reg, firstIdx, lastIdx )
		if(firstIdx < 1)then
			_throw("XArray.sort_n error: firstIdx out of range")
		end
		if(lastIdx > this.size)then
			_throw("XArray.sort_n error: lastIdx out of range")
		end
		local n = lastIdx
		local t = this.pool
	    local i = firstIdx
	    local j = 1
	    while (i <= n) do
	    	j = i + 1
	    	while (j <= n) do
	    		if(not sort_reg(t[i], t[j]))then
	    			local tmp = t[j]
	    			t[j] = t[i]
	    			t[i] = tmp
	    		end
	    		j = j + 1
	    	end
	    	i = i + 1
	    end
	end
	--[[
		常规的数组使用线性探索的方法实现，时间复杂度是O(N^2)
		这里使用哈希表高效地移除所有当前元素在xarray中存在的元素
		时间复杂度是O(N)，空间复杂度是O(N)
	]]
	function this:removeFrom(xarray )
		local m = {}
		local v = xarray.pool
		for i = 1, xarray.size do
			m[v[i]] = true
		end
		v = this.pool
		for i = 1, this.size do
			if(m[v[i]])then
				v[i] = nil
				this.dirty = true
			end
		end
		this:cleanUp()
	end
	
	return this
end

XQueue = {}
--[[
	XQueue : XArray
	创建一个queue
	@return 一个具有Array的所有行为，但是增加了获取头节点和删除头节点的功能
--]]
XQueue.create = function()
	local this = XArray.create()
	function this:front()
		if(this.size == 0 )then
			return nil
		else
			return this.pool[1]
		end
	end
	
	function this:popFront()
		local v = this.pool
		local tmp = v[1]
		table.remove(v,1)
		this.size = this.size - 1
		return tmp
	end
				
					
	return this
end

XCQueue = {}
--[[
	循环队列
	XCQueue : XQueue
--]]
XCQueue.create = function()
	local this = XQueue.create()
	this.topIdx = 1
	--[[
		获取循环队列的下一个节点
		@return 1 元素
		@return 2 元素所在的下标
	--]]
	function this:next()
		local tmp = this.pool[this.topIdx]
		this.topIdx = this.topIdx + 1
		if(this.topIdx > this.size)then
			this.topIdx = 1
		end
		return tmp , this.topIdx
	end	
	return this
end












