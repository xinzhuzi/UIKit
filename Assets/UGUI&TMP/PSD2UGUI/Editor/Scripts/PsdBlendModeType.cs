/*
Copyright (c) 2020 Omar Duarte
Unauthorized copying of this file, via any medium is strictly prohibited.
Writen by Omar Duarte, 2020.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PSD2UGUI
{
    public enum BlendingShaderType { DEFAULT, FAST, GRAB_PASS }

    
    [DebuggerDisplay("{Name}")]
    [System.Serializable]
    public sealed class PsdBlendModeType
    {
        public enum BlendModeType
        {
            PASS_THROUGH,
            NORMAL,
            DISSOLVE,
            DARKEN,
            MULTIPLY,
            COLOR_BURN,
            LINEAR_BURN,
            DARKER_COLOR,
            LIGHTEN,
            SCREEN,
            COLOR_DODGE,
            LINEAR_DODGE,
            LIGHTER_COLOR,
            OVERLAY,
            SOFT_LIGHT,
            HARD_LIGHT,
            VIVID_LIGHT,
            LINEAR_LIGHT,
            PIN_LIGHT,
            HARD_MIX,
            DIFFERENCE,
            EXCLUSION,
            SUBTRACT,
            DIVIDE
#if UNITY_WEBGL
#else
            ,HUE,
            SATURATION,
            COLOR,
            LUMINOSITY
#endif
        }

        public enum FastBlendModeType
        {
            PASS_THROUGH,
            NORMAL,
            DISSOLVE,
            DARKEN,
            MULTIPLY,
            COLOR_BURN,
            LINEAR_BURN,
            
            LIGHTEN,
            SCREEN,
            COLOR_DODGE,
            LINEAR_DODGE,
            
            OVERLAY,
            SOFT_LIGHT,
            HARD_LIGHT,
            VIVID_LIGHT,
            LINEAR_LIGHT,
            
            EXCLUSION,
            SUBTRACT,
            DIVIDE,
        }

        public static FastBlendModeType GetFastBlendModeType(BlendModeType value)
        {
            switch (value)
            {
                case BlendModeType.DISSOLVE: return FastBlendModeType.DISSOLVE;
                case BlendModeType.DARKEN: return FastBlendModeType.DARKEN;
                case BlendModeType.MULTIPLY: return FastBlendModeType.MULTIPLY;
                case BlendModeType.COLOR_BURN: return FastBlendModeType.COLOR_BURN;
                case BlendModeType.LINEAR_BURN: return FastBlendModeType.LINEAR_BURN;
                case BlendModeType.LIGHTEN: return FastBlendModeType.LIGHTEN;
                case BlendModeType.SCREEN: return FastBlendModeType.SCREEN;
                case BlendModeType.COLOR_DODGE: return FastBlendModeType.COLOR_DODGE;
                case BlendModeType.LINEAR_DODGE: return FastBlendModeType.LINEAR_DODGE;
                case BlendModeType.OVERLAY: return FastBlendModeType.OVERLAY;
                case BlendModeType.SOFT_LIGHT: return FastBlendModeType.SOFT_LIGHT;
                case BlendModeType.HARD_LIGHT: return FastBlendModeType.HARD_LIGHT;
                case BlendModeType.VIVID_LIGHT: return FastBlendModeType.VIVID_LIGHT;
                case BlendModeType.LINEAR_LIGHT: return FastBlendModeType.LINEAR_LIGHT;
                case BlendModeType.EXCLUSION: return FastBlendModeType.EXCLUSION;
                case BlendModeType.SUBTRACT: return FastBlendModeType.SUBTRACT;
                case BlendModeType.DIVIDE: return FastBlendModeType.DIVIDE;
                case BlendModeType.PASS_THROUGH: return FastBlendModeType.PASS_THROUGH;
                default: return FastBlendModeType.NORMAL;
            }
        }

        public static BlendModeType GetGroupBlendModeType(FastBlendModeType value)
        {
            switch (value)
            {
                case FastBlendModeType.DISSOLVE: return BlendModeType.DISSOLVE;
                case FastBlendModeType.DARKEN: return BlendModeType.DARKEN;
                case FastBlendModeType.MULTIPLY: return BlendModeType.MULTIPLY;
                case FastBlendModeType.COLOR_BURN: return BlendModeType.COLOR_BURN;
                case FastBlendModeType.LINEAR_BURN: return BlendModeType.LINEAR_BURN;
                case FastBlendModeType.LIGHTEN: return BlendModeType.LIGHTEN;
                case FastBlendModeType.SCREEN: return BlendModeType.SCREEN;
                case FastBlendModeType.COLOR_DODGE: return BlendModeType.COLOR_DODGE;
                case FastBlendModeType.LINEAR_DODGE: return BlendModeType.LINEAR_DODGE;
                case FastBlendModeType.OVERLAY: return BlendModeType.OVERLAY;
                case FastBlendModeType.SOFT_LIGHT: return BlendModeType.SOFT_LIGHT;
                case FastBlendModeType.HARD_LIGHT: return BlendModeType.HARD_LIGHT;
                case FastBlendModeType.VIVID_LIGHT: return BlendModeType.VIVID_LIGHT;
                case FastBlendModeType.LINEAR_LIGHT: return BlendModeType.LINEAR_LIGHT;
                case FastBlendModeType.EXCLUSION: return BlendModeType.EXCLUSION;
                case FastBlendModeType.SUBTRACT: return BlendModeType.SUBTRACT;
                case FastBlendModeType.DIVIDE: return BlendModeType.DIVIDE;
                case FastBlendModeType.PASS_THROUGH: return BlendModeType.PASS_THROUGH;
                default: return BlendModeType.NORMAL;
            }
        }

        public static readonly PsdBlendModeType NORMAL = new PsdBlendModeType((int)BlendModeType.NORMAL, "norm", "Normal");
        public static readonly PsdBlendModeType DISSOLVE = new PsdBlendModeType((int)BlendModeType.DISSOLVE, "diss", "Dissolve");

        public static readonly PsdBlendModeType DARKEN = new PsdBlendModeType((int)BlendModeType.DARKEN, "dark", "Darken");
        public static readonly PsdBlendModeType MULTIPLY = new PsdBlendModeType((int)BlendModeType.MULTIPLY, "mul ", "Multiply");
        public static readonly PsdBlendModeType COLOR_BURN = new PsdBlendModeType((int)BlendModeType.COLOR_BURN, "idiv", "Color Burn");
        public static readonly PsdBlendModeType LINEAR_BURN = new PsdBlendModeType((int)BlendModeType.LINEAR_BURN, "lbrn", "Linear Burn");
        public static readonly PsdBlendModeType DARKER_COLOR = new PsdBlendModeType((int)BlendModeType.DARKER_COLOR, "dkCl", "Darker Color", true);

        public static readonly PsdBlendModeType LIGHTEN = new PsdBlendModeType((int)BlendModeType.LIGHTEN, "lite", "Lighten");
        public static readonly PsdBlendModeType SCREEN = new PsdBlendModeType((int)BlendModeType.SCREEN, "scrn", "Screen");
        public static readonly PsdBlendModeType COLOR_DODGE = new PsdBlendModeType((int)BlendModeType.COLOR_DODGE, "div ", "Color Dodge");
        public static readonly PsdBlendModeType LINEAR_DODGE = new PsdBlendModeType((int)BlendModeType.LINEAR_DODGE, "lddg", "Linear Dodge");
        public static readonly PsdBlendModeType LIGHTER_COLOR = new PsdBlendModeType((int)BlendModeType.LIGHTER_COLOR, "lgCl", "Lighter Color", true);

        public static readonly PsdBlendModeType OVERLAY = new PsdBlendModeType((int)BlendModeType.OVERLAY, "over", "Overlay");
        public static readonly PsdBlendModeType SOFT_LIGHT = new PsdBlendModeType((int)BlendModeType.SOFT_LIGHT, "sLit", "Soft Light");
        public static readonly PsdBlendModeType HARD_LIGHT = new PsdBlendModeType((int)BlendModeType.HARD_LIGHT, "hLit", "Hard Light");
        public static readonly PsdBlendModeType VIVID_LIGHT = new PsdBlendModeType((int)BlendModeType.VIVID_LIGHT, "vLit", "Vivid Light");
        public static readonly PsdBlendModeType LINEAR_LIGHT = new PsdBlendModeType((int)BlendModeType.LINEAR_LIGHT, "lLit", "Linear Light");
        public static readonly PsdBlendModeType PIN_LIGHT = new PsdBlendModeType((int)BlendModeType.PIN_LIGHT, "pLit", "Pin Light", true);
        public static readonly PsdBlendModeType HARD_MIX = new PsdBlendModeType((int)BlendModeType.HARD_MIX, "hMix", "Hard Mix", true);

        public static readonly PsdBlendModeType DIFFERENCE = new PsdBlendModeType((int)BlendModeType.DIFFERENCE, "diff", "Difference", true);
        public static readonly PsdBlendModeType EXCLUSION = new PsdBlendModeType((int)BlendModeType.EXCLUSION, "smud", "Exclusion");
        public static readonly PsdBlendModeType SUBTRACT = new PsdBlendModeType((int)BlendModeType.SUBTRACT, "fsub", "Subtract");
        public static readonly PsdBlendModeType DIVIDE = new PsdBlendModeType((int)BlendModeType.DIVIDE, "fdiv", "Divide");

#if UNITY_WEBGL
#else
        public static readonly PsdBlendModeType HUE = new PsdBlendModeType((int)BlendModeType.HUE, "hue ", "Hue", true);
        public static readonly PsdBlendModeType SATURATION = new PsdBlendModeType((int)BlendModeType.SATURATION, "sat ", "Saturation", true);
        public static readonly PsdBlendModeType COLOR = new PsdBlendModeType((int)BlendModeType.COLOR, "colr", "Color", true);
        public static readonly PsdBlendModeType LUMINOSITY = new PsdBlendModeType((int)BlendModeType.LUMINOSITY, "lum ", "Luminosity", true);
#endif
        public static readonly PsdBlendModeType PASS_THROUGH = new PsdBlendModeType((int)BlendModeType.PASS_THROUGH, "pass", "Pass Through");

        public readonly int Id = -1;
        public readonly string Key = null;
        public readonly string Name = null;
        public readonly bool GrabPass = false;

        public static readonly string[] FastBlendModeNames = new string[]
        {
            PASS_THROUGH.Name,
            NORMAL.Name,
            DISSOLVE.Name,
            DARKEN.Name,
            MULTIPLY.Name,
            COLOR_BURN.Name,
            LINEAR_BURN.Name,
            LIGHTEN.Name,
            SCREEN.Name,
            COLOR_DODGE.Name,
            LINEAR_DODGE.Name,
            OVERLAY.Name,
            SOFT_LIGHT.Name,
            HARD_LIGHT.Name,
            VIVID_LIGHT.Name,
            LINEAR_LIGHT.Name,
            EXCLUSION.Name,
            SUBTRACT.Name,
            DIVIDE.Name
        };

        public static readonly string[] GrabPassBlendModeNames = new string[] 
        {
            PASS_THROUGH.Name,
            NORMAL.Name,
            DISSOLVE.Name,
            DARKEN.Name,
            MULTIPLY.Name,
            COLOR_BURN.Name,
            LINEAR_BURN.Name,
            DARKER_COLOR.Name,
            LIGHTEN.Name,
            SCREEN.Name,
            COLOR_DODGE.Name,
            LINEAR_DODGE.Name,
            LIGHTER_COLOR.Name,
            OVERLAY.Name,
            SOFT_LIGHT.Name,
            HARD_LIGHT.Name,
            VIVID_LIGHT.Name,
            LINEAR_LIGHT.Name,
            PIN_LIGHT.Name,
            HARD_MIX.Name,
            DIFFERENCE.Name,
            EXCLUSION.Name,
            SUBTRACT.Name,
            DIVIDE.Name
#if UNITY_WEBGL
#else
            ,HUE.Name,
            SATURATION.Name,
            COLOR.Name,
            LUMINOSITY.Name
#endif
        };

        public static readonly string[] GroupBlendModeNames = new string[]
        {
            PASS_THROUGH.Name,
            NORMAL.Name,
            DISSOLVE.Name,
            DARKEN.Name,
            MULTIPLY.Name,
            COLOR_BURN.Name,
            LINEAR_BURN.Name,
            DARKER_COLOR.Name,
            LIGHTEN.Name,
            SCREEN.Name,
            COLOR_DODGE.Name,
            LINEAR_DODGE.Name,
            LIGHTER_COLOR.Name,
            OVERLAY.Name,
            SOFT_LIGHT.Name,
            HARD_LIGHT.Name,
            VIVID_LIGHT.Name,
            LINEAR_LIGHT.Name,
            PIN_LIGHT.Name,
            HARD_MIX.Name,
            DIFFERENCE.Name,
            EXCLUSION.Name,
            SUBTRACT.Name,
            DIVIDE.Name
#if UNITY_WEBGL
#else
            ,HUE.Name,
            SATURATION.Name,
            COLOR.Name,
            LUMINOSITY.Name
#endif
        };

        private PsdBlendModeType() { }
        private PsdBlendModeType(int id, string key, string name, bool grabPass = false)
        {
            Id = id;
            Key = key;
            Name = name;
            GrabPass = grabPass;
        }

        public static implicit operator string(PsdBlendModeType value)
        {
            return value.Key;
        }

        public static implicit operator int(PsdBlendModeType value)
        {
            return value.Id;
        }

        public static implicit operator BlendModeType(PsdBlendModeType value)
        {
            return (BlendModeType)value.Id;
        }

        public static implicit operator PsdBlendModeType(string key)
        {
            if (key == NORMAL.Key) return NORMAL;
            else if (key == DISSOLVE.Key) return DISSOLVE;
            else if (key == DARKEN.Key) return DARKEN;
            else if (key == MULTIPLY.Key) return MULTIPLY;
            else if (key == COLOR_BURN.Key) return COLOR_BURN;
            else if (key == LINEAR_BURN.Key) return LINEAR_BURN;
            else if (key == DARKER_COLOR.Key) return DARKER_COLOR;
            else if (key == LIGHTEN.Key) return LIGHTEN;
            else if (key == SCREEN.Key) return SCREEN;
            else if (key == COLOR_DODGE.Key) return COLOR_DODGE;
            else if (key == LINEAR_DODGE.Key) return LINEAR_DODGE;
            else if (key == LIGHTER_COLOR.Key) return LIGHTER_COLOR;
            else if (key == OVERLAY.Key) return OVERLAY;
            else if (key == SOFT_LIGHT.Key) return SOFT_LIGHT;
            else if (key == HARD_LIGHT.Key) return HARD_LIGHT;
            else if (key == VIVID_LIGHT.Key) return VIVID_LIGHT;
            else if (key == LINEAR_LIGHT.Key) return LINEAR_LIGHT;
            else if (key == PIN_LIGHT.Key) return PIN_LIGHT;
            else if (key == HARD_MIX.Key) return HARD_MIX;
            else if (key == DIFFERENCE.Key) return DIFFERENCE;
            else if (key == EXCLUSION.Key) return EXCLUSION;
            else if (key == SUBTRACT.Key) return SUBTRACT;
            else if (key == DIVIDE.Key) return DIVIDE;
#if UNITY_WEBGL
#else
            else if (key == HUE.Key) return HUE;
            else if (key == SATURATION.Key) return SATURATION;
            else if (key == COLOR.Key) return COLOR;
            else if (key == LUMINOSITY.Key) return LUMINOSITY;
#endif
            else if (key == PASS_THROUGH.Key) return PASS_THROUGH;
            else return NORMAL;
        }

        public static implicit operator PsdBlendModeType(int id)
        {
            if (id == NORMAL.Id) return NORMAL;
            else if (id == DISSOLVE.Id) return DISSOLVE;
            else if (id == DARKEN.Id) return DARKEN;
            else if (id == MULTIPLY.Id) return MULTIPLY;
            else if (id == COLOR_BURN.Id) return COLOR_BURN;
            else if (id == LINEAR_BURN.Id) return LINEAR_BURN;
            else if (id == DARKER_COLOR.Id) return DARKER_COLOR;
            else if (id == LIGHTEN.Id) return LIGHTEN;
            else if (id == SCREEN.Id) return SCREEN;
            else if (id == COLOR_DODGE.Id) return COLOR_DODGE;
            else if (id == LINEAR_DODGE.Id) return LINEAR_DODGE;
            else if (id == LIGHTER_COLOR.Id) return LIGHTER_COLOR;
            else if (id == OVERLAY.Id) return OVERLAY;
            else if (id == SOFT_LIGHT.Id) return SOFT_LIGHT;
            else if (id == HARD_LIGHT.Id) return HARD_LIGHT;
            else if (id == VIVID_LIGHT.Id) return VIVID_LIGHT;
            else if (id == LINEAR_LIGHT.Id) return LINEAR_LIGHT;
            else if (id == PIN_LIGHT.Id) return PIN_LIGHT;
            else if (id == HARD_MIX.Id) return HARD_MIX;
            else if (id == DIFFERENCE.Id) return DIFFERENCE;
            else if (id == EXCLUSION.Id) return EXCLUSION;
            else if (id == SUBTRACT.Id) return SUBTRACT;
            else if (id == DIVIDE.Id) return DIVIDE;
#if UNITY_WEBGL
#else
            else if (id == HUE.Id) return HUE;
            else if (id == SATURATION.Id) return SATURATION;
            else if (id == COLOR.Id) return COLOR;
            else if (id == LUMINOSITY.Id) return LUMINOSITY;
#endif
            else if (id == PASS_THROUGH.Id) return PASS_THROUGH;
            else return NORMAL;
        }

        public static implicit operator PsdBlendModeType(BlendModeType id)
        {
            return (PsdBlendModeType)(int)id;
        }

        public static bool IsSimple(BlendModeType mode)
        {
            return mode == PASS_THROUGH
            || mode == NORMAL
            || mode == DISSOLVE
            || mode == DARKEN
            || mode == MULTIPLY
            || mode == COLOR_BURN
            || mode == LINEAR_BURN
            || mode == LIGHTEN
            || mode == SCREEN
            || mode == COLOR_DODGE
            || mode == LINEAR_DODGE
            || mode == LINEAR_LIGHT
            || mode == SUBTRACT
            || mode == DIVIDE;
        }
    }
}
