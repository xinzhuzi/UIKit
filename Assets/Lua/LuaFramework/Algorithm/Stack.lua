_G.Stack = class("Stack")

--初始化
function Stack:initialize(name)
    self.name = name
    self.length = 0 ---数据长度,当数据为 0 时,弹空了栈
    self.data = {nil,nil,nil,nil,nil,nil,nil,nil,nil,nil,}---数据容器,使用数组实现,不使用链表实现
end

--有多少个数据
function Stack:Count()
    return self.length
end

--清空
function Stack:Clear()
    self.data = nil
    self.length = nil
    self.name = nil
    collectgarbage("collect")
end

--弹出数据
function Stack:Pop()
    if self.length <= 0 then
        error("Stack is empty")
    end
    local firstValue = self.data[self.length]
    self.data[self.length] = nil
    self.length = self.length - 1
    if self.length <= 0 then
        self.length = 0
        self.data = {nil,nil,nil,nil,nil,nil,nil,nil,nil,nil,}---数据容器,使用数组实现,不使用链表实现
        collectgarbage("collect")
    end
    return firstValue
end

--查看队列首端的值
function Stack:Peek()
    if self.length <= 0 then
        error("Stack is empty")
    end
    return self.data[self.length]
end

--压入数据
function Stack:Push(value)
    self.length = self.length + 1
    self.data[self.length] = value
end

--包含某个值
function Stack:Contains(value)
    for i = 1, self.length do
        if value == self.data[i] then
            return true
        end
    end
    return false
end




--[[
队列用法:
local myStack = Stack:new("Stack的名字") //创建了一个 Stack 对象
myStack:Push(value)
myStack:Pop()
if _stackMessage:Count() <= 0 then 
 
end
]]--
