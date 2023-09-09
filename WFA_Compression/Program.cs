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

        static bool TryGetResultFilePath(string input, string ResultFileSuffix, out string resultFilePath)
        {
            resultFilePath = null;
            if (!File.Exists(input))
            {
                Console.WriteLine("The input file was not found.");
                return false;
            }
            if (args[0] == "decode")
            {
                var inputWFAFile = args[1];
                if (!File.Exists(inputWFAFile))
                {
                    Console.WriteLine("The input file for loading WFA was not found.");
                    return;
                }

            string path = Path.GetDirectoryName(input) + Path.DirectorySeparatorChar;
            resultFilePath = path + Path.GetFileNameWithoutExtension(input) + ResultFileSuffix;

            if (File.Exists(resultFilePath))
                {
                    do
                    {
                    Console.WriteLine($"The file {Path.GetFileName(resultFilePath)} already exists in {path}." +
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
                        resultFilePath = path + Console.ReadLine().Trim() + ".png";
                        }
                        else
                            break;
                } while (File.Exists(resultFilePath));
            }

            return true;
                }

        static bool TryGetAdditionalParametrs(string[] args, out int depth)
        {
            depth = 0;

                if (args.Length > 2)
                {
                    string[] newResolution = args[2].Split('=');
                    if (newResolution[0] == "d")
                    {
                        if (!int.TryParse(newResolution[1], out depth))
                        {
                            Console.WriteLine($"You entered depth in wrong format - it cannot be parsed.");
                        return false;
                        }
                        if (depth <= 0)
                        {
                            Console.WriteLine($"Depth must be bigger than zero.");
                        return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input for depth.");
                    }

                }

            return true;
        }


        static void Main(string[] args)
        {
                try
            if (args.Length == 0)
                {
                Console.WriteLine("The program needs input parameters.");
            }

            string action = args[0];
            string input = args[1];



            if (action == "decode")
            {
                if (!TryGetResultFilePath(input, ".png", out string resultFileName))
                    return;

                if (!TryGetAdditionalParametrs(args, out int depth))
                    return;

                    var progressBar = new ProgressBar();

                try
                {
                    Console.WriteLine("Decoding...");
                    Decoder.WFAToImage(input, resultFileName, depth, progressBar);
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
            else if (action == "encode")
            {
                if (!TryGetResultFilePath(input, ".wfa", out string resultFileName))
                    return;
                }

                var progressBar = new ProgressBar();

                var progressBar = new ProgressBar();
                try
                {
                    Console.WriteLine("Encoding...");
                    Encoder.ImageToWFA(input, resultFileName, progressBar);
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
                Console.WriteLine($"{action} is not a correct command.");
                return;
            }
        }
    }
}
