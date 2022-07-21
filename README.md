# WFA Convertor

## Repository Contents

- WFA_Lib - C# library for data structures and algorithms
- WFA_Convertor - C# application for WFA Convertor
- WFA_Convertor.sln - Visual Studio solution file

## Installation

Run the installation package TODO included in the attachment TODO directory to install WFA\_Convertor on your computer. Follow the installer's instructions. Run the WFA\_COnvertor.exe to launch WFA\_Convertor after the installation is finished.

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
decode <WFAFile> [newSize]
```

where

- keyword `decode` indicates, that you have chosen the decoding mode,
- `<WFAFile>` is a path to the WFA file. The path can be either absolute or relative to the application's location in the file system,
- `[newSize]` is an optional parameter. The parameter indicates the width or height of the generated image in pixels. If the parameter is not supplied, the decoder uses the native resolution of the encoded image.
	The parameter must be in the following format:
	
	`w=<value>`  for specifying the width of the decoded image or
	
	`h=<value>`  for specifying the new height.

    You cannot specify both, the width and height, at the same time, i. e. the width-height ratio cannot be changed. If both parameters are supplied, the decoder uses only the first parameter entered. The `<value>` of new width or height must be a positive integer number determining the chosen dimension in pixels. If the value has a the wrong format or is negative, the program will write an error message.

For example, a correct input for decoding is
 ```
 decode C:\Users\MyName\Desktop\pictures\garden.wfa w=512
 ```

#### Output

The output is a decoded PNG image. It will be saved in the same directory as the input WFA file with the same name as the WFA. If there already is an image with the same name, the program will ask you, if you want to overwrite the existing image. If not, the program will ask you to enter a new name of the image. Please, just write the name without the directory path or suffix.
