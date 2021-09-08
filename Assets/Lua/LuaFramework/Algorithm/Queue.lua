_G.Queue = class("Queue")

--初始化
function Queue:initialize(name)
    self.name = name
    self.last = 0 ---数据末尾
    self.first = 1 ---数据开头
    self.length = 0 ---数据长度
    self.data = {nil,nil,nil,nil,nil,nil,nil,nil,nil,nil,} ---数据容器,使用数组实现,不使用链表实现
end

--有多少个数据
function Queue:Count()
    return self.length
end

--清空
function Queue:Clear()
    self.data = nil
    self.length = nil
    self.name = nil
    self.first = nil
    collectgarbage("collect")
end

--压入数据
function Queue:Enqueue(value)
    self.length = self.length + 1
    self.last = self.last + 1
    self.data[self.last] = value
end

--弹出数据
function Queue:Dequeue()
    if self.length <= 0 then
        error("Queue is empty")
    end
    local firstValue = self.data[self.first]
    self.length = self.length - 1
    self.first = self.first + 1
    if self.length <= 0 then
        self.last = -1 ---数据末尾
        self.first = 0 ---数据开头
        self.length = 0 ---数据长度
        ---数据容器,使用数组实现,不使用链表实现
        self.data = {nil,nil,nil,nil,nil,nil,nil,nil,nil,nil,}
        collectgarbage("collect")
    end
    return firstValue
end

--查看队列首端的值
function Queue:Peek()
    if self.length <= 0 then
        error("Stack is empty")
    end
    return self.data[self.first]
end

--包含某个值
function Queue:Contains(value)
    for i = self.first, self.first + self.length+1 do
        if value == self.data[i] then
            return true
        end
    end
    return false
end




--[[
队列用法:
local myQueue = Queue:new("Queue的名字",10) //创建了一个 queue 对象
myQueue:Enqueue(value)
myQueue:Dequeue()
if _queueMessage:Count() <= 0 then
end
]]--

