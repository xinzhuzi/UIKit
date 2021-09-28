using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;


namespace PSD2UGUI
{
    public enum PsdColorMode
    {
        Bitmap = 0,
        Grayscale = 1,
        Indexed = 2,
        RGB = 3,
        CMYK = 4,
        Multichannel = 7,
        Duotone = 8,
        Lab = 9
    };

    public class PsdFile
    {
        public readonly string Path;
        public event Action<float> OnProgressChanged;
        public event Action OnDone;
        public event Action<string> OnError;
        private bool _cancel = false;

        /// <summary>
        /// Represents the composite image.
        /// </summary>
        public Layer BaseLayer { get; set; }

        public ImageCompression ImageCompression { get; set; }

        ///////////////////////////////////////////////////////////////////////////

        public PsdFile(string filename = null)
        {
            Path = filename;
            Version = 1;
            BaseLayer = new Layer(this);

            ImageResources = new ImageResources();
            Layers = new List<Layer>();
            RootLayers = new List<Layer>();
            AdditionalInfo = new List<LayerInfo>();
        }

        public PsdFile(string filename, Encoding encoding)
            : this()
        {
            Path = filename;
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                Load(stream, encoding);
            }
        }

        ///////////////////////////////////////////////////////////////////////////

        private void Load(Stream stream, Encoding encoding)
        {
            Layer.ResetInstanceCount();
            var reader = new PsdBinaryReader(stream, encoding);

            if (CancelCheck()) return;
            LoadHeader(reader);
            OnProgressChanged(0.01f);

            if (CancelCheck()) return;
            LoadColorModeData(reader);
            OnProgressChanged(0.02f);

            if (CancelCheck()) return;
            LoadImageResources(reader);
            OnProgressChanged(0.03f);

            if (CancelCheck()) return;
            LoadLayerAndMaskInfo(reader);
            OnProgressChanged(0.04f);

            if (CancelCheck()) return;
            LoadImage(reader);
            OnProgressChanged(0.1f);

            if (CancelCheck()) return;
            DecompressImages();
            if (RootLayers.Count == 0)
            {
                var bgLayer = BaseLayer;
                bgLayer.Name = "Background";
                bgLayer.Opacity = 255;
                RootLayers.Add(bgLayer);
                _layerDictionary.Add(bgLayer.Id, bgLayer);
            }
            OnProgressChanged(1f);
        }

        public void Load()
        {
            try
            {
                using (var stream = new FileStream(Path, FileMode.Open))
                {
                    Load(stream, System.Text.Encoding.Default);
                }
            }
            catch(PsdInvalidException e)
            {
                OnError(e.Message);
            }
            OnDone();
        }

        public void Cancel()
        {
            _cancel = true;
        }

        private bool CancelCheck()
        {
            if (_cancel)
            {
                OnDone();
            }
            return _cancel;
        }

        ///////////////////////////////////////////////////////////////////////////

        #region Header

        /// <summary>
        /// Always equal to 1.
        /// </summary>
        public Int16 Version { get; private set; }

        private Int16 channelCount;
        /// <summary>
        /// The number of channels in the image, including any alpha channels.
        /// </summary>
        public Int16 ChannelCount
        {
            get { return channelCount; }
            set
            {
                if (value < 1 || value > 56)
                    throw new ArgumentException("Number of channels must be from 1 to 56.");
                channelCount = value;
            }
        }

        /// <summary>
        /// The height of the image in pixels.
        /// </summary>
        public int RowCount
        {
            get { return (int)this.BaseLayer.Rect.height; }
            set
            {
                if (value < 0 || value > 30000)
                    throw new ArgumentException("Number of rows must be from 1 to 30000.");
                BaseLayer.Rect = new Rect(0, 0, BaseLayer.Rect.width, value);
            }
        }


        /// <summary>
        /// The width of the image in pixels. 
        /// </summary>
        public int ColumnCount
        {
            get { return (int)this.BaseLayer.Rect.width; }
            set
            {
                if (value < 0 || value > 30000)
                    throw new ArgumentException("Number of columns must be from 1 to 30000.");
                this.BaseLayer.Rect = new Rect(0, 0, value, this.BaseLayer.Rect.height);
            }
        }

        private int bitDepth;
        /// <summary>
        /// The number of bits per channel. Supported values are 1, 8, 16, and 32.
        /// </summary>
        public int BitDepth
        {
            get { return bitDepth; }
            set
            {
                switch (value)
                {
                    case 1:
                    case 8:
                    case 16:
                    case 32:
                        bitDepth = value;
                        break;
                    default:
                        throw new NotImplementedException("Invalid bit depth.");
                }
            }
        }

        /// <summary>
        /// The color mode of the file.
        /// </summary>
        public PsdColorMode ColorMode { get; set; }

        ///////////////////////////////////////////////////////////////////////////

        private void LoadHeader(PsdBinaryReader reader)
        {
            var signature = reader.ReadAsciiChars(4);
            if (signature != "8BPS")
                throw new PsdInvalidException("The given stream is not a valid PSD file");

            Version = reader.ReadInt16();
            if (Version != 1)
                throw new PsdInvalidException("The PSD file has an unknown version");

            //6 bytes reserved
            reader.BaseStream.Position += 6;

            this.ChannelCount = reader.ReadInt16();
            this.RowCount = reader.ReadInt32();
            this.ColumnCount = reader.ReadInt32();
            BitDepth = reader.ReadInt16();
            ColorMode = (PsdColorMode)reader.ReadInt16();
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////////

        #region ColorModeData

        /// <summary>
        /// If ColorMode is ColorModes.Indexed, the following 768 bytes will contain 
        /// a 256-color palette. If the ColorMode is ColorModes.Duotone, the data 
        /// following presumably consists of screen parameters and other related information. 
        /// Unfortunately, it is intentionally not documented by Adobe, and non-Photoshop 
        /// readers are advised to treat duotone images as gray-scale images.
        /// </summary>
        public byte[] ColorModeData = new byte[0];

        private void LoadColorModeData(PsdBinaryReader reader)
        {
            var paletteLength = reader.ReadUInt32();
            if (paletteLength > 0)
            {
                ColorModeData = reader.ReadBytes((int)paletteLength);
            }
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////////

        #region ImageResources

        /// <summary>
        /// The Image resource blocks for the file
        /// </summary>
        public ImageResources ImageResources { get; set; }
        ///////////////////////////////////////////////////////////////////////////

        private void LoadImageResources(PsdBinaryReader reader)
        {
            var imageResourcesLength = reader.ReadUInt32();
            if (imageResourcesLength <= 0)
                return;

            var startPosition = reader.BaseStream.Position;
            var endPosition = startPosition + imageResourcesLength;
            while (reader.BaseStream.Position < endPosition)
            {
                if (CancelCheck()) return;
                var imageResource = ImageResourceFactory.CreateImageResource(reader);
                ImageResources.Add(imageResource);
            }

            //-----------------------------------------------------------------------
            // make sure we are not on a wrong offset, so set the stream position 
            // manually
            reader.BaseStream.Position = startPosition + imageResourcesLength;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////

        #region LayerAndMaskInfo

        public List<Layer> Layers { get; private set; }
        public List<Layer> RootLayers { get; private set; }
        private Dictionary<int, Layer> _layerDictionary = new Dictionary<int, Layer>();
        public List<LayerInfo> AdditionalInfo { get; private set; }

        public bool AbsoluteAlpha { get; set; }

        ///////////////////////////////////////////////////////////////////////////

        private void LoadLayerAndMaskInfo(PsdBinaryReader reader)
        {
            var layersAndMaskLength = reader.ReadUInt32();
            if (layersAndMaskLength <= 0)
                return;

            var startPosition = reader.BaseStream.Position;
            var endPosition = startPosition + layersAndMaskLength;

            LoadLayers(reader, true);
            LoadGlobalLayerMask(reader);

            //-----------------------------------------------------------------------
            // Load Additional Layer Information

            while (reader.BaseStream.Position < endPosition)
            {
                var info = LayerInfoFactory.Load(reader);
                AdditionalInfo.Add(info);

                if (info is RawLayerInfo)
                {
                    var layerInfo = (RawLayerInfo)info;
                    switch (info.Key)
                    {
                        case "Layr":
                        case "Lr16":
                        case "Lr32":
                            using (var memoryStream = new MemoryStream(layerInfo.Data))
                            using (var memoryReader = new PsdBinaryReader(memoryStream, reader))
                            {
                                LoadLayers(memoryReader, false);
                            }
                            break;

                        case "LMsk":
                            GlobalLayerMaskData = layerInfo.Data;
                            break;
                    }
                }
            }

            //-----------------------------------------------------------------------
            // make sure we are not on a wrong offset, so set the stream position 
            // manually
            reader.BaseStream.Position = startPosition + layersAndMaskLength;
        }

        ///////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load Layers Info section, including image data.
        /// </summary>
        /// <param name="reader">PSD reader.</param>
        /// <param name="hasHeader">Whether the Layers Info section has a length header.</param>
        private void LoadLayers(PsdBinaryReader reader, bool hasHeader)
        {
            _layerDictionary.Clear();

            UInt32 sectionLength = 0;
            if (hasHeader)
            {
                sectionLength = reader.ReadUInt32();
                if (sectionLength <= 0)
                    return;
            }

            var startPosition = reader.BaseStream.Position;
            var numLayers = reader.ReadInt16();

            // If numLayers < 0, then number of layers is absolute value,
            // and the first alpha channel contains the transparency data for
            // the merged result.
            if (numLayers < 0)
            {
                AbsoluteAlpha = true;
                numLayers = Math.Abs(numLayers);
            }
            if (numLayers == 0)
                return;

            for (int i = 0; i < numLayers; i++)
            {
                var layer = new Layer(reader, this);
                Layers.Add(layer);
            }

            //-----------------------------------------------------------------------

            // Load image data for all channels.
            foreach (var layer in Layers)
            {
                if (layer is LayerGroup) continue;
                foreach (var channel in layer.Channels)
                {
                    channel.LoadPixelData(reader);
                }
            }

            // Length is set to 0 when called on higher bitdepth layers.
            if (sectionLength > 0)
            {
                // Layers Info section is documented to be even-padded, but Photoshop
                // actually pads to 4 bytes.
                var endPosition = startPosition + sectionLength;
                var positionOffset = reader.BaseStream.Position - endPosition;

                if (reader.BaseStream.Position < endPosition)
                    reader.BaseStream.Position = endPosition;
            }
            var groupStack = new Stack<LayerGroup>();
            LayerGroup currentGroup = null;

            for (int layerIdx = Layers.Count - 1; layerIdx >= 0; --layerIdx)
            {
                var currentLayer = Layers[layerIdx];

                if (currentLayer.Name == "</Layer group>" || currentLayer.Name == "</Layer set>")
                {
                    groupStack.Pop();
                    currentGroup = groupStack.Count == 0 ? null : groupStack.Peek();
                    continue;
                }

                foreach (var info in currentLayer.AdditionalInfo)
                {
                    if (!(info is LayerSectionInfo)) continue;
                    var sectionInfo = (LayerSectionInfo)info;
                    if (sectionInfo.SectionType != LayerSectionType.OpenFolder && sectionInfo.SectionType != LayerSectionType.ClosedFolder) continue;
                    var previousGroup = currentGroup;
                    currentLayer = currentGroup = new LayerGroup(currentLayer, sectionInfo.SectionType == LayerSectionType.OpenFolder);
                    if (previousGroup == null)
                    {
                        RootLayers.Add(currentLayer);
                    }
                    else
                    {
                        previousGroup.AddChild(currentLayer);
                    }
                    groupStack.Push(currentGroup);
                    break;
                }

                _layerDictionary.Add(currentLayer.Id, currentLayer);

                if (currentGroup == null)
                {
                    RootLayers.Add(currentLayer);
                }
                else if (currentGroup != currentLayer)
                {
                    currentGroup.AddChild(currentLayer);
                }
            }
        }


        public Layer GetLayer(int layerId)
        {
            if (!_layerDictionary.ContainsKey(layerId)) return null;
            return _layerDictionary[layerId];
        }
        ///////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Decompress the document image data and all the layers' image data, in parallel.
        /// </summary>
        private void DecompressImages()
        {
            var imageLayers = Layers.Concat(new List<Layer>() { this.BaseLayer });
            int i = 0;
            foreach (var layer in imageLayers)
            {
                if (CancelCheck()) return;
                if (layer is LayerGroup) continue;
                foreach (var channel in layer.Channels)
                {
                    if (CancelCheck()) return;
                    var dcc = new DecompressChannelContext(channel);
                    dcc.DecompressChannel(null);
                }
                var progress = 0.1f + 0.9f * (float)i / (float)imageLayers.Count();
                OnProgressChanged(progress);
                ++i;
            }
            foreach (var layer in Layers)
            {
                if (CancelCheck()) return;
                if (layer is LayerGroup) continue;
                foreach (var channel in layer.Channels)
                {
                    if (CancelCheck()) return;
                    if (channel.ID == -2)
                        layer.Masks.LayerMask.ImageData = channel.ImageData;
                    else if (channel.ID == -3)
                        layer.Masks.UserMask.ImageData = channel.ImageData;
                }
            }
        }
        
        ///////////////////////////////////////////////////////////////////////////

        byte[] GlobalLayerMaskData = new byte[0];

        private void LoadGlobalLayerMask(PsdBinaryReader reader)
        {
            var maskLength = reader.ReadUInt32();
            if (maskLength <= 0)
                return;

            GlobalLayerMaskData = reader.ReadBytes((int)maskLength);
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////////

        #region ImageData

        ///////////////////////////////////////////////////////////////////////////

        private void LoadImage(PsdBinaryReader reader)
        {
            ImageCompression = (ImageCompression)reader.ReadInt16();

            // Create channels
            for (Int16 i = 0; i < ChannelCount; i++)
            {
                if (CancelCheck()) return;
                var channel = new Channel(i, this.BaseLayer);
                channel.ImageCompression = ImageCompression;
                channel.Length = this.RowCount * Util.BytesPerRow(BaseLayer.Rect, BitDepth);

                // The composite image stores all RLE headers up-front, rather than
                // with each channel.
                if (ImageCompression == ImageCompression.Rle)
                {
                    channel.RleRowLengths = new RleRowLengths(reader, RowCount);
                    channel.Length = channel.RleRowLengths.Total;
                }

                BaseLayer.Channels.Add(channel);
            }

            foreach (var channel in this.BaseLayer.Channels)
            {
                if (CancelCheck()) return;
                channel.ImageDataRaw = reader.ReadBytes(channel.Length);
            }

            // If there is exactly one more channel than we need, then it is the
            // alpha channel.
            if ((ColorMode != PsdColorMode.Multichannel)
              && (ChannelCount == ColorMode.MinChannelCount() + 1))
            {
                var alphaChannel = BaseLayer.Channels.Last();
                alphaChannel.ID = -1;
            }
        }

        ///////////////////////////////////////////////////////////////////////////

        private class DecompressChannelContext
        {
            private Channel ch;

            public DecompressChannelContext(Channel ch)
            {
                this.ch = ch;
            }

            public void DecompressChannel(object context)
            {
                ch.DecodeImageData();
            }
        }

        #endregion
    }


    /// <summary>
    /// The possible Compression methods.
    /// </summary>
    public enum ImageCompression
    {
        /// <summary>
        /// Raw data
        /// </summary>
        Raw = 0,
        /// <summary>
        /// RLE compressed
        /// </summary>
        Rle = 1,
        /// <summary>
        /// ZIP without prediction.
        /// </summary>
        Zip = 2,
        /// <summary>
        /// ZIP with prediction.
        /// </summary>
        ZipPrediction = 3
    }

}