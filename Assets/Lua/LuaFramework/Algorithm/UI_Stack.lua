--[[--
 * @Description:	UI专用的栈
 * @Author:			陈俊家
 * @Path:			UI_Stack
 * @DateTime:		2019/3/19 15:48
]]

UI_Stack = {}
UI_Stack.__index = UI_Stack

function UI_Stack.New()
    local self = {}
    self.stack_table = {}
    setmetatable(self,UI_Stack)
    return self
end

function UI_Stack:push(element)
    local size = self:size()
    self.stack_table[size+1] = element
end

function UI_Stack:pop()   
    local size = self:size()
    if self:isEmpty() then
        print("pop Warning:UI_Stack is empty!")
        -- print_stack()
        return nil
    end
    return table.remove(self.stack_table,size)
end

function UI_Stack:peek()   
    local size = self:size()
    if self:isEmpty() then
        print("peek Warning:UI_Stack is Empty")
        -- print_stack()
        return nil
    end
    return self.stack_table[size]
end

function UI_Stack:isEmpty()
    local size = self:size()
    if size == 0 then
        return true
    end
    return false
end

function UI_Stack:size()    
    return #self.stack_table or 0
end

function UI_Stack:clear()
    self.stack_table = nil
    self.stack_table = {}
end

function UI_Stack:printElement()
    local size = self:size()
    if self:isEmpty() then
        print("Warning: UI_Stack is empty!")
        -- print_stack()
        return
    end
    local str = "{"..self.stack_table[size]
    size = size -1
    while size > 0 do
        str = str .. ", "..self.stack_table[size]
        size = size - 1
    end
    str = str .. "}"
    print(str)
end

--移除栈中的某个元素
function UI_Stack:RemoveElementFromStack(element)
    if self:isEmpty() then
        print("Warning: UI_Stack is empty!")
        -- print_stack()
        return
    end
    local size = self:size()
    for i = 1, size do
        if self.stack_table[i] == element then
            table.remove(self.stack_table,i)
            break
        end
    end
    
end

--判断栈中是否存在某个元素
function UI_Stack:IsExistElement(element)
    if self:isEmpty() then

        print("Warning: UI_Stack is empty!很大概率是该对象没有Show就执行了CLOSE,或者重复调用了自己的CLOSE接口element"..element.Path)
        -- print_stack()
        return
    end
    local size = self:size()
    for i = 1, size do
        if self.stack_table[i] == element then
           return true
        end
    end
    return false
end