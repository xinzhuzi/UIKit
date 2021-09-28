using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using UnityEngine;

namespace PSD2UGUI
{
    [DebuggerDisplay("Name = {Name}")]
    public class Layer
    {
        private static int instanceCount = 0;
        public readonly int Id = 0;
        internal PsdFile PsdFile { get; private set; }

        /// <summary>
        /// The rectangle containing the contents of the layer.
        /// </summary>
        public Rect Rect { get; set; }

        /// <summary>
        /// Image channels.
        /// </summary>
        public ChannelList Channels { get; protected set; }

        /// <summary>
        /// Returns alpha channel if it exists, otherwise null.
        /// </summary>
        public Channel AlphaChannel
        {
            get
            {
                if (Channels.ContainsId(-1))
                    return Channels.GetId(-1);
                else
                    return null;
            }
        }

        protected string blendModeKey;

        /// <summary>
        /// Photoshop blend mode key for the layer
        /// </summary>
        public string BlendModeKey
        {
            get => blendModeKey;
            private set
            {
                if (value.Length != 4) throw new ArgumentException("Key length must be 4");
                blendModeKey = value;
            }
        }

        /// <summary>
        /// 0 = transparent ... 255 = opaque
        /// </summary>
        public byte Opacity { get; set; }

        /// <summary>
        /// false = base, true = non-base
        /// </summary>
        public bool Clipping { get; set; }

        private static int protectTransBit = BitVector32.CreateMask();
        private static int visibleBit = BitVector32.CreateMask(protectTransBit);
        BitVector32 flags = new BitVector32();

        /// <summary>
        /// If true, the layer is visible.
        /// </summary>
        public bool Visible
        {
            get { return !flags[visibleBit]; }
            set { flags[visibleBit] = !value; }
        }

        /// <summary>
        /// Protect the transparency
        /// </summary>
        public bool ProtectTrans
        {
            get => flags[protectTransBit];
            set => flags[protectTransBit] = value;
        }

        /// <summary>
        /// The descriptive layer name
        /// </summary>
        public string Name { get; set; }

        public BlendingRanges BlendingRangesData { get; set; }

        public MaskInfo Masks { get; set; }

        public List<LayerInfo> AdditionalInfo { get; set; }

        private LayerGroup _parent = null;

        public LayerGroup Parent
        {
            get => _parent;
            set
            {
                if (_parent == value) return;
                if (value == null || _parent != null)
                {
                    var previousParent = _parent;
                    _parent = null;
                    previousParent.RemoveChild(this);
                    if (value == null) return;
                }

                _parent = value;
                _parent.AddChild(this);
            }
        }

        ///////////////////////////////////////////////////////////////////////////
        public Layer(Layer other)
        {
            // Id = instanceCount++;
            Id = other.Id;
            Rect = other.Rect;
            Channels = other.Channels;
            blendModeKey = other.blendModeKey;
            Opacity = other.Opacity;
            Clipping = other.Clipping;
            flags = other.flags;
            Name = other.Name;
            BlendingRangesData = other.BlendingRangesData;
            Masks = other.Masks;
            AdditionalInfo = other.AdditionalInfo;
            _parent = other._parent;
        }

        public Layer(PsdFile psdFile)
        {
            Id = instanceCount++;
            PsdFile = psdFile;
            Rect = new Rect();
            Channels = new ChannelList();
            BlendModeKey = PsdBlendModeType.NORMAL;
            AdditionalInfo = new List<LayerInfo>();
        }

        public Layer(PsdBinaryReader reader, PsdFile psdFile)
            : this(psdFile)
        {
            Rect = reader.ReadRectangle();

            //-----------------------------------------------------------------------
            // Read channel headers.  Image data comes later, after the layer header.

            int numberOfChannels = reader.ReadUInt16();
            for (int channel = 0; channel < numberOfChannels; channel++)
            {
                var ch = new Channel(reader, this);
                Channels.Add(ch);
            }

            //-----------------------------------------------------------------------
            // 

            var signature = reader.ReadAsciiChars(4);
            if (signature != "8BIM")
                throw (new PsdInvalidException("Invalid signature in layer header."));

            BlendModeKey = reader.ReadAsciiChars(4);
            Opacity = reader.ReadByte();
            Clipping = reader.ReadBoolean();

            var flagsByte = reader.ReadByte();
            flags = new BitVector32(flagsByte);

            reader.ReadByte(); //padding

            //-----------------------------------------------------------------------

            // This is the total size of the MaskData, the BlendingRangesData, the 
            // Name and the AdjustmentLayerInfo.
            var extraDataSize = reader.ReadUInt32();
            var extraDataStartPosition = reader.BaseStream.Position;

            Masks = new MaskInfo(reader, this);
            BlendingRangesData = new BlendingRanges(reader, this);
            Name = reader.ReadPascalString(4);

            //-----------------------------------------------------------------------
            // Process Additional Layer Information

            long adjustmentLayerEndPos = extraDataStartPosition + extraDataSize;
            while (reader.BaseStream.Position < adjustmentLayerEndPos)
            {
                var layerInfo = LayerInfoFactory.Load(reader);
                AdditionalInfo.Add(layerInfo);
                if (layerInfo is LayerSectionInfo)
                {
                    if (((LayerSectionInfo) layerInfo).BlendModeKey == null) continue;
                    BlendModeKey = ((LayerSectionInfo) layerInfo).BlendModeKey;
                }
            }

            foreach (var adjustmentInfo in AdditionalInfo)
            {
                switch (adjustmentInfo.Key)
                {
                    case "luni":
                        Name = ((LayerUnicodeName) adjustmentInfo).Name;
                        break;
                }
            }

        }

        public void CopyData(Layer other)
        {
            Rect = other.Rect;
            Channels = other.Channels;
            blendModeKey = other.blendModeKey;
            Opacity = other.Opacity;
            Clipping = other.Clipping;
            flags = other.flags;
            Name = other.Name;
            BlendingRangesData = other.BlendingRangesData;
            Masks = other.Masks;
            AdditionalInfo = other.AdditionalInfo;
        }

        public bool CheckVisibilityInHierarchy()
        {
            var layer = Parent;
            while (layer != null)
            {
                if (!layer.Visible) return false;
                layer = layer.Parent;
            }

            return true;
        }


        private bool _visibleInHierarchy = true;

        public bool VisibleInHierarchy
        {
            get => _visibleInHierarchy;
            set => _visibleInHierarchy = value;
        }

        public float Alpha => Mathf.Round((float) Opacity * 100f / 255f) / 100f;

        public float AlphaInHierarchy
        {
            get
            {
                var result = Alpha;
                var layer = Parent;
                while (layer != null)
                {
                    result *= layer.Alpha;
                    layer = layer.Parent;
                }

                return result;
            }
        }

        public PsdBlendModeType BlendModeInHierarchy
        {
            get
            {
                PsdBlendModeType result = BlendModeKey;
                var layer = Parent;
                while (layer != null)
                {
                    if (layer.BlendModeKey != PsdBlendModeType.PASS_THROUGH.Key)
                    {
                        result = layer.BlendModeKey;
                    }

                    layer = layer.Parent;
                }

                return result;
            }
        }

        public int HierarchyDepth
        {
            get
            {
                int result = 0;
                var layer = Parent;
                while (layer != null)
                {
                    layer = layer.Parent;
                    ++result;
                }

                return result;
            }
        }

        public static void ResetInstanceCount()
        {
            instanceCount = 0;
        }
    }

    [DebuggerDisplay("Name = {Name}")]
    public class LayerGroup : Layer
    {
        private List<Layer> _chlindren = new List<Layer>();
        public Layer[] Children => _chlindren.ToArray();
        public readonly bool IsOpen = false;

        public LayerGroup(Layer layer) : base(layer)
        {
        }

        public LayerGroup(Layer layer, bool isOpen) : base(layer)
        {
            IsOpen = isOpen;
        }

        public LayerGroup(Layer layer, Layer[] children, bool isOpen) : base(layer)
        {
            IsOpen = isOpen;
            AddChildren(children);
        }

        public bool HasChild(Layer child)
        {
            return _chlindren.Contains(child);
        }

        public void AddChild(Layer child)
        {
            if (HasChild(child)) return;

            if (child.Parent != null && child.Parent != this)
            {
                child.Parent.RemoveChild(child);
            }

            _chlindren.Add(child);
            child.Parent = this;
            child.VisibleInHierarchy = this.Visible && this.VisibleInHierarchy;
        }

        public void AddChildren(Layer[] children)
        {
            foreach (var layer in children)
            {
                AddChild(layer);
            }
        }

        public bool RemoveChild(Layer child)
        {
            if (!HasChild(child)) return false;
            _chlindren.Remove(child);
            child.Parent = null;
            return true;
        }
    }


}
