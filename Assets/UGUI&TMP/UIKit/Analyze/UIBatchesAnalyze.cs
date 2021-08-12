#if UNITY_EDITOR


using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

namespace UIKit
{
    internal static class UIBatchesAnalyze
    {
        
        //Hierarchy 中的所有图形的数据
        //合批ID/深度/材质ID/贴图ID
        internal class GraphicData
        {
            public string name;             //名字
            public Graphic graphic;         //此对象
            public string path;             //全路径,是指在其 Hierarchy 中的路径
            public int batchID;             //合批ID
            public int relativeDepth;       //自身的相对于第一个父 canvas 的深度
            public int materialID;          //材质ID
            public int textureID;           //贴图ID
        }

        private static List<Canvas> AllCanvas = new List<Canvas>();
        private static Dictionary<Graphic, GraphicData> AllGraphics = new Dictionary<Graphic, GraphicData>(100);

        //一个 canvas 下对应多少个子物体,分析这些子物体,对其进行合批优化分析
        private static Dictionary<Canvas, List<GraphicData>> CGDDict = new Dictionary<Canvas, List<GraphicData>>();
        private static StringBuilder log = new StringBuilder(1000);
        private static string AnalyzePath = Application.dataPath.Replace("Assets", "AnalyzeBatches.txt");
        

        public static void AnalyzeOverlaps()
        {
            InitAllCanvas();//初始化所有 canvas
            InitAllGraphics();//初始化所有图形节点
            InitAllCanvasWithGraphicData();//获取每个 canvas 下每个 Graphic 的图形信息
            CheckAllGraphicOverlaps();
            
        }
        
        public static void Analyze()
        {
            if (File.Exists(AnalyzePath)) File.Delete(AnalyzePath);
            log.Clear();
            log.Append(
                "1. 优化顺序: CPU(合批,重建,update 中的 Event 事件) > GPU(过渡绘制overdraw,多边形,shadow阴影,outline外轮廓,mask 遮罩等) > Memory(Vertex Count顶点数,Mesh 数据,SubMesh 数据,缓存过载)\n" +
                "2. 先使用FrameDebugger和Profiler查看 UI 问题,对症下药,查看顶点数,打断合批的原因.\n" +
                "3. 在不影响最终效果的情况下,尽量保证 Text 文本在 Image 图片的上方 \n" +
                "4. 文本框内文字数量过多,需要使用 RectMask2D 裁剪文字.\n" +
                "5. 模块拆分,动静分离,图集字体格式尽量少.\n" + 
                "6. UI 模块的缓存模式要及时查看是否导致内存暴涨.\n" +
                "7. 将 Scale 设置为 0 代替 SetActive.\n" + 
                "8. 将 CanvasGroup 挂载到某个节点下面,将Alpha设置为 0,不勾选 Interachtable,不勾选 Blocks Raycasts,勾选 Ignore Parent Groups 用来隐藏一组物体.\n" + 
                "9. 使用多个 Canvas 分层界面,动静分离" 
            );
            
            {
                log.Append("\n========================================开始UI重叠合批分析========================================\n");
                AnalyzeBatches();
                log.Append("\n========================================UI重叠合批分析结束========================================\n");
                File.WriteAllText(AnalyzePath, log.ToString());
                System.Diagnostics.Process.Start(AnalyzePath);
            }
            
        }


        private static void AnalyzeBatches()
        {
            InitAllCanvas();//初始化所有 canvas
            InitAllGraphics();//初始化所有图形节点
            InitAllCanvasWithGraphicData();//获取每个 canvas 下每个 Graphic 的图形信息
            CheckCanvasGraphicBatches();//检查图片的重叠,texture id ,material id,这些信息是否一致等
        }

        private static void InitAllCanvas()
        {
            AllCanvas.Clear();
            foreach (var item in UnityEngine.Object.FindObjectsOfType<Canvas>())//如果 canvas 在 UIPoolManager 或者 CanvasGroup 下,此时这个东西无需记录
            {
                RectTransform rectTransform = item.GetComponent<RectTransform>();
                if (item.gameObject.layer != LayerMask.NameToLayer("UI") || 
                    rectTransform.localScale == Vector3.zero || 
                    IsCanvasGroupChildAndAlpha0(item.transform) || 
                    false == item.gameObject.activeSelf)
                {
                    continue;//如果不是 UI类型;缩放值为 0;是CanvasGroup的子节点,并且CanvasGroup的Alpha的值是0;自身被隐藏了; 不进行收集操作,不渲染的情况都需要排除
                }
                AllCanvas.Add(item);
            }
            if (AllCanvas.Count > 5)
            {
                Debug.Log("<color=yellow>警告:不建议 Canvas 的总显示数量超过 5 个,如果界面过重,建议拆分在下级显示</color>");
            }
        }
        private static void InitAllGraphics()
        {
            AllGraphics.Clear();
            foreach (var item in UnityEngine.Object.FindObjectsOfType<Graphic>())
            {
                RectTransform rectTransform = item.GetComponent<RectTransform>();
                if (item.gameObject.layer != LayerMask.NameToLayer("UI") || 
                    rectTransform.localScale == Vector3.zero || 
                    IsCanvasGroupChildAndAlpha0(item.transform) || 
                    false == item.gameObject.activeSelf)
                {
                    continue;//如果不是 UI类型;缩放值为 0;是CanvasGroup的子节点,并且CanvasGroup的Alpha的值是0; 不进行收集操作,不渲染的情况都需要排除
                }

                if (item.GetType().ToString().Contains("TMP_SubMeshUI")) continue;//不计算此类
                
                int tmpMaterialID = item.material.GetInstanceID();
                int tmpTextureID = item.mainTexture.GetNativeTexturePtr().ToInt32();
                
                if (item.GetType().ToString().Contains("TextMeshProUGUI"))
                {
                    tmpMaterialID = item.materialForRendering.GetInstanceID();
                    tmpTextureID = item.materialForRendering.mainTexture.GetNativeTexturePtr().ToInt32();
                }
                
                
                GraphicData graphicData = new GraphicData()
                {
                    name = item.gameObject.name,
                    graphic = item,
                    path = FindPath(item.transform),
                    batchID = -1,//目前只是收集数据,并不是进行合批分析.
                    relativeDepth = item.canvasRenderer.relativeDepth,
                    materialID = tmpMaterialID,
                    textureID = tmpTextureID,
                };
                AllGraphics.Add(item,graphicData);                
            }
        }

        private static void InitAllCanvasWithGraphicData()
        {
            CGDDict.Clear();
            foreach (var ItemCanvas in AllCanvas)
            {
                if (!CGDDict.TryGetValue(ItemCanvas,out List<GraphicData> graphicDatas))
                {
                    graphicDatas = new List<GraphicData>();
                }
                
                foreach (var graphic in AllGraphics)
                {
                    if (ItemCanvas == graphic.Key.canvas)
                    {
                        graphicDatas.Add(graphic.Value);
                    }
                }

                CGDDict[ItemCanvas] = graphicDatas;
            }
            
            //获得了一个 canvas 对应多个Graphic的数据
            //下个步骤要分析每个 canvas 下的所有信息,查看是否可以合批
        }

        private static void CheckCanvasGraphicBatches()
        {
            /**
             * 合批规则第一条:
             * Canvas 计算自己的合批
             * Canvas 中的 Grapthic 先使用 CanvasRenderer 中的 relativeDepth 最高的先渲染
             * 检查重叠
             * 如果重叠,判断当前的 material.id 与 texture.id 是否一致,一致可以合批,不一致则将 depth + 1
             * 如果不重叠,则计算 depth.
             */
            foreach (var item in CGDDict)
            {
                if (item.Value.Count<=0) continue;
                

                {//计算没有重叠的情况
                    Dictionary<string, List<GraphicData>> dictOptimal = new Dictionary<string, List<GraphicData>>();
                    foreach (var graphicData in item.Value)//共有这么多的图形
                    {
                        //图形相同的 material.id / texture.id ,就表示最优可以合批多少个了
                        string key = "material.id:" + graphicData.materialID + " && texture.id:" + graphicData.textureID;
                        if (!dictOptimal.TryGetValue(key,out List<GraphicData> batchGraphic))
                        {
                            batchGraphic = new List<GraphicData>();
                        }
                        batchGraphic.Add(graphicData);
                        dictOptimal[key] = batchGraphic;
                    }

                    log.Append(
                        $"\n---[[[\n{item.Key.name}(Canvas) 极限情况下可以优化到 {dictOptimal.Count} 个 Batches;\n");
                    if (dictOptimal.Count > 7)
                    {
                        log.Append("极限情况下都能达到 7 个,请考虑拆分模块,我发誓一定查看是否图集使用多套,文本使用多种格式了?\n");
                    }
                    
                    
                    foreach (var kv in dictOptimal)
                    {
                        log.Append("\n\n     可以合成一批," + kv.Key + "\n");

                        foreach (var graphicData in kv.Value)
                        {
                            log.Append(graphicData.name + ",");
                        }
                    }
                }
                log.Append("\n]]]---\n\n\n---[[[\n");
                int length = log.Length;
                {//打印输出所有的重叠链条
                    List<GraphicData> graphicDatas = new List<GraphicData>(item.Value);//根据 depth 进行排序
                    graphicDatas.Sort((x, y) => y.relativeDepth.CompareTo(x.relativeDepth));
                    log.Append($"{item.Key.name}(Canvas) 的 UI 元素重叠链:\n");

                    //从relativeDepth最高的地方向下计算重叠.
                    for (int i = 0; i < graphicDatas.Count-1; i++)
                    {
                        var ii = graphicDatas[i].graphic;
                        //先检查有没有重叠,有重叠就打印输出,没有就不打印输出
                        bool isHaveOverlaps = false;
                        for (int j = (1 + i); j < graphicDatas.Count; j++)
                        {
                            var jj = graphicDatas[j].graphic;
                            if (IsOverlaps(ii.rectTransform,jj.rectTransform))
                            {
                                isHaveOverlaps = true;
                                break;
                            }
                        }

                        if (!isHaveOverlaps) continue; //没有重叠,就跳过

                        
                        log.Append("------------------------------------------------------------------\n");
                        log.Append(FindPath(ii.transform) + "-->\n");
                        for (int j = (1 + i); j < graphicDatas.Count; j++)
                        {
                            var jj = graphicDatas[j].graphic;
                            if (IsOverlaps(ii.rectTransform,jj.rectTransform))
                            {
                                log.Append(FindPath(jj.transform) + "-->\n");
                            }
                        }
                    }

                    log.Append("\n]]]---\n");
                }

                // {//计算有重叠情况下的 batch 数量,计算重叠的元素,打印输出.布局方式,节点排布无法给出建议.只能由开发人员掌控
                //     
                //     List<GraphicData> graphicDatas = new List<GraphicData>(item.Value);//根据 depth 进行排序
                //     graphicDatas.Sort((x, y) => y.relativeDepth.CompareTo(x.relativeDepth));
                //     
                //     //计算重叠情况,从上方计算到最下方的重叠链条.也就是从 relativeDepth 最高的,计算到0,把这种链条计算清晰,再计算合批.
                //     //重叠带来的问题是 overdraw 变高,合批会被打断.
                //     // List<string> dc = new List<string>();
                //     // foreach (var graphicData in graphicDatas) // 所有的图形,从最高的 depth 进行遍历
                //     // {
                //     //     Debug.Log(graphicData.relativeDepth +"      " + graphicData.name);
                //     //     //得到一个 canvas 中的所有数据,谁与谁重叠,这个重叠可能是多重重叠,要记载下来重叠顺序,从最后的重叠到最前方的重叠顺序
                //     //     if (graphicData.graphic.rectTransform.IsOverlaps())
                //     //     {
                //     //         
                //     //     }
                //     //     string key = "material.id:" + graphicData.materialID + " && texture.id:" + graphicData.textureID;
                //     //     if (!dc.Contains(key))
                //     //     {
                //     //         dc.Add(key);
                //     //     }
                //     // }
                //
                //
                // }
                
            }
        }

        private static void CheckAllGraphicOverlaps()
        {
            foreach (var item in CGDDict)
            {
                Debug.Log($"在{item.Key.name}的画布下的重叠对象:\n");
                List<Graphic> r = new List<Graphic>();
                //使用冒泡排序检测所有图形重叠的情况
                for (int i = 0; i < item.Value.Count - 1; i++)
                {
                    var graphicData1 = item.Value[i];
                    for (int j = i + 1; j < item.Value.Count; j++)
                    {
                        var graphicData2 = item.Value[j];
                        bool o = IsOverlaps(graphicData1.graphic.rectTransform,graphicData2.graphic.rectTransform);
                        if (o)
                        {
                            if (!r.Contains(graphicData1.graphic))
                            {
                                r.Add(graphicData1.graphic);
                            }
                            if (!r.Contains(graphicData2.graphic))
                            {
                                r.Add(graphicData2.graphic);
                            }
                            Debug.Log(graphicData1.name + "与" + graphicData2.name + "重叠         ");
                        }
                    }
                }
                
                if (r.Count>=6)
                {
                    Debug.Log($"<color=yellow> 在{item.Key.name}的画布下的重叠元素:{r.Count},重叠元素过多,建议减少重叠元素.</color>");
                }
            }
        }

        #region 工具方法

        private static bool IsOverlaps(RectTransform rt1, RectTransform rt2)
        {
            Camera camera = UIManager.UICamera ? UIManager.UICamera : null;

            Vector3[]corners1 = new Vector3[4];
            rt1.GetWorldCorners(corners1);
            Vector2 v10 = RectTransformUtility.WorldToScreenPoint(camera, corners1[0]);
            Vector2 v11 = RectTransformUtility.WorldToScreenPoint(camera, corners1[2]);
            Rect rect1 = new Rect(v10, v11 - v10);
            
            Vector3[] corners2 = new Vector3[4];
            rt2.GetWorldCorners(corners2);
            Vector2 v20 = RectTransformUtility.WorldToScreenPoint(camera, corners2[0]);
            Vector2 v21 = RectTransformUtility.WorldToScreenPoint(camera, corners2[2]);
            Rect rect2 = new Rect(v20, v21 - v20);
            return rect1.Overlaps(rect2);
        }
        
        private static string FindPath(Transform t)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("/" + t.name);
            while (true)
            {
                Transform parent = t.parent;
                if (parent)
                {
                    stringBuilder.Insert(0, "/" + parent.name);
                    t = parent;
                }
                else
                {
                    break;
                }
            }

            return stringBuilder.ToString();
        }
        
        private static bool IsCanvasGroupChildAndAlpha0(Transform t)
        {
            while (true)
            {
                CanvasGroup canvasGroup = t.GetComponent<CanvasGroup>();//查找其父节点有没有CanvasGroup,并且alpha为 0.
                if (canvasGroup && canvasGroup.alpha == 0)
                {
                    return true;
                }
                else
                {
                    Transform parent = t.parent;
                    if (parent)
                    {
                        t = parent;
                        continue;
                    } 
                }
                return false;
            }
        }
        
        #endregion
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
    }
}


#endif