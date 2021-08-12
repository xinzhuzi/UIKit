namespace UnityEngine.UI
{
	[AddComponentMenu("UI/UIImageCircle", 60)]
	public class UIImageCircle : Image
	{
		// //分成多少份弧度
		// [Range(4, 360)] 
		// [SerializeField]
		// private int _segments = 36;
		// protected override void OnPopulateMesh(VertexHelper toFill)
		// {
		// 	toFill.Clear();
		//
		// 	float width = rectTransform.rect.width;
		// 	float height = rectTransform.rect.height;
		// 	
		// 	//得到 UV 信息
		//     Vector4 uv = overrideSprite ?  DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;
		//     //获取 UV 的宽高
		//     float uvWidth = uv.z - uv.x;
		//     float uvHeight = uv.w - uv.x;
		// 	//从 UV 信息中得到中心点.
		// 	Vector2 uvCenter = new Vector2(uvWidth * 0.5f, uvHeight * 0.5f);
		// 	//UV与图片的比例
		// 	Vector2 convertRotio = new Vector2(uvWidth / width, uvHeight / height);
		// 	//一个圆的弧度就是 2 PI , 弧长 = 角度数 * π * 圆半径 / 180 ;
		// 	//角度与弧度的计算: 360 度 = 2 PI ; 1角度 =  PI / 180 ; 1弧度 = 180/PI
		// 	double radian = (2 * Math.PI) / _segments;//得到弧度
		// 	double radius = width * 0.5f;//半径
		// 	// double angle = (radian * 180 / Math.PI);//角度,通过弧度得到角度
		// 	// double a = angle * Mathf.PI * radius / 180;//得到弧长
		//
		// 	UIVertex origin = new UIVertex();
		// 	origin.color = color;
		// 	origin.position = Vector3.zero;//位置--中心点
		// 	origin.uv0 = new Vector2(origin.position.x * convertRotio.x,origin.position.y * convertRotio.y);
		// 	toFill.AddVert(origin);
		// 	int verTexCount = _segments + 1;
		// 	double curRadian = 0f;
		// 	for (int i = 0; i < verTexCount; i++)
		// 	{
		// 		double x= Mathf.Cos((float)curRadian) * radius;
		// 		double y = Mathf.Sin((float)curRadian) * radius;
		// 		curRadian += radian;
		//
		// 		UIVertex vertexTemp = new UIVertex();
		// 		vertexTemp.color = color;
		// 		vertexTemp.position = new Vector2((float)x,(float)y);
		// 		vertexTemp.uv0 = new Vector2(origin.position.x * convertRotio.x, origin.position.y * convertRotio.y);
		// 		toFill.AddVert(vertexTemp;vcddddddccc
		//
		// 	}
		//
		// }


		private const int FILL_PERCENT = 100;
		private float thickness = 5;

		[Range(4, 360)] 
		[SerializeField]
		private int _segments = 36;

		public int segments
		{
			get { return _segments; }
			set
			{
				if (_segments != value)
				{
					_segments = value;
					SetVerticesDirty();
#if UNITY_EDITOR
					UnityEditor.EditorUtility.SetDirty(transform);
#endif
				}
			}
		}


		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();
			this.thickness = (float) Mathf.Clamp(this.thickness, 0, rectTransform.rect.width / 2);
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			float outer = -rectTransform.pivot.x * rectTransform.rect.width;
			float inner = -rectTransform.pivot.x * rectTransform.rect.width + this.thickness;

			vh.Clear();

			Vector2 prevX = Vector2.zero;
			Vector2 prevY = Vector2.zero;
			Vector2 uv0 = new Vector2(0, 0);
			Vector2 uv1 = new Vector2(0, 1);
			Vector2 uv2 = new Vector2(1, 1);
			Vector2 uv3 = new Vector2(1, 0);
			Vector2 pos0;
			Vector2 pos1;
			Vector2 pos2;
			Vector2 pos3;

			float tw = rectTransform.rect.width;
			float th = rectTransform.rect.height;

			float angleByStep = (FILL_PERCENT / 100f * (Mathf.PI * 2f)) / segments;
			float currentAngle = 0f;
			for (int i = 0; i < segments + 1; i++)
			{

				float c = Mathf.Cos(currentAngle);
				float s = Mathf.Sin(currentAngle);

				StepThroughPointsAndFill(outer, inner, ref prevX, ref prevY, out pos0, out pos1, out pos2, out pos3, c,
					s);

				uv0 = new Vector2(pos0.x / tw + 0.5f, pos0.y / th + 0.5f);
				uv1 = new Vector2(pos1.x / tw + 0.5f, pos1.y / th + 0.5f);
				uv2 = new Vector2(pos2.x / tw + 0.5f, pos2.y / th + 0.5f);
				uv3 = new Vector2(pos3.x / tw + 0.5f, pos3.y / th + 0.5f);

				vh.AddUIVertexQuad(SetVbo(new[] {pos0, pos1, pos2, pos3}, new[] {uv0, uv1, uv2, uv3}));

				currentAngle += angleByStep;
			}
		}

		private void StepThroughPointsAndFill(float outer, float inner, ref Vector2 prevX, ref Vector2 prevY,
			out Vector2 pos0, out Vector2 pos1, out Vector2 pos2, out Vector2 pos3, float c, float s)
		{
			pos0 = prevX;
			pos1 = new Vector2(outer * c, outer * s);

			pos2 = Vector2.zero;
			pos3 = Vector2.zero;

			prevX = pos1;
			prevY = pos2;
		}

		protected UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs)
		{
			UIVertex[] vbo = new UIVertex[4];
			for (int i = 0; i < vertices.Length; i++)
			{
				var vert = UIVertex.simpleVert;
				vert.color = color;
				vert.position = vertices[i];
				vert.uv0 = uvs[i];
				vbo[i] = vert;
			}

			return vbo;
		}

	}
}