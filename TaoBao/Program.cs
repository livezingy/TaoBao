/* --------------------------------------------------------
 * author：livezingy
 * 
 * BLOG：http://www.livezingy.com
 * 
 * Development environment：
 *      Visual Studio V2013
 * Revision History：
   
--------------------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaoBao
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
