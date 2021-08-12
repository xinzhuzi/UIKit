local login_view = class("login_view",UIView)
local this = login_view

-------------------------------------变量-----------------------------------
local _controller = nil --控制器
local _account = nil --账号
local _password = nil --密码
local _loginButton = nil --登录按钮

-------------------------------------方法-----------------------------------
local _loginCallback = nil --点击按钮

function this:initView()
    --获取控制器
    _controller = lua_data_center.Get("login")
    
    --获取具体的 UI 节点
    _account = child(_controller.transform,"Account"):GetComponent(typeof(TMP_InputField))
    _password = child(_controller.transform,"Password"):GetComponent(typeof(TMP_InputField))
    _loginButton = child(_controller.transform,"LoginButton")


    --添加事件
    addClick(_loginButton, _loginCallback)

    --适配
    --adapter(xxx)
    --adapter(xxx)

    --初始逻辑
    --SetScale(xxx,false)
    --SetScale(xxx,true)
    
    --xxx
    
    
    
    this:refreshView()
end

--刷新数据
function this:refreshView()
    
    
end


function this:resetView()
    
end

function this:destroyView()
    _controller = nil
end

function _loginCallback(p)
    print("登录了")
    OpenUI(UIModule.main_page)
    CloseUI(UIModule.login)
end



return this