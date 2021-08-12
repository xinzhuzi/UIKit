Stack = {}
Stack.__index = Stack

function Stack.New()
    local self = {}
    self.stack_table = {}
    setmetatable(self,Stack)
    return self
end

function Stack:push(element)
    local size = self:size()
    self.stack_table[size+1] = element
end

function Stack:pop()   
    local size = self:size()
    if self:isEmpty() then
        print("Error:Stack is empty!")
        return nil
    end
    return table.remove(self.stack_table,size)
end

function Stack:peek()   
    local size = self:size()
    if self:isEmpty() then
        print("Error:Stack is Empty")
        return nil
    end
    return self.stack_table[size]
end

function Stack:isEmpty()
    local size = self:size()
    if size == 0 then
        return true
    end
    return false
end

function Stack:size()    
    return #self.stack_table or 0
end

function Stack:clear()
    self.stack_table = nil
    self.stack_table = {}
end

function Stack:printElement()
    local size = self:size()
    if self:isEmpty() then
        print("Error: Stack is empty!")
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
function Stack:RemoveElementFromStack(element)
    if self:isEmpty() then
        print("Error: Stack is empty!")
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
function Stack:IsExistElement(element)
    if self:isEmpty() then
        print("Error: Stack is empty!")
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