require("LuaFramework/Global")







--主入口函数。从这里开始lua逻辑
function Main()					
	print("logic start")
	OpenUI(UIModule.gm)

	OpenUI(UIModule.empty_background)
	OpenUI(UIModule.login)
	
end

--场景切换通知
function OnLevelWasLoaded(level)
	collectgarbage("collect")
	Time.timeSinceLevelLoad = 0
end

function OnApplicationQuit()
	
end