#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/UpscalingTask.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System.IO;
using Pixeval.Attributes;

namespace Pixeval.Upscaling;

[LocalizationMetadata(typeof(AiUpscalerResources))]
public enum RealESRGANModel
{
    [LocalizedResource(typeof(AiUpscalerResources), nameof(AiUpscalerResources.RealESRGANX4Plus))]
    RealESRGANX4Plus,

    [LocalizedResource(typeof(AiUpscalerResources), nameof(AiUpscalerResources.RealESRNETX4Plus))]
    RealESRNETX4Plus,

    [LocalizedResource(typeof(AiUpscalerResources), nameof(AiUpscalerResources.RealESRGANX4PlusAnime))]
    RealESRGANX4PlusAnime
}

[LocalizationMetadata(typeof(AiUpscalerResources))]
public enum UpscalerOutputType
{
    [LocalizedResource(typeof(AiUpscalerResources), nameof(AiUpscalerResources.PngOutputType))]
    Png,

    [LocalizedResource(typeof(AiUpscalerResources), nameof(AiUpscalerResources.JpegOutputType))]
    Jpeg,

    [LocalizedResource(typeof(AiUpscalerResources), nameof(AiUpscalerResources.WebPOutputType))]
    WebP
}

public record UpscaleTask(
    Stream ImageStream, 
    RealESRGANModel Model,
    int ScaleRatio,
    UpscalerOutputType OutputType);
