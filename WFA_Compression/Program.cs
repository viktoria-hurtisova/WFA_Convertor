using System;
using WFA_Lib;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace WFA_Convertor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("The program needs input parameters.");
            }
            if (args[0] == "decode")
            {
                var inputWFAFile = args[1];
                if (!File.Exists(inputWFAFile))
                {
                    Console.WriteLine("The input file for loading WFA was not found.");
                    return;
                }

                string path = Path.GetDirectoryName(inputWFAFile) + Path.DirectorySeparatorChar;
                string imageName = path + Path.GetFileNameWithoutExtension(inputWFAFile) + ".png";

                if (File.Exists(imageName))
                {
                    do
                    {
                        Console.WriteLine($"An image {Path.GetFileName(imageName)} already exists in {path}." +
                            $" Do you want to replace it? Replacing it will overwrite its contents. (y/n)");

                        ConsoleKeyInfo key;
                        do
                        {
                            key = Console.ReadKey();
                            Console.WriteLine();
                        } while (key.Key != ConsoleKey.N && key.Key != ConsoleKey.Y);

                        if (key.Key == ConsoleKey.N)
                        {
                            Console.WriteLine("Please enter new file name:");
                            imageName = path + Console.ReadLine().Trim() + ".png";
                        }
                        else
                            break;
                    } while (File.Exists(imageName));
                }

                int depth = 0;

                if (args.Length > 2)
                {
                    string[] newResolution = args[2].Split('=');
                    if (newResolution[0] == "d")
                    {
                        if (!int.TryParse(newResolution[1], out depth))
                        {
                            Console.WriteLine($"You entered depth in wrong format - it cannot be parsed.");
                            return;
                        }
                        if (depth <= 0)
                        {
                            Console.WriteLine($"Depth must be bigger than zero.");
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input for depth.");
                    }

                }

                try
                {
                    var progressBar = new ProgressBar();
                    Console.Write("Decoding...   ");
                    Decoder.WFAToImage(inputWFAFile, imageName, depth, progressBar);
                    progressBar.Report(1);
                    Thread.Sleep(400);
                    Console.WriteLine("Done");

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }
            }
            else if (args[0] == "encode")
            {
                var inputImage = args[1];
                if (!File.Exists(inputImage))
                {
                    Console.WriteLine("The input image was not found.");
                    return;
                }

                string path = Path.GetDirectoryName(inputImage) + Path.DirectorySeparatorChar;
                string wfaName = path + Path.GetFileNameWithoutExtension(inputImage) + ".wfa";
                if (File.Exists(wfaName))
                {
                    do
                    {
                        Console.WriteLine($"The file {Path.GetFileName(wfaName)} already exists in {path}." +
                            $" Do you want to replace it? Replacing it will overwrite its contents. (y/n)");

                        ConsoleKeyInfo key;
                        do
                        {
                            key = Console.ReadKey();
                            Console.WriteLine();
                        } while (key.Key != ConsoleKey.N && key.Key != ConsoleKey.Y);

                        if (key.Key == ConsoleKey.N)
                        {
                            Console.WriteLine("Please enter new file name:");
                            wfaName = path + Console.ReadLine().Trim() + ".wfa";
                        }
                        else
                            break;
                    } while (File.Exists(wfaName));
                }

                var progressBar = new ProgressBar();
                try
                {
                    Console.Write("Encoding...   ");
                    Encoder.ImageToWFA(inputImage, wfaName, progressBar);
                    progressBar.Report(1);
                    Thread.Sleep(400);
                    Console.WriteLine("Done");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }
            }
            else
            {
                Console.WriteLine($"{args[0]} is not a correct command.");
                return;
            }
        }
    }
}
