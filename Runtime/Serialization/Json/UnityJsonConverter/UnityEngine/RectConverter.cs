#region License
// The MIT License (MIT)
//
// Copyright (c) 2020 Wanzyee Studio
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion

using UnityEngine;

namespace Newtonsoft.Json.UnityConverters
{
    /// <summary>
    /// Custom Newtonsoft.Json converter <see cref="JsonConverter"/> for the Unity Vector3 type <see cref="Vector3"/>.
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public class RectConverter : AutoPartialConverter<Rect>
    {
        protected override void ReadValue(ref Rect value, string name, JsonReader reader, JsonSerializer serializer)
        {
            switch (name)
            {
                case nameof(value.x):
                    value.x = (float?)reader.ReadAsDouble() ?? 0f;
                    break;
                case nameof(value.y):
                    value.y = (float?)reader.ReadAsDouble() ?? 0f;
                    break;
                case nameof(value.width):
                    value.width = (float?)reader.ReadAsDouble() ?? 0f;
                    break;
                case nameof(value.height):
                    value.height = (float?)reader.ReadAsDouble() ?? 0f;
                    break;
            }
        }

        protected override void WriteJsonProperties(JsonWriter writer, Rect value, JsonSerializer serializer)
        {
            writer.WritePropertyName(nameof(value.x));
            writer.WriteValue(value.x);
            writer.WritePropertyName(nameof(value.y));
            writer.WriteValue(value.y);
            writer.WritePropertyName(nameof(value.width));
            writer.WriteValue(value.width);
            writer.WritePropertyName(nameof(value.height));
            writer.WriteValue(value.height);
        }
    }
}
