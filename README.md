# WFA Convertor

## About

This project is only as a "Proof of concept" for the Weighted Finite Automata (WFA) Compression. This project only implements conversion of pictures to WFA and back. It is not very fast, so please do not use pictures bigger than 256x256 pixels.

## Repository Contents

- WFA_Lib - C# library for data structures and algorithms
- WFA_Convertor - C# application WFA_Convertor
- WFA_Convertor.sln - Visual Studio solution file

## Installation

Run the installation package to install WFA\_Convertor on your computer. Follow the installer's instructions. Run the WFA\_COnvertor.exe to launch WFA\_Convertor after the installation is finished.

## Usage

After the application is installed and started the user has to enter the parameters for encoding and decoding.

The program has two modes - encoding and decoding. The first is for encoding images into WFA, the letter is for generating images from WFA.

### Encoding an image

The encoding algorithm will take an image given by the user and convert it into WFA.

#### Input parameters

The parameters for encoding an image must bein the following format:

```
encode <image> 
```

where

- the keyword `encode` indicates that you have chosen the encoding mode,
- `<image>` is a path to the image file. The path can be either absolute or relative to the application's location in the file system. The program supports images in the following formats: BMP, JPG, PNG, TIFF. If the image is in a different format, the program will either not be able to process it and will write out an error message, or the resulting WFA may not represent the image.

For example, a correct input for encoding is

```
encode C:\Users\MyName\Desktop\pictures\garden.png
```

Because the algorithm builds a WFA that represents the image exactly, the time for encoding can take significant time (even tens of minutes, depending on the image size and complexity). That is why we advise you to encode only images with a size at most $512\times512$ px.

#### Output

The resulting automaton will be saved in the same directory as the input image, with the same name but with the suffix `.wfa`.
If there already is a WFA file with the same name, the program will ask you, if you want to overwrite the existing file. If not, the program will ask you to enter a new name of the file. Please, just write the name without the directory path or suffix.

### Decoding image

The decoding algorithm generates an image from an input WFA.

#### Input parameters

The parameters for decoding an image must bein the following format:

```
decode <WFAFile> [depth]
```

where

- keyword `decode` indicates, that you have chosen the decoding mode,
- `<WFAFile>` is a path to the WFA file. The path can be either absolute or relative to the application's location in the file system,
- `[depth]` is an optional parameter. The parameter indicates how deep into the WFA the algorithm goes. If the parameter is not supplied, the decoder uses the native resolution of the encoded image.
	
	The parameter must be in the format `d=<value>`. The `<value>` must be a positive integer number determining the chosen depth. If the value has a the wrong format or is negative, the program will write an error message.

For example, a correct input for decoding is
 ```
 decode C:\Users\MyName\Desktop\pictures\garden.wfa d=8
 ```

#### Output

The output is a decoded PNG image. It will be saved in the same directory as the input WFA file with the same name as the WFA. If there already is an image with the same name, the program will ask you, if you want to overwrite the existing image. If not, the program will ask you to enter a new name of the image. Please, just write the name without the directory path or suffix.
