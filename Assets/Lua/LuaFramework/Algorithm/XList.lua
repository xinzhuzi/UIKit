XList = {}
XListNode = {}

local XListAllocator_alloc = nil
local XListAllocator_dealloc = nil
local XListAllocator_pool = nil
local XListAllocator_size = 0

require("common/Object")

--[[
	interface:
	build
	push_back
	pop_back
	push_front
	pop_front
	erase
	remove
	removeAt
	find
	find_if
	find_step
	insert
	insert_at
	unique
	cleanUp
	for_each
	for_each_if
	join
	clear
	begin
	last
	rbegin
]]


function XListNode.create(value)
	local this = LuaObject.create()
	this.Class = XListNode
	this.name = "XListNode"

	this.next = nil
	this.prev = nil
	this.value = value

	function this:connect(node)
		this.next = node
		node.prev = this
	end

	--默认是节点环
	this:connect(this)
	return this
end


function XListAllocator_alloc (value)
	if(not XListAllocator_pool)then
		XListAllocator_pool = {}
		XListAllocator_size = 0
	end 
	local ret = nil
	if(XListAllocator_size > 0)then
		ret = XListAllocator_pool[XListAllocator_size]
		XListAllocator_pool[XListAllocator_size] = nil
		ret.value = value
		XListAllocator_size = XListAllocator_size - 1
	else
		ret = XListNode.create(value)
	end
	return ret
end

function XListAllocator_dealloc (node)
	if(not XListAllocator_pool)then
		XListAllocator_pool = {}
		XListAllocator_size = 0
	end
	XListAllocator_size = XListAllocator_size + 1
	XListAllocator_pool[XListAllocator_size] = node
	node:connect(node)
	node.value = nil
end

function XListAllocator_punch()
	XListAllocator_pool = nil
	XListAllocator_size = 0
end

local function find(start,step)
	while step > 0 do
		start = start.next
		step = step - 1
	end
	return start
end

local function swap(x,y)
	--这几步的时序不能乱
	x.prev.next = y
	y.next.prev = x
	x.next.prev = y
	y.prev.next = x
end

local function moveTo(x,y)
	--这几步的时序不能乱
	x.prev.next = x.next
	x.next.prev = x.prev
	x.next = y.next
	y.next.prev = x
	y.next = x
	x.prev = y
end

--[[
	快速排序 ，平均性能最优，不稳定，如果希望得到稳定的排序，
	请使用多重比较字段
	在基本有序的情况下，快速排序的性能非常低下，时间复杂度是O(n^2)
	最糟糕的情况，列表已经有序，时间复杂度是O(N^2)
	最好的情况，列表非常凌乱无序，时间复杂度是O(Nlg(N)),系数接近2，小于3
	平均情况，看RP而定，时间复杂度接近O(Nlg(N))
]]
local function qsort(first , last , fnCmp)
	--空表或者只有一个元素的情况
	if(first == last or first.next == last)then
		return
	end
	--只有2个元素的情况
	if(first.next.next == last)then
		if(fnCmp(first.value, first.next.value))then
			swap(first,first.next)
		end
	else
		local cmpIt = first
		local tmp = nil
		local x = cmpIt.prev
		local y = first.prev
		local x2 = cmpIt.next
		local y2 = last
		while x ~= y do	--将大于切割值的都放右边
			tmp = x		
			x = x.prev
			if(fnCmp(tmp.value , cmpIt.value))then
				moveTo(tmp,cmpIt)
			end
		end
		while x2 ~= y2 do --将小于切割值的都放在左边
			tmp = x2
			x2 = x2.next
			if(fnCmp(cmpIt.value , tmp.value))then
				moveTo(tmp,cmpIt.prev)
			end
		end
		--递归处理切割值左右的对象
		qsort(y.next, cmpIt, fnCmp)
		qsort(cmpIt.next, y2, fnCmp)
	end
end

--[[
	插入排序, 平均性能不好（对于很大的数量级而言的），
	最糟糕的情况时间复杂度是O(N^2),最好的情况的时间复杂度是O(N)
	平均情况的时间复杂度是O(N^2),系数是1
	但是如果是基本有序的情况下，插入排序能得到非常好的性能，
	一些特殊的需求，比如每一帧都需要做的深度排序，插入排序是非常理想的选择
]]
local function isort(first, last, fnCmp )
	local left,right = first.prev,nil
	local i = first
	local v = nil
	while i ~= last do
		right = i.prev
		while right	~= left do
			if(fnCmp(i.value,right.value))then
				local tmp = i.value
				i.value = right.value
				right.value = tmp
			else
				break
			end
			right = right.prev
		end
		i = i.next
	end
end
--[[
	获取first和last之间间隔多少个元素
]]
local function distance(first,last )
	local n = 0
	while first ~= last do
		first = first.next
		n = n + 1
	end
	return n
end 

local cmpFn = nil

--[[
	将L1,L2merge到head后面
]]
local function doMerge(L1,E1,L2,E2 )
	local list = L1.prev
	while L1 ~= E1 or L2 ~= E2 do
		if(L1 ~= E1 and L2 ~= E2)then
			if(cmpFn(it1.value , it2.value))then
				list.next = L1--使用单链穿线可以减少操作，双向穿线是不必要的操作
				list = L1
				L1 = L1.next
			else
				list.next = L2
				list = L2
				L2 = L2.next
			end
		else
			break
		end
	end
end
--[[
	根据步长做merge操作
]]
local function merge_by_step(step,first,last)
	local n = 0
	local left = first
	local mid = nil
	local right = nil
	local over = false
	local i = 0
	while true do
		left = first
		i = 0
		while i < step and (not over) do
			first = first.next
			over = (first == last)
			i = i + 1				
		end
		mid = first
		i = 0
		while i < step and (not over) do
			first = first.next
			over = (first == last)
			i = i + 1
		end
		right = first
		doMerge(left,mid,mid,right)
		n = n + 1
		if(over)then
			break
		end
	end
	return n
end

--[[
	归并排序，平均性能只比快速排序的最好性能慢一点点，
	时间复杂度是O(N*lg(N)),空间复杂度是O(1),
	这个是最差，最好和平均性能，
	甚至连因子都一样(大概是3，一次用来切割left,mid,right，
	一次用来merge，一次用来穿线)
	和数组的实现不同，链表的归并排序实现中它具有更加良好的性能
]]
local function merge_sort(first, last ,fnCmp )
	local head = first.prev	--first会被修改，但是head不会
	cmpFn = fnCmp
	local n = nil
	local pow = 1
	while true do
		n = merge_by_step(pow,first,last)
		if(n <= 1)then	--如果只需要一次的merge完成工作，这终止
			break
		else
			pow = pow + pow
		end
	end
	cmpFn = nil
	first = head.next
	while first ~= last do
		first.next.prev = first
		first = first.next
	end
end

--[[
	提供一些类方法
]]
XList.distance = distance
XList.find = find
XList.moveTo = moveTo
XList.swap = swap

--[[
	构造方法
	@param intrusive 是否为入侵式的
]]
function XList.create(intrusive)
	local this = LuaObject.create()
	this.Class = XList
	this.name = "XList"
	---------------------------------------------
	this.intrusive = intrusive
	--创建头节点
	this.head = XListAllocator_alloc()
	this.size = 0
	--[[
		根据lua table构建链表
	]]
	function this:build(t)
		for i = 1, #t do
			this:push_back(t[i])
		end
	end
	--[[
		根据lua table构建链表，同时会清除链表旧的数据
	]]
	function this:rebuild(t)
		this:clear()
		this:build(t)
	end
	--[[
		将某个区间的元素转换为lua table
	]]
	function this:toTable(tableIn,first,last )
		if(first == nil)then
			first = this:begin()
			last = this:last()
		end
		local ret = tableIn
		if(ret == nil)then
			ret = {}
		end
		local n = #ret
		while first ~= last do
			ret[n] = first.value
			n = n + 1
			first = first.next
		end
		return ret
	end
	--[[
		根据步长查找迭代器的位置
	]]
	function this:find_step(it , step)
		return find(it, step)
	end
	--[[
		找到value所在的迭代器位置
	]]
	function this:find(first,last, value)
		while first ~= last do
			if(first.value == value)then
				return first
			else
				first = first.next
			end
		end
		return nil
	end
	--[[
		根据条件查找迭代器的位置
		@param first 起始位置
		@param last 开区间的尾端
	]]
	function this:find_if(first ,last ,fn)
		while first ~= last do
			if(fn(first.value))then
				return first.value
			end
			first = first.next
		end
		return nil
	end
	--[[
		查找某个位置的元素,这个操作的平均时间复杂度是O(N)
		如果需要使用随机访问，比较推荐使用XArray
	]]
	function this:at(i)
		if(i > this.size)then
			_throw("XList.at index error: idx is:"..i)
		end
		return find(this:begin(),i).value
	end
	--[[
		根据值查找迭代器的位置
		@param first 起始位置
		@param last 开区间的尾端
	]]
	function this:find_val(first,last,value)
		if(this.intrusive)then
			return value.__node__
		end
		while first ~= last do
			if first.value == value then
				return first
			end
			first = first.next
		end
	end
	--[[
		链表是否为空
	]]
	function this:empty( )
		return this.size == 0
	end
	--[[
		从尾部插入
	]]
	function this:push_back(value )
		local node = XListAllocator_alloc(value)
		this.head.prev:connect(node)
		node:connect(this.head)
		this.size = this.size + 1
		if(this.intrusive)then
			value.__node__ = node
		end
	end
	--[[
		从头节点插入
	]]
	function this:push_front(value)
		local node = XListAllocator_alloc(value)
		node:connect(this.head.next)
		this.head:connect(node)
		this.size = this.size + 1
		if(this.intrusive)then
			value.__node__ = node
		end
	end
	--[[
		移除头节点
	]]
	function this:pop_front()
		if(this.size == 0)then
			_throw("XList.pop_front error,size is 0")
		end
		local tmp = this.head.next
		this.head:connect(tmp.next)
		if(this.intrusive)then
			tmp.__node__ = nil
		end
		XListAllocator_dealloc(tmp)
		this.size = this.size - 1
	end
	--[[
		移除末尾的节点
	]]
	function this:pop_back( )
		if(this.size == 0)then
			_throw("XList.pop_back error,size is 0")
		end
		local tmp = this.head.prev
		tmp.prev:connect(this.head)
		if(this.intrusive)then
			tmp.__node__ = nil
		end
		XListAllocator_dealloc(tmp)
		this.size = this.size - 1
	end
	--[[
		根据迭代器移除元素
		@param node 迭代器
		@param late_handle 是否延迟移除
	]]
	function this:erase(node,late_handle)
		if(not node)then
			_throw("XList.erase node is nil")
		end
		if(late_handle)then
			node.value = nil
			return
		end
		local tmp = node.next
		node.prev:connect(node.next)
		if(this.intrusive)then
			node.value.__node__ = nil
		end
		XListAllocator_dealloc(node)
		this.size = this.size - 1
		return tmp
	end
	--[[
		将非唯一的元素去掉
	]]
	function this:unique( )
		local t = {}
		local i = this:begin()
		local last = this:last()
		while i ~= last do
			local v = i.value
			if(t[i] == nil)then
				t[i] = 0
				i = i.next
			else
				i = this:erase(i)
			end
		end
	end
	--[[
		从链表中移除值相等的元素
		@param value 值
		@param late_handle 惰性移除
	]]
	function this:remove(value,late_handle)
		if(this.intrusive)then
			if(value.__node__)then
				this:erase(value.__node__, late_handle)
				value.__node__ = nil
			end
		else
			local it = this:begin()
			local last = this:last()
			while it ~= last do
				if it.value == value then
					it = this:erase(it, late_handle)
					break
				else
					it = it.next
				end
			end
		end
	end
	--[[
		从链表中移除值相等的元素
		@param fn 比较条件
		@param late_handle 惰性移除
	]]
	function this:remove_if(fn,late_handle)
		local it = this:begin()
		local last = this:last()
		while it ~= last do
			if fn(it.value) then
				it = this:erase(it,late_handle)
				break
			else
				it = it.next
			end
		end
	end
	--[[
		插入到where之前
		@param where 插入的位置
		@param value 插入的值
	]]
	function this:insert_at(where , value)
		if(not where.instanceOf(XListNode))then
			_throw("XList.insert_at error first arg illegal")
		end
		local node = XListAllocator_alloc(value)
		if(this.intrusive)then
			value.__node__ = node
		end
		where.prev:connect(node)
		node:connect(where)
		this.size = this.size + 1
	end
	--[[
		插入到where之后
		@param where 插入的位置
		@param value 插入的值
	]]
	function this:insert(where,value)
		if(not where.instanceOf(XListNode))then
			_throw("XList.insert_at error first arg illegal")
		end
		local node = XListAllocator_alloc(value)
		if(this.intrusive)then
			value.__node__ = node
		end
		node:connect(where.next)
		where:connect(node)
		this.size = this.size + 1
	end
	--[[
		清除空的元素
	]]
	function this:cleanUp( )
		local it = this:begin()
		local last = this:last()
		while it ~= last do
			if(it.value == nil)then
				it = this:erase(it)
			else
				it = it.next
			end
		end
	end
	--[[
		简洁遍历
		@param fn function(value) end
	]]
	function this:for_each(fn )
		local it = this:begin()
		local last = this:last()
		while it ~= last do
			if(it.value)then
				fn(it.value)
				it = it.next
			else
				it = this:eare(it)
			end
		end
	end
	--[[
		简洁遍历，如果返回值为空，则元素被移除
		@param
	]]
	function this:for_each_if(fn )
		local it = this:begin()
		local last = this:last()
		while it ~= last do
			if(this.value)then
				if(fn(it.value))then
					it = this:erase(it)
				else
					it = it.next
				end
			else
				it = this:erase(it)
			end
		end
	end
	--[[
		将另外一条链表合并到当前的链表的尾部
	]]
	function this:join(list )
		if(not list)then
			_throw("XList.join error param is nil")
		end
		if(not list.instanceOf(XList) )then
			_throw("XList.join error param is not XList")
		end
		if(list.intrusive ~= this.intrusive )then
			_throw("XList.join error , not the same type")
		end
		this.head.prev:connect(list.head.next)
		list.head.prev.next:connect(this.head)
		this.size = this.size + list.size 
	end
	--[[
		将另外一条链表合并到当前链表的某个位置
	]]
	function this:join_at(where, list)
		if(not where)then
			_throw("XList.join_at error param is nil")
		end
		if(not list)then
			_throw("XList.join_at error param is nil")
		end
		if(not where.instanceOf(XListNode))then
			_throw("XList.insert_at error first arg illegal")
		end
		if(not list.instanceOf(XList))then
			_throw("XList.join_at error param is not XList")
		end
		if(not list.intrusive == this.intrusive)then
			_throw("XList.join_at error , not the same type")
		end
		where.prev:connect(list.head.next)
		list.head.prev:connect(where)
		this.size = this.size + list.size
	end
	--[[
		利用快速排序算法对链表排序
	]]
	function this:quick_sort(fn,begin,last)
		if(not begin)then
			qsort(this:begin(),this:last(),fn)
		else
			qsort(begin,last,fn)
		end
	end
	--[[
		利用插入排序算法对链表排序
		平均性能和最糟糕的性能都是O(N^2)的时间复杂度
		但是在基本有序的情况下性能非常好，接近O(N)
		如果每一帧都需要处理，比如渲染排序，场景管理，
		用插入排序要比快速排序和归并排序要理想很多
		这个排序是稳定的
	]]
	function this:insert_sort(fn,begin,last)
		if(not begin)then
			isort(this:begin(),this:last(),fn)
		else
			isort(begin,last,fn)
		end
	end
	--[[
		利用归并排序算法对链表排序
		在最糟糕的情况性能比插入排序和快速排序都理想很多
		比较推荐使用
	]]
	function this:merge_sort(fn,begin,last)
		if(not begin)then
			merge_sort(this:begin(),this:last(),fn)
		else
			isort(begin,last,fn)
		end
	end
	--[[
		获取头部的元素
	]]
	function this:front( )
		return this.head.next.value
	end
	--[[
		获取尾部的元素的值
	]]
	function this:back( )
		return this.head.prev.value
	end
	--[[
		获取迭代器的结束位置，相当于stl的end，但是由于end是关键字，所以用last代替
	]]
	function this:last()
		return this.head
	end
	--[[
		顺序遍历的开始节点
	]]
	function this:begin()
		return this.head.next
	end
	--[[
		逆序遍历的开始节点
	]]
	function this:rbegin( )
		return this.head.prev
	end
	--[[
		清除所有元素
	]]
	function this:clear( )
		local it = this:begin()
		local last = this:last()
		while it ~= last do
			it = this:erase(it)
		end
	end

	return this
end

