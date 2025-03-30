# Texture-Channel-Packer-Unity
 Unity tool for packing multiple grayscale textures into a single RGBA output texture

## Overview
The Unity Texture Combiner is an advanced editor tool that allows you to pack multiple texture channels into a single texture. This is particularly useful for optimizing materials by combining different texture maps (like metallic, smoothness, occlusion, etc.) into a single texture's RGBA channels.

## Features
**Channel Packing**: Combine up to 4 textures into one

**Flexible Channel Mapping**: Map any source channel to any output channel

**Preset System**: Save and load frequently used configurations

**Customizable Output**: Control texture size, format, and import settings

**Real-time Preview**: See results before exporting

**Texture Resizing**: Automatic resizing of input textures

## Installation
Clone or download this repository

Copy the ``Editor/TextureCombiner`` folder into your Unity project's ``Assets`` folder

The tool will be available under `` Window > Texture Combiner``

## How to Use
### Basic Usage
1. Open the Texture Combiner window (``Window > Texture Combiner``)

2. Assign textures to the available slots

3. For each texture, select:

   - Which output channel (R, G, B, or A) to pack into

   - Which source channel (R, G, B, A, grayscale, or average) to use

5. Set the output path (default: ``Assets/CombinedTexture.png``)

6. Click "Combine Textures" to generate the packed texture

### Presets
1. To save a configuration:

   - Set up your textures and channel mappings

   - Enter a name in the "Preset Name" field

   - Click "Save Preset"

2. To load a preset:

   - Select a preset from the dropdown menu

   - The texture assignments and channel mappings will be restored

### Settings
Texture Combiner settings can be accessed and modified through the included ScriptableObject asset at:
```Assets/Editor/TextureCombiner/TextureCombinerSettings.asset```

Key settings include:

   - Default output texture size

   - Whether to use largest input texture size

   - Mipmap generation

   - Color space (sRGB or Linear)

   - Texture import type

   - Filter and wrap modes

## Script Overview
The tool consists of four main scripts:

1. **TextureCombinerWindow.cs** - Main editor window and functionality

2. **TextureCombinerPresets.cs** - Preset saving/loading system

3. **TextureCombinerSettings.cs** - Persistent configuration settings

4. **TextureCombinerUtilities.cs** - Helper functions

## Examples
### Common Use Cases
1. PBR Material Optimization:

   - Red: Metallic

   - Green: Smoothness

   - Blue: Occlusion

   - Alpha: Height/Dispalcement

2. Terrain Splat Maps:

   - Combine multiple grayscale masks into a single RGBA texture

3. Character Textures:

   - Pack subsurface scattering, specular, and other masks into one texture

## Requirements
   - Unity 2019.4 or later

   - No additional packages required

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
