using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace fptTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;
            string ext = Path.GetExtension(args[0]);
            FPT fpt = new FPT();
            if (ext == ".fpt")
            {
                fpt.Extract(args[0]);
                return;
            }
            else if (ext == ".txt")
            {
                fpt.Create(args[0]);
                return;
            }
            else Console.WriteLine("Формат не поддерживается.");
        }
    }
}
