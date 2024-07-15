# WFA Converter

## About

This project is only a "Proof of Concept" for my bachelor thesis **Representing Images by Weighted Finite Automata**. This project only implements the conversion of pictures to WFA and back. It represents the images exactly, therefore, please do not use big images.

## Repository Contents

- `WFA_Lib` - C# library for data structures and algorithms
- `WFA_Convertor` - C# application WFA_Convertor
- `WFA_Convertor.sln` - Visual Studio solution file

## Dependencies

The main library is using the **Extreme Optimization Numerical Libraries** library. To use this software, you may purchase a license online from [here](https://www.extremeoptimization.com/howtobuy). To obtain a trial key, go [here](https://www.extremeoptimization.com/trialkey) and follow the instructions there.

## Installation

The installation package is no longer updated with the new commits. The application works, but terribly.
Run the installation package to install `WFA_Convertor` on your computer. Follow the installer's instructions. Run the `WFA_Convertor.exe` to launch `WFA_Convertor` after the installation is finished.

## Usage

After the application is installed and started the user has to enter the parameters for encoding and decoding.

The program has two modes - encoding and decoding. The first is for encoding images into WFA, the letter is for generating images from WFA.

### Encoding an image

The encoding algorithm will take an image given by the user and convert it into WFA.

#### Input parameters

The parameters for encoding an image must be in the following format:

```bash
encode <image> 
```

where

- the keyword `encode` indicates that you have chosen the encoding mode,
- `<image>` is a path to the image file. The path can be either absolute or relative to the application's location in the file system. The program supports images in the following formats: BMP, JPG, PNG, TIFF. If the image is in a different format, the program will either not be able to process it and will write out an error message, or the resulting WFA may not represent the image.

For example, the correct input for encoding is

```bash
encode C:\Users\MyName\Desktop\pictures\garden.png
```

Because the algorithm builds a WFA that represents the image exactly, the time for encoding can take significant time (even tens of minutes, depending on the image size and complexity). That is why we advise you to encode only images with a size at most $512\times512$ px.

#### Output

The resulting automaton will be saved in the same directory as the input image, with the same name but with the suffix `.wfa`.
If there is a WFA file with the same name, the program will ask you, if you want to overwrite the existing file. If not, the program will ask you to enter a new name of the file. Please, write the name without the directory path or suffix.

### Decoding image

The decoding algorithm generates an image from an input WFA.

#### Input parameters

The parameters for decoding an image must be in the following format:

```bash
decode <WFAFile> [depth]
```

where

- The keyword `decode` indicates, that you have chosen the decoding mode,
- `<WFAFile>` is a path to the WFA file. The path can be either absolute or relative to the application's location in the file system,
- `[depth]` is an optional parameter. The parameter indicates how deep into the WFA the algorithm goes. If the parameter is not supplied, the decoder uses the native resolution of the encoded image.

The parameter must be in the format `d=<value>`. The `<value>` must be a positive integer number determining the chosen depth. If the value has a wrong format or is negative, the program will write an error message.

For example, the correct input for decoding is

 ```bash
 decode C:\Users\MyName\Desktop\pictures\garden.wfa d=8
 ```

#### Output

The output is a decoded PNG image. It will be saved in the same directory as the input WFA file with the same name as the WFA. If there already is an image with the same name, the program will ask you, if you want to overwrite the existing image. If not, the program will ask you to enter a new name for the image. Please, write the name without the directory path or suffix.
