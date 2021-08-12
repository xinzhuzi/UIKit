﻿using UnityEngine;
using System;
using System.Collections.Generic;
using LuaInterface;
using UnityEditor;

using BindType = ToLuaMenu.BindType;
using System.Reflection;
using UIKit;

public static class CustomSettings
{
    public static string saveDir = Application.dataPath + "/Source/Generate/";    
    public static string toluaBaseType = Application.dataPath + "/ToLua/BaseType/";
    public static string baseLuaDir = Application.dataPath + "/ToLua/Lua/";
    public static string injectionFilesPath = Application.dataPath + "/ToLua/Injection/";

    //导出时强制做为静态类的类型(注意customTypeList 还要添加这个类型才能导出)
    //unity 有些类作为sealed class, 其实完全等价于静态类
    public static List<Type> staticClassTypes = new List<Type>
    {        
        typeof(UnityEngine.Application),
        typeof(UnityEngine.Time),
        typeof(UnityEngine.Screen),
        typeof(UnityEngine.SleepTimeout),
        typeof(UnityEngine.Input),
        typeof(UnityEngine.Resources),
        typeof(UnityEngine.Physics),
        typeof(UnityEngine.RenderSettings),
        typeof(UnityEngine.QualitySettings),
        typeof(UnityEngine.GL),
        typeof(UnityEngine.Graphics),
    };

    //附加导出委托类型(在导出委托时, customTypeList 中牵扯的委托类型都会导出， 无需写在这里)
    public static DelegateType[] customDelegateList = 
    {        
        _DT(typeof(Action)),                
        _DT(typeof(UnityEngine.Events.UnityAction)),
        _DT(typeof(System.Predicate<int>)),
        _DT(typeof(System.Action<int>)),
        _DT(typeof(System.Comparison<int>)),
        _DT(typeof(System.Func<int, int>)),
    };

    //在这里添加你要导出注册到lua的类型列表
    public static BindType[] customTypeList =
    {
        _GT(typeof(LuaInjectionStation)),
        _GT(typeof(InjectType)),
        _GT(typeof(Debugger)).SetNameSpace(null),          

#if USING_DOTWEENING
        _GT(typeof(DG.Tweening.DOTween)),
        _GT(typeof(DG.Tweening.Tween)).SetBaseType(typeof(System.Object)).AddExtendType(typeof(DG.Tweening.TweenExtensions)),
        _GT(typeof(DG.Tweening.Sequence)).AddExtendType(typeof(DG.Tweening.TweenSettingsExtensions)),
        _GT(typeof(DG.Tweening.Tweener)).AddExtendType(typeof(DG.Tweening.TweenSettingsExtensions)),
        _GT(typeof(DG.Tweening.LoopType)),
        _GT(typeof(DG.Tweening.PathMode)),
        _GT(typeof(DG.Tweening.PathType)),
        _GT(typeof(DG.Tweening.RotateMode)),
        _GT(typeof(Component)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Transform)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Light)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Material)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Rigidbody)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Camera)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(AudioSource)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        //_GT(typeof(LineRenderer)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        //_GT(typeof(TrailRenderer)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),    
#else
                                         
        _GT(typeof(Component)),
        _GT(typeof(Transform)),
        _GT(typeof(Material)),
        _GT(typeof(Light)),
        _GT(typeof(Rigidbody)),
        _GT(typeof(Camera)),
        _GT(typeof(AudioSource)),
        //_GT(typeof(LineRenderer))
        //_GT(typeof(TrailRenderer))
#endif
      
        _GT(typeof(Behaviour)),
        _GT(typeof(MonoBehaviour)),        
        _GT(typeof(GameObject)),
        _GT(typeof(TrackedReference)),
        _GT(typeof(Application)),
        _GT(typeof(Physics)),
        _GT(typeof(Collider)),
        _GT(typeof(Time)),        
        _GT(typeof(Texture)),
        _GT(typeof(Texture2D)),
        _GT(typeof(Shader)),        
        _GT(typeof(Renderer)),
        _GT(typeof(Screen)),        
        _GT(typeof(CameraClearFlags)),
        _GT(typeof(AudioClip)),        
        _GT(typeof(AssetBundle)),
        _GT(typeof(ParticleSystem)),
        _GT(typeof(AsyncOperation)).SetBaseType(typeof(System.Object)),        
        _GT(typeof(LightType)),
        _GT(typeof(SleepTimeout)),
        _GT(typeof(Animator)),
        _GT(typeof(Input)),
        _GT(typeof(KeyCode)),
        _GT(typeof(SkinnedMeshRenderer)),
        _GT(typeof(Space)),      
       

        _GT(typeof(MeshRenderer)),
        _GT(typeof(BoxCollider)),
        _GT(typeof(MeshCollider)),
        _GT(typeof(SphereCollider)),        
        _GT(typeof(CharacterController)),
        _GT(typeof(CapsuleCollider)),
        
        _GT(typeof(Animation)),        
        _GT(typeof(AnimationClip)).SetBaseType(typeof(UnityEngine.Object)),        
        _GT(typeof(AnimationState)),
        _GT(typeof(AnimationBlendMode)),
        _GT(typeof(QueueMode)),  
        _GT(typeof(PlayMode)),
        _GT(typeof(WrapMode)),

        _GT(typeof(QualitySettings)),
        _GT(typeof(RenderSettings)),                                                   
        _GT(typeof(SkinWeights)),           
        _GT(typeof(RenderTexture)),
        _GT(typeof(Resources)),     
        _GT(typeof(LuaProfiler)),
 
        
        
        // UGUI & TextMeshPro
        _GT(typeof(UIManager)),
        _GT(typeof(UIHelper)),
        _GT(typeof(UIAdapter)),
        _GT(typeof(LocalisationManager)),
        _GT(typeof(LocalisationText)),
        _GT(typeof(UnityEngine.UI.UIDoubleClickListener)),
        _GT(typeof(UnityEngine.UI.UIDragListener)),
        // _GT(typeof(UnityEngine.UI.UIEventListener)),
        _GT(typeof(UnityEngine.UI.UILongPressListener)),
        _GT(typeof(UnityEngine.UI.UIPointAllListener)),
        _GT(typeof(UnityEngine.UI.UIPointClickListener)),
        _GT(typeof(UnityEngine.UI.UIScrollListener)),
        _GT(typeof(UnityEngine.EventSystems.EventSystem)),
        _GT(typeof(UnityEngine.EventSystems.PointerEventData)),
        _GT(typeof(UnityEngine.UI.Image)),
        _GT(typeof(UnityEngine.UI.RawImage)),
        _GT(typeof(TMPro.TextMeshPro)),
        _GT(typeof(TMPro.TextMeshProUGUI)),
        _GT(typeof(TMPro.TMP_Dropdown)),
        _GT(typeof(TMPro.TMP_InputField)),
        _GT(typeof(UnityEngine.UI.Toggle)),
        _GT(typeof(UnityEngine.UI.ToggleGroup)),
        _GT(typeof(UnityEngine.UI.Slider)),
        _GT(typeof(UnityEngine.UI.ScrollView)),
        _GT(typeof(UnityEngine.UI.Scrollbar)),
        _GT(typeof(UnityEngine.UI.RectMask2D)),
        _GT(typeof(UnityEngine.UI.Mask)),
        _GT(typeof(UnityEngine.UI.GraphicRaycaster)),
        _GT(typeof(UnityEngine.UI.Button)),
        _GT(typeof(UnityEngine.UI.GridLayoutGroup)),
        _GT(typeof(UnityEngine.UI.HorizontalLayoutGroup)),
        _GT(typeof(UnityEngine.UI.VerticalLayoutGroup)),
        _GT(typeof(UnityEngine.UI.CanvasScaler)),
        _GT(typeof(UnityEngine.Canvas)),
        _GT(typeof(UnityEngine.CanvasGroup)),
        _GT(typeof(UnityEngine.CanvasRenderer)),
        _GT(typeof(UnityEngine.RectTransform)),
        _GT(typeof(UnityEngine.U2D.SpriteAtlas)),
        _GT(typeof(UnityEngine.Sprite)),
        _GT(typeof(UnityEngine.UI.UITableView)),
        _GT(typeof(UnityEngine.UI.UITableCell)),
    };

    public static List<Type> dynamicList = new List<Type>()
    {
        typeof(MeshRenderer),
#if !UNITY_5_4_OR_NEWER
        typeof(ParticleEmitter),
        typeof(ParticleRenderer),
        typeof(ParticleAnimator),
#endif

        typeof(BoxCollider),
        typeof(MeshCollider),
        typeof(SphereCollider),
        typeof(CharacterController),
        typeof(CapsuleCollider),

        typeof(Animation),
        typeof(AnimationClip),
        typeof(AnimationState),

        typeof(SkinWeights),
        typeof(RenderTexture),
        typeof(Rigidbody),
        
        
        
    };

    //重载函数，相同参数个数，相同位置out参数匹配出问题时, 需要强制匹配解决
    //使用方法参见例子14
    public static List<Type> outList = new List<Type>()
    {
        
    };
        
    //ngui优化，下面的类没有派生类，可以作为sealed class
    public static List<Type> sealedList = new List<Type>()
    {
        // UGUI & TextMeshPro
        typeof(UIManager),
        typeof(UIHelper),
        typeof(UIAdapter),
        typeof(LocalisationManager),
        typeof(LocalisationText),
        typeof(UnityEngine.UI.UIDoubleClickListener),
        typeof(UnityEngine.UI.UIDragListener),
        typeof(UnityEngine.UI.UIEventListener),
        typeof(UnityEngine.UI.UILongPressListener),
        typeof(UnityEngine.UI.UIPointAllListener),
        typeof(UnityEngine.UI.UIPointClickListener),
        typeof(UnityEngine.UI.UIScrollListener),
        typeof(UnityEngine.EventSystems.EventSystem),
        typeof(UnityEngine.EventSystems.PointerEventData),
        typeof(UnityEngine.UI.Image),
        typeof(UnityEngine.UI.RawImage),
        typeof(TMPro.TextMeshPro),
        typeof(TMPro.TextMeshProUGUI),
        typeof(TMPro.TMP_Dropdown),
        typeof(TMPro.TMP_InputField),
        typeof(UnityEngine.UI.Toggle),
        typeof(UnityEngine.UI.ToggleGroup),
        typeof(UnityEngine.UI.Slider),
        typeof(UnityEngine.UI.ScrollView),
        typeof(UnityEngine.UI.Scrollbar),
        typeof(UnityEngine.UI.RectMask2D),
        typeof(UnityEngine.UI.Mask),
        typeof(UnityEngine.UI.GraphicRaycaster),
        typeof(UnityEngine.UI.Button),
        typeof(UnityEngine.UI.GridLayoutGroup),
        typeof(UnityEngine.UI.HorizontalLayoutGroup),
        typeof(UnityEngine.UI.VerticalLayoutGroup),
        typeof(UnityEngine.UI.CanvasScaler),
        typeof(UnityEngine.Canvas),
        typeof(UnityEngine.CanvasGroup),
        typeof(UnityEngine.CanvasRenderer),
        typeof(UnityEngine.RectTransform),
        typeof(UnityEngine.U2D.SpriteAtlas),
        typeof(UnityEngine.Sprite),
        typeof(UnityEngine.UI.UITableView),
        typeof(UnityEngine.UI.UITableCell),
    };

    public static BindType _GT(Type t)
    {
        return new BindType(t);
    }

    public static DelegateType _DT(Type t)
    {
        return new DelegateType(t);
    }    


    [MenuItem("Lua/Attach Profiler", false, 151)]
    static void AttachProfiler()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("警告", "请在运行时执行此功能", "确定");
            return;
        }

        LuaClient.Instance.AttachProfiler();
    }

    [MenuItem("Lua/Detach Profiler", false, 152)]
    static void DetachProfiler()
    {
        if (!Application.isPlaying)
        {            
            return;
        }

        LuaClient.Instance.DetachProfiler();
    }
}
