# Texture-Channel-Packer-Unity
 Unity tool for packing multiple grayscale textures into a single RGBA output texture

## Overview
The Unity Texture Combiner is an advanced editor tool that allows you to pack multiple texture channels into a single texture. This is particularly useful for optimizing materials by combining different texture maps (like metallic, smoothness, occlusion, etc.) into a single texture's RGBA channels.

## Features
- Channel Packing: Combine up to 4 textures into one

- Flexible Channel Mapping: Map any source channel to any output channel

- Preset System: Save and load frequently used configurations

- Customizable Output: Control texture size, format, and import settings

- Real-time Preview: See results before exporting

- Texture Resizing: Automatic resizing of input textures

## Installation
1. Clone or download this repository

2. Copy the Editor/TextureCombiner folder into your Unity project's Assets folder

3. The tool will be available under Window > Texture Combiner

## How to Use
### Basic Usage
Open the Texture Combiner window (Window > Texture Combiner)

Assign textures to the available slots

For each texture, select:

Which output channel (R, G, B, or A) to pack into

Which source channel (R, G, B, A, grayscale, or average) to use

Set the output path (default: Assets/CombinedTexture.png)

Click "Combine Textures" to generate the packed texture

### Presets
To save a configuration:

Set up your textures and channel mappings

Enter a name in the "Preset Name" field

Click "Save Preset"

To load a preset:

Select a preset from the dropdown menu

The texture assignments and channel mappings will be restored

### Settings
Texture Combiner settings can be accessed and modified through the included ScriptableObject asset at:
Assets/Editor/TextureCombiner/TextureCombinerSettings.asset

Key settings include:

Default output texture size

Whether to use largest input texture size

Mipmap generation

Color space (sRGB or Linear)

Texture import type

Filter and wrap modes

Script Overview
The tool consists of four main scripts:

TextureCombinerWindow.cs - Main editor window and functionality

TextureCombinerPresets.cs - Preset saving/loading system

TextureCombinerSettings.cs - Persistent configuration settings

TextureCombinerUtilities.cs - Helper functions

Examples
Common Use Cases
PBR Material Optimization:

Red: Metallic

Green: Smoothness

Blue: Occlusion

Alpha: Height/Dispalcement

Terrain Splat Maps:

Combine multiple grayscale masks into a single RGBA texture

Character Textures:

Pack subsurface scattering, specular, and other masks into one texture

Requirements
Unity 2019.4 or later

No additional packages required

License
This project is licensed under the MIT License - see the LICENSE.md file for details.
