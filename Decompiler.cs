/*
 
 ~ Luau bytecode decompiler ~

 - Welcome to my decompiler source!
 - This will decompile mediocre bytecode into it's original shape.
 This is purposly released as a learning example, but usage of it
 such as in a decompiler is permitted.
 
 Type:            Console application.
 Initialized sln: No.

 - Towards the bottom of the code, there'll be a "isFunction" method,
 add the functions you'd like to the string array there.

 - That pretty much wraps it up!
 It contains some bad code, but this is not written for neither
 efficiency nor for aestetichs.
 
 ~ Native x64 (_King_)

 */


using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Bytecode_decompiler
{
    class Program
    { 
        static void Main(string[] args)
        {
            Console.WriteLine(args.Length);
            Decompile(File.ReadAllText(args[0]));
        }

        static string strintable_siz = "";
        static void Decompile(string bytecode)
        {
            Console.Title = "Bytecode decompiler by Native x64 (_King_)";
            string inpt = bytecode;
            strintable_siz = inpt.Split(' ')[1];
            string[] hexs = inpt.Split(' ');
            string constr = "";
            
            int lk_r = 0;
            int init = 0;
            int clsr = 0;
            Console.WriteLine("Constant pool size: " + hex2size(strintable_siz));
            List<string> values = new List<string>();
            if (hexs[0] == "01") // If compilation was successfull
            {
                /*
                 * Conversions and checks
                */

                int topselctr = 2;
                int strintable_s = hex2size(hexs[1]);
                int b_s = hex2size(hexs[topselctr]);
                string temp_constr = "";
                int psr = 1;
                bool brkr = false;
                int conv = 0;
                Console.WriteLine("Constant pool\n{");
                for (int i = 0; i < strintable_s; i++)
                {
                    for (int cur = topselctr + psr; cur < topselctr + b_s + 1; cur++)
                    {
                        temp_constr += ConvFHex(hexs[cur]);
                        conv = cur;
                    }
                    if (!brkr)
                    {
                        psr++;
                        brkr = true;
                    }
                    b_s = hex2size(hexs[conv + 1]) +1 ;
                    Console.WriteLine("   [" + (i+1) + "] => " + temp_constr);
                    topselctr = conv++;
                    values.Add(temp_constr);
                    temp_constr = "";
                }
                Console.WriteLine("}\n");

                /*
                 * - Instruction analyser
                 */

                Console.WriteLine("IDX : Instruction   : Bytes                     :   parameters");
                Console.WriteLine("=============================================================");

                for (int i = conv; i < hexs.Length; i++) // iterating through all hexadecimals after the constant table
                {
                    if (hexs[i] == Opcode.GETGLOBAL)
                    {
                        if(isFunction(values[lk_r])) // If function exists, initialize as call, else assign as gettablek parent
                            constr += values[lk_r] + "(";
                        else
                            constr += values[lk_r] + ".";

                        GetDebugLayout(Hex2Instruction(hexs[i]), values[lk_r], (hexs[i] + " " + hexs[i + 1] + " " + hexs[i + 2] + " " + hexs[i + 3] + " " + hexs[i + 4] + " " + hexs[i + 5] + " " + hexs[i + 6] + " " + hexs[i + 7]) , lk_r);
                        lk_r++;
                    }
                    else if(hexs[i] == Opcode.CLEARSTACK)
                    {
                        lk_r = 0; // sets constant table iteration position back to 0
                        GetDebugLayout(Hex2Instruction(hexs[i]), "None", (hexs[i] + " " + hexs[i + 1] + " " + hexs[i + 2] + " " + hexs[i + 3]) , lk_r);
                    }
                    else if (hexs[i] == Opcode.LOADK)
                    {
                        if (lk_r+1 != values.Count) // Load the constant into a call depending on parameter count
                            constr += "\"" + values[lk_r] + "\", ";
                        else
                            constr += "\"" + values[lk_r] + "\"";
                        GetDebugLayout(Hex2Instruction(hexs[i]), values[lk_r], (hexs[i] + " " + hexs[i + 1] + " " + hexs[i + 2] + " " + hexs[i + 3]), lk_r);
                        lk_r++;
                    }
                    else if (hexs[i] == Opcode.CALL)
                    {
                        if(clsr == 0) // If isnt in a closure context, will remove possible gettablek extension and add the actual call to the function
                        {
                            if (constr[constr.Length - 1] == '.')
                                constr = constr.Remove(constr.Length - 1);
                            constr += ")";
                            GetDebugLayout(Hex2Instruction(hexs[i]), "None", (hexs[i] + " " + hexs[i + 1] + " " + hexs[i + 2] + " " + hexs[i + 3]), lk_r);
                        }
                    }
                    else if(hexs[i] == Opcode.GETTABLEK)
                    {
                        constr += values[lk_r] + "."; // Will add the constant table index to the script available for a child
                        GetDebugLayout(Hex2Instruction(hexs[i]), values[lk_r] , (hexs[i] + " " + hexs[i + 1] + " " + hexs[i + 2] + " " + hexs[i + 3]), lk_r);
                        lk_r++;
                    }
                    else if(hexs[i] == Opcode.LOADNUMBER)
                    {
                        constr += hex2size(hexs[i + 2]).ToString(); // Adds a number to the script
                        GetDebugLayout(Hex2Instruction(hexs[i]), hex2size(hexs[i + 2]).ToString(), (hexs[i] + " " + hexs[i + 1] + " " + hexs[i + 2] + " " + hexs[i + 3]), lk_r);
                    }
                    else if(hexs[i] == Opcode.LOADBOOL)
                    {
                        string mConstr = ""; // Will decide if 2nd arg in loadbool is true or not, and push to script
                        if(hexs[i + 2] == "00")
                            mConstr = "false";
                        else if(hexs[i + 2] == "01")
                            mConstr = "true";
                        constr += mConstr;
                        GetDebugLayout(Hex2Instruction(hexs[i]), mConstr, (hexs[i] + " " + hexs[i + 1] + " " + hexs[i + 2] + " " + hexs[i + 3]), lk_r);
                    }
                    else if(hexs[i] == Opcode.LOADNIL)
                    {
                        constr += "nil"; // Loads a nil into the script
                        GetDebugLayout(Hex2Instruction(hexs[i]), "None", (hexs[i] + " " + hexs[i + 1] + " " + hexs[i + 2] + " " + hexs[i + 3]), lk_r);
                    }
                    else if(hexs[i] == Opcode.INIT)
                    {
                        init++; // Adds to init value to tell which context the ctor is in (outside or inside closure)
                        GetDebugLayout(Hex2Instruction(hexs[i]), "None", (hexs[i] + " " + hexs[i + 1] + " " + hexs[i + 2] + " " + hexs[i + 3]), lk_r);
                    }
                    else if(hexs[i] == Opcode.CLOSURE)
                    {
                        if(init > 0) // puts the script in a closure, if init is not 0, it will call the closure
                        {
                            constr = "(function() " + constr + "end)()";
                            GetDebugLayout(Hex2Instruction(hexs[i]), "constructor call", (hexs[i] + " " + hexs[i + 1] + " " + hexs[i + 2] + " " + hexs[i + 3]), lk_r);
                            init--;
                            clsr++;
                        }
                        else
                        {
                            constr = "(function() " + constr + "end)";
                            GetDebugLayout(Hex2Instruction(hexs[i]), "constructor gen", (hexs[i] + " " + hexs[i + 1] + " " + hexs[i + 2] + " " + hexs[i + 3]), lk_r);
                            clsr++;
                        }
                    }
                    else if(hexs[i] == Opcode.RETURN)
                    {
                        clsr = 0; // Checks to 0 for a finishing remark
                        GetDebugLayout(Hex2Instruction(hexs[i]), "None", (hexs[i] + " " + hexs[i + 1] + " " + hexs[i + 2] + " " + hexs[i + 3]), lk_r);
                    }
                }
            }
            Console.WriteLine("\n\nScript:\n====\n");
            Console.WriteLine(constr + "\n\n====");
            Console.WriteLine("Done!");
            Console.ReadLine();
        }

        class Opcode // All opcodes used
        {
            public static string GETGLOBAL = "A4";
            public static string GETTABLEK = "4D";
            public static string LOADK = "6F";
            public static string RETURN = "82";
            public static string LOADNUMBER = "8C";
            public static string CALL = "9F";
            public static string CLEARSTACK = "A3";
            public static string LOADBOOL = "A9";
            public static string LOADNIL = "C6";
            public static string CLOSURE = "D9";
            public static string INIT = "C0";
        };


        /*
         * Conversions and checks
        */

        static string Hex2Instruction(string hex)
        {
            if (hex == "A4")
                return "getglobal";
            else if (hex == "6F")
                return "loadk";
            else if (hex == "A3")
                return "clearstack";
            else if (hex == "9F")
                return "call";
            else if (hex == "4D")
                return "gettablek";
            else if (hex == "8C")
                return "loadnumber";
            else if (hex == "A9")
                return "loadbool";
            else if (hex == "C6")
                return "loadnil";
            else if (hex == "C0")
                return "init";
            else if (hex == "D9")
                return "closure";
            else if (hex == "82")
                return "return";
            return "nil";
        }

        static void GetDebugLayout(string instruction, string argument, string bytes, int lk_r) // debugging purposes
        {
            Console.WriteLine("[" + (lk_r+1) + "] : " + instruction + "              ".Remove(0, instruction.Length) + ": " + bytes + "                          ".Remove(0, bytes.Length) + ":   " + argument);
        }
        static bool isFunction(string func) // function table to check if input is a function
        {
            string[] funcs = { "print", "warn" }; // Add more functions here as you like
            for(int i = 0; i < funcs.Length; i++)
            {
                if(funcs[i] == func)
                {
                    return true;
                }
            }
            return false;
        }
        static int hex2size(string hex) // Reversed process of Writecompressedint
        {
            return int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        }
        static string ConvFHex(string hex) // Reversed process of Writebyte
        {
            StringBuilder stb = new StringBuilder();
            for (int i = 0; i < hex.Length; i += 2)
            {
                stb.Append((char)Convert.ToByte(hex.Substring(i, 2), 16));
            }
            return stb.ToString();
        }
    }
}
