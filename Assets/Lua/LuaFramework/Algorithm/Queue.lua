Queue = {}
Queue.__index = Queue

function Queue.New()
    local self = {}
    self.queue = {}
    self.size = 0
    self.first = nil
    self.last = nil
    setmetatable(self,Queue)
    return self
end

function Queue:isEmpty()
    if self.size == 0 and self.first == nil and self.last == nil then
        return true
    end
    return false
end

function Queue:pushFirst(data)
    local lst = {}
    lst.pre = nil
    lst.value = data
    lst.next = nil
    if self.first == nil then
        self.first = lst
        self.last = lst
    else
        lst.next = self.first
        self.first.pre = lst
        self.first = lst
    end
    self.size = self.size + 1
end

function Queue:popLast()
    if self:isEmpty() then
        print("Error: Queue is empty!")
        return
    end
    local popData = self.last
    local temp = popData.pre
    if temp then
        temp.next = nil
        self.last = temp
    else
        self.last = nil
        self.first = nil
    end
    self.size = self.size -1
    return popData
end

function Queue:printElement()
    local temp = self.first
    if not temp then
        print("Queue is empty")
        return
    end
    local str = "{"
    while temp do
        str = str..temp.value..", "
        temp = temp.next
    end
    str = str.."}"
    print(str)
end