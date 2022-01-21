using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificacionVencidosServices.Clases
{
    public class cError : System.Web.UI.Page
    {
        public void errorMessage(string message)
        {
            StreamWriter sWErrorMess = new StreamWriter(Server.MapPath("~/Archivos/Error/Error.txt"), true);

            try
            {
                sWErrorMess.WriteLine(message);
                sWErrorMess.WriteLine(Convert.ToString(DateTime.Now));
                sWErrorMess.Flush();
                sWErrorMess.Close();
            }
            catch
            {
                sWErrorMess.Flush();
                sWErrorMess.Close();
            }
        }
    }
}
