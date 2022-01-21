using NotificacionVencidosServices.Clases;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace NotificacionVencidosServices
{
    partial class NotificacionVencidosServices : ServiceBase
    {
        private cError cError = new cError();
        private cDataBase cDataBase = new cDataBase();
        public NotificacionVencidosServices()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            lapso.Start(); // TODO: agregar código aquí para iniciar el servicio.
        }

        protected override void OnStop()
        {
            lapso.Stop(); // TODO: agregar código aquí para realizar cualquier anulación necesaria para detener el servicio.
        }

        private void lapso_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            string datetime = DateTime.Now.ToString("hh");

            if (datetime == "07")
            {
                funcionprincipal();
            }
            else
            {
                EventLog.WriteEntry("No es hora de ejecucion", EventLogEntryType.Information);

            }

        }

        private void funcionprincipal()
        {
            DataTable ProximosVence = new DataTable();
            ProximosVence = loadInfoEventos();

            if (ProximosVence.Rows.Count > 0)
            {
                for (int rows = 0; rows < ProximosVence.Rows.Count; rows++)

                {
                    DataTable CorreosEnnviados = new DataTable();
                    CorreosEnnviados = BuscaCorreoEnviado(ProximosVence.Rows[rows]["CodigoPlan"].ToString());

                    if (CorreosEnnviados.Rows.Count <= 0)
                    {
                        string idPadre = ProximosVence.Rows[rows]["IdPadre"].ToString();
                        string idHijo = ProximosVence.Rows[rows]["IdHijo"].ToString();
                        string CodigoPlan = ProximosVence.Rows[rows]["CodigoPlan"].ToString();
                        string NombrePlan = ProximosVence.Rows[rows]["NombrePlan"].ToString();
                        string NomResponsable = ProximosVence.Rows[rows]["NombreResponsable"].ToString();
                        string FechaCompromiso = ProximosVence.Rows[rows]["FechaCompromiso"].ToString();
                        string FechaExtension = ProximosVence.Rows[rows]["FechaExtension"].ToString();

                        NotificarProximosVencer(idHijo, CodigoPlan, NombrePlan, FechaCompromiso, FechaExtension, NomResponsable);
                    }                    

                }

                EventLog.WriteEntry("Notifico Proximos a vencer", EventLogEntryType.Information);
            }
            else
            {

                EventLog.WriteEntry("No encontro Proximos a vencer", EventLogEntryType.Information);
            }

            DataTable Vencidos = new DataTable();
            Vencidos = loadInfoEventosVencidos();

            if (Vencidos.Rows.Count > 0)
            {
                // NotificarVencidos(Vencidos);
                CambiaEstadoVencidos(Vencidos);
                NotificarVencidos(Vencidos);
                EventLog.WriteEntry("Notifico Eventos Vencidos", EventLogEntryType.Information);
            }
            else
            {

                EventLog.WriteEntry("No encontro Eventos Vencidos", EventLogEntryType.Information);
            }


        }

        private void CambiaEstadoVencidos(DataTable Vencidos)
        {
            for (int rows = 0; rows < Vencidos.Rows.Count; rows++)

            {
                cambiaestado(Vencidos.Rows[rows]["CodigoPlan"].ToString());
            }


        }

        private void cambiaestado(string Codigoplan)
        {
            string strConsulta = "";

            try
            {
                strConsulta = "UPDATE Riesgos.planes SET [Estado]= '4. Vencido' " +
                   "Where CodigoPlan='" + Codigoplan + "'";

                cDataBase.conectar();
                cDataBase.ejecutarQuery(strConsulta);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("genero error: " + ex.Message + "en consulta: " + strConsulta, EventLogEntryType.Error);
            }
            finally
            {
                cDataBase.desconectar();
            }


        }


        private void NotificarVencidos(DataTable campos)
        {

            for (int rows = 0; rows < campos.Rows.Count; rows++)
            {
                string idPadre = campos.Rows[rows]["IdPadre"].ToString();
                string idHijo = campos.Rows[rows]["IdHijo"].ToString();
                string CodigoPlan = campos.Rows[rows]["CodigoPlan"].ToString();
                string NombrePlan = campos.Rows[rows]["NombrePlan"].ToString();
                string NomResponsable = campos.Rows[rows]["NombreResponsable"].ToString();
                string FechaCompromiso = campos.Rows[rows]["FechaCompromiso"].ToString();
                string FechaExtension = campos.Rows[rows]["FechaExtension"].ToString();
                string CuerpoCorreo = string.Empty;

                CuerpoCorreo = "<B> Código del plan: </B>" + CodigoPlan +
                    "<br /><B> Nombre del plan: </B>" + NombrePlan +
                    "<br /><B> Fecha de Compromiso: </B>" + FechaCompromiso +
                    "<br /><B> Fecha de Extensión: </B>" + FechaExtension +
                    "<br /><B> Responsable: </B>" + NomResponsable +
                    "<br />" +
                    "<br /><B> El plan de acción se vencio. </B>" +
                    "<br />";
                EnviarNotificacion(23, 0, Convert.ToInt32(idHijo), "", CuerpoCorreo, CodigoPlan);
            }


        }



        private void NotificarProximosVencer(string idHijo, string CodigoPlan, string NombrePlan, string FechaCompromiso, string FechaExtension, string NomResponsable)
        {


            string CuerpoCorreo = string.Empty;

            CuerpoCorreo = "<B> Código del plan: </B>" + CodigoPlan +
                "<br /><B> Nombre del plan: </B>" + NombrePlan +
                "<br /><B> Fecha de Compromiso: </B>" + FechaCompromiso +
                "<br /><B> Fecha de Extensión: </B>" + FechaExtension +
                "<br /><B> Responsable: </B>" + NomResponsable +
                "<br />" +
                "<br /><B> El plan de acción cumple hoy con la fecha de compromiso y/o extensión. </B>" +
                "<br />";
            EnviarNotificacion(23, 0, Convert.ToInt32(idHijo), "", CuerpoCorreo, CodigoPlan);


        }


        private bool EnviarNotificacion(int idEvento, int idRegistro, int idNodoJerarquia, string FechaFinal, string textoAdicional, string codplan)
        {
            bool err = false;
            string Destinatario = "", Copia = "", Asunto = "", Otros = "", Cuerpo = "", NroDiasRecordatorio = "";
            string selectCommand = "", AJefeInmediato = "", AJefeMediato = "", RequiereFechaCierre = "";
            string idJefeInmediato = "", idJefeMediato = "";
            //string conString = WebConfigurationManager.ConnectionStrings["SarlaftConnectionString"].ConnectionString;

            ConnectionStringSettings conString =
       ConfigurationManager.ConnectionStrings["SarlaftConnectionString"];

            try
            {
                //Consulta la informacion basica necesario para enviar el correo de la tabla correos destinatarios
                selectCommand = "SELECT CD.Copia,CD.Otros,CD.Asunto,CD.Cuerpo,CD.NroDiasRecordatorio,CD.AJefeInmediato,CD.AJefeMediato,E.RequiereFechaCierre FROM [Notificaciones].[CorreosDestinatarios] AS CD, [Notificaciones].[Evento] AS E WHERE E. IdEvento = '" + idEvento + "' AND CD.IdEvento = E.IdEvento";
                SqlDataAdapter dad = new SqlDataAdapter(selectCommand, conString.ToString());
                DataTable dtblDiscuss = new DataTable();
                dad.Fill(dtblDiscuss);
                DataView view = new DataView(dtblDiscuss);

                foreach (DataRowView row in view)
                {
                    Copia = row["Copia"].ToString().Trim();
                    Otros = row["Otros"].ToString().Trim();
                    Asunto = row["Asunto"].ToString().Trim();
                    Cuerpo = textoAdicional + "<br />***Nota: " + row["Cuerpo"].ToString().Trim();
                    NroDiasRecordatorio = row["NroDiasRecordatorio"].ToString().Trim();
                    AJefeInmediato = row["AJefeInmediato"].ToString().Trim();
                    AJefeMediato = row["AJefeMediato"].ToString().Trim();
                    RequiereFechaCierre = row["RequiereFechaCierre"].ToString().Trim();
                }

                //Consulta el correo del Destinatario segun el nodo de la Jerarquia Organizacional
                selectCommand = "SELECT DJ.CorreoResponsable, JO.idPadre FROM [Parametrizacion].[JerarquiaOrganizacional] AS JO, [Parametrizacion].[DetalleJerarquiaOrg] AS DJ WHERE JO.idHijo = '" + idNodoJerarquia + "' AND DJ.idHijo = JO.idHijo";
                dad = new SqlDataAdapter(selectCommand, conString.ToString());
                dtblDiscuss.Clear();
                dad.Fill(dtblDiscuss);
                view = new DataView(dtblDiscuss);

                foreach (DataRowView row in view)
                {
                    Destinatario = row["CorreoResponsable"].ToString().Trim();
                    idJefeInmediato = row["idPadre"].ToString().Trim();
                }

                //Consulta el correo del Jefe Inmediato
                if (AJefeInmediato == "SI")
                {
                    selectCommand = "SELECT DJ.CorreoResponsable, JO.idPadre FROM [Parametrizacion].[JerarquiaOrganizacional] AS JO, [Parametrizacion].[DetalleJerarquiaOrg] AS DJ WHERE JO.idHijo = '" + idJefeInmediato + "' AND DJ.idHijo = JO.idHijo";
                    dad = new SqlDataAdapter(selectCommand, conString.ToString());
                    dtblDiscuss.Clear();
                    dad.Fill(dtblDiscuss);
                    view = new DataView(dtblDiscuss);

                    foreach (DataRowView row in view)
                    {
                        Destinatario = Destinatario + ";" + row["CorreoResponsable"].ToString().Trim();
                        idJefeMediato = row["idPadre"].ToString().Trim();
                    }
                }

                //Consulta el correo del Jefe Mediato
                if (AJefeMediato == "SI")
                {
                    selectCommand = "SELECT DJ.CorreoResponsable, JO.idPadre FROM [Parametrizacion].[JerarquiaOrganizacional] AS JO, [Parametrizacion].[DetalleJerarquiaOrg] AS DJ WHERE JO.idHijo = '" + idJefeMediato + "' AND DJ.idHijo = JO.idHijo";
                    dad = new SqlDataAdapter(selectCommand, conString.ToString());
                    dtblDiscuss.Clear();
                    dad.Fill(dtblDiscuss);
                    view = new DataView(dtblDiscuss);

                    foreach (DataRowView row in view)
                    {
                        Destinatario = Destinatario + ";" + row["CorreoResponsable"].ToString().Trim();
                    }
                }

                InsertarCorreo(Destinatario, Copia, Otros, Asunto, Cuerpo, idEvento.ToString().Trim(), idRegistro.ToString().Trim(), codplan);

                //Insertar el Registro en la tabla de Correos Enviados
                /*  SqlDataSource200.InsertParameters["Destinatario"].DefaultValue = Destinatario.Trim();
                  SqlDataSource200.InsertParameters["Copia"].DefaultValue = Copia;
                  SqlDataSource200.InsertParameters["Otros"].DefaultValue = Otros;
                  SqlDataSource200.InsertParameters["Asunto"].DefaultValue = Asunto;
                  SqlDataSource200.InsertParameters["Cuerpo"].DefaultValue = Cuerpo;
                  SqlDataSource200.InsertParameters["Estado"].DefaultValue = "POR ENVIAR";
                  SqlDataSource200.InsertParameters["Tipo"].DefaultValue = "CIERRE";
                  SqlDataSource200.InsertParameters["FechaEnvio"].DefaultValue = "";
                  SqlDataSource200.InsertParameters["IdEvento"].DefaultValue = idEvento.ToString().Trim();
                  SqlDataSource200.InsertParameters["IdRegistro"].DefaultValue = idRegistro.ToString().Trim();
                  SqlDataSource200.InsertParameters["IdUsuario"].DefaultValue = Session["idUsuario"].ToString().Trim(); //Aca va el id del Usuario de la BD
                  SqlDataSource200.InsertParameters["FechaRegistro"].DefaultValue = System.DateTime.Now.ToString().Trim();
                  SqlDataSource200.Insert();*/
            }
            catch (Exception except)
            {

                EventLog.WriteEntry("genero error insertando a base de datos Correos " + except.Message, EventLogEntryType.Error);
                // Handle the Exception.
                //omb.ShowMessage("Error en el envío de la notificación." + "<br/>" + "Descripción: " + except.Message.ToString().Trim(), 1, "Atención");
                err = true;
            }

            if (!err)
            {
                // Si no existe error en la creacion del registro en el log de correos enviados se procede a escribir en la tabla CorreosRecordatorios y a enviar el correo 
                if (RequiereFechaCierre == "SI" && FechaFinal != "")
                {

                    InsertarCorreoRecordatorio(NroDiasRecordatorio, FechaFinal);
                    //Si los NroDiasRecordatorio es diferente de vacio se inserta el registro correspondiente en la tabla CorreosRecordatorio
                    /*   SqlDataSource201.InsertParameters["IdCorreosEnviados"].DefaultValue = LastInsertIdCE.ToString().Trim();
                        SqlDataSource201.InsertParameters["NroDiasRecordatorio"].DefaultValue = NroDiasRecordatorio;
                        SqlDataSource201.InsertParameters["Estado"].DefaultValue = "POR ENVIAR";
                        SqlDataSource201.InsertParameters["FechaFinal"].DefaultValue = FechaFinal;
                        SqlDataSource201.InsertParameters["IdUsuario"].DefaultValue = Session["idUsuario"].ToString().Trim(); //Aca va el id del Usuario de la BD
                        SqlDataSource201.InsertParameters["FechaRegistro"].DefaultValue = System.DateTime.Now.ToString().Trim();
                        SqlDataSource201.Insert();*/
                }

                try
                {
                    MailMessage message = new MailMessage();
                    SmtpClient smtpClient = new SmtpClient();
                    MailAddress fromAddress = new MailAddress(((System.Net.NetworkCredential)(smtpClient.Credentials)).UserName, "Software Sherlock");
                    //MailAddress fromAddress = new MailAddress("sherlock @riskconsultingcolombia.com", "Software Sherlock");
                    message.From = fromAddress;//here you can set address

                    #region
                    foreach (string substr in Destinatario.Split(';'))
                    {
                        if (!string.IsNullOrEmpty(substr.Trim()))
                        {
                            message.To.Add(substr);
                        }
                    }
                    #endregion

                    #region
                    if (Copia.Trim() != "")
                    {
                        foreach (string substr in Copia.Split(';'))
                        {
                            if (!string.IsNullOrEmpty(substr.Trim()))
                            {
                                message.CC.Add(substr);
                            }
                        }
                    }
                    #endregion

                    #region
                    if (Otros.Trim() != "")
                    {
                        foreach (string substr in Otros.Split(';'))
                        {
                            if (!string.IsNullOrEmpty(substr.Trim()))
                            {
                                message.CC.Add(substr);
                            }
                        }
                    }
                    #endregion

                    message.Subject = Asunto;//subject of email
                    message.IsBodyHtml = true;//To determine email body is html or not
                    message.Body = Cuerpo;

                    smtpClient.Send(message);
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry("genero error en el envio de correo: " + ex.Message, EventLogEntryType.Error);
                    //throw exception here you can write code to handle exception here
                    //omb.ShowMessage("Error en el envío de la notificación." + "<br/>" + "Descripción: " + ex.Message.ToString().Trim(), 1, "Atención");
                    err = true;
                }

                if (!err)
                {
                    ActualizaEstadoCoreo();
                    //Actualiza el Estado del Correo Enviado
                    /* SqlDataSource200.UpdateParameters["IdCorreosEnviados"].DefaultValue = LastInsertIdCE.ToString().Trim();
                     SqlDataSource200.UpdateParameters["Estado"].DefaultValue = "ENVIADO";
                     SqlDataSource200.UpdateParameters["FechaEnvio"].DefaultValue = System.DateTime.Now.ToString().Trim();
                     SqlDataSource200.Update();*/
                }
            }

            return (err);
        }



        public DataTable loadInfoEventos()
        {
            DataTable dtInformacion = new DataTable();
            string condicion = string.Empty;
            string CondicionOtroFiltro = string.Empty;
            try
            {



                CondicionOtroFiltro = "select distinct PDJ.idHijo, PJO.idPadre, RP.NombrePlan, RP.CodigoPlan, RP.FechaCompromiso, RP.FechaExtension, " +
            "PDJ.NombreResponsable, PDJ.CorreoResponsable " +
            "from Riesgos.planes RP " +
            "LEFT JOIN Parametrizacion.DetalleJerarquiaOrg PDJ ON RP.IdResponsable = PDJ.idHijo " +
            "LEFT JOIN Parametrizacion.JerarquiaOrganizacional PJO ON PDJ.idHijo = PJO.idHijo " +
            "where CONVERT(date,RP.FechaCompromiso) = CONVERT (date, GETDATE()) " +
            "OR CONVERT(date,RP.FechaExtension) = CONVERT (date, GETDATE())";


                #region consulta
                cDataBase.conectar();
                dtInformacion = cDataBase.ejecutarConsulta(CondicionOtroFiltro);
                #endregion Consulta

                cDataBase.desconectar();
                return dtInformacion;
            }
            catch (Exception ex)
            {

                cDataBase.desconectar();
                EventLog.WriteEntry("genero error: " + ex.Message + "en consulta: " + CondicionOtroFiltro, EventLogEntryType.Error);

                throw new Exception(ex.Message + CondicionOtroFiltro);
            }
        }

        public DataTable BuscaCorreoEnviado(string codPlan)
        {
            DataTable dtInformacion = new DataTable();
            string condicion = string.Empty;
            string CondicionOtroFiltro = string.Empty;
            try
            {
                DateTime fechaHoraActual = System.DateTime.Now;
                string fecharegistro = fechaHoraActual.ToShortDateString();
                int Mes = fechaHoraActual.Month;
                int year = fechaHoraActual.Year;
                int day = fechaHoraActual.Day;



                CondicionOtroFiltro = "select * from [Notificaciones].[CorreosEnviados] where MONTH(FechaRegistro)=" + Mes + " AND YEAR(FechaRegistro)=" + year + " and DAY(FechaRegistro)=" + day + " and Tipo='CIERRE' and CodigoPlan='" + codPlan + "'";


                #region consulta
                cDataBase.conectar();
                dtInformacion = cDataBase.ejecutarConsulta(CondicionOtroFiltro);
                #endregion Consulta

                cDataBase.desconectar();
                return dtInformacion;
            }
            catch (Exception ex)
            {

                cDataBase.desconectar();
                EventLog.WriteEntry("genero error: " + ex.Message + "en consulta: " + CondicionOtroFiltro, EventLogEntryType.Error);

                throw new Exception(ex.Message + CondicionOtroFiltro);
            }
        }



        public DataTable loadInfoEventosVencidos()
        {
            DataTable dtInformacion = new DataTable();
            string condicion = string.Empty;
            string CondicionOtroFiltro = string.Empty;
            try
            {

                CondicionOtroFiltro = "select distinct PDJ.idHijo, PJO.idPadre, RP.NombrePlan, RP.CodigoPlan, RP.FechaCompromiso, RP.FechaExtension, PDJ.NombreResponsable, PDJ.CorreoResponsable " +
            "from Riesgos.planes RP " +
            "LEFT JOIN Parametrizacion.DetalleJerarquiaOrg PDJ ON RP.IdResponsable = PDJ.idHijo " +
            "LEFT JOIN Parametrizacion.JerarquiaOrganizacional PJO ON PDJ.idHijo = PJO.idHijo " +
            "where RP.Estado='2. En proceso' or RP.Estado='1. Pendiente iniciar' or RP.Estado='3. En espera de reso' AND CONVERT(date,RP.FechaCompromiso) < CONVERT (date, GETDATE())";


                #region consulta
                cDataBase.conectar();
                dtInformacion = cDataBase.ejecutarConsulta(CondicionOtroFiltro);
                #endregion Consulta

                cDataBase.desconectar();
                return dtInformacion;
            }
            catch (Exception ex)
            {

                cDataBase.desconectar();
                EventLog.WriteEntry("genero error: " + ex.Message + "en consulta: " + CondicionOtroFiltro, EventLogEntryType.Error);

                throw new Exception(ex.Message + CondicionOtroFiltro);
            }
        }



        private void InsertarCorreo(string Destinatario, string Copia, string Otros, string Asunto, string cuerpo, string idEvento, string idRegistro, string codPlan)
        {

            string strConsulta = "";

            DateTime fechaHoraActual = System.DateTime.Now;
            string fecharegistro = fechaHoraActual.ToShortDateString();


            //string fecharegistro = System.DateTime.Now.ToString("yyyy-MM-ss HH:mm:ss");
            try
            {

                strConsulta = "INSERT INTO [Notificaciones].[CorreosEnviados] ([IdEvento], [Destinatario], [Copia], [Otros], [Asunto], [Cuerpo], [Estado], [IdRegistro], [FechaEnvio], [FechaRegistro], [IdUsuario], [Tipo], [CodigoPlan]) " +
                "VALUES ('" + idEvento + "', '" + Destinatario + "', '" + Copia + "', '" + Otros + "', '" + Asunto + "', '" + cuerpo + "', 'POR ENVIAR', '" + idRegistro + "', '', getdate(), '1','CIERRE','"+ codPlan + "')";

                //strConsulta = "INSERT INTO [Notificaciones].[CorreosEnviados] ([IdEvento], [Destinatario], [Copia], [Otros], [Asunto], [Cuerpo], [Estado], [IdRegistro], [FechaEnvio], [FechaRegistro], [IdUsuario], [Tipo]) " +
                //    "VALUES ('" + idEvento + "', '" + Destinatario + "', '" + Copia + "', '" + Otros + "', '" + Asunto + "', '" + cuerpo + "', 'POR ENVIAR', '" + idRegistro + "', '', " + fecharegistro + ", '1','CIERRE')";

                cDataBase.conectar();
                cDataBase.ejecutarQuery(strConsulta);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("genero error: " + ex.Message + "en consulta: " + strConsulta, EventLogEntryType.Error);
            }
            finally
            {
                cDataBase.desconectar();
            }


        }


        private void InsertarCorreoRecordatorio(string NroDiasRecordatorio, string FechaFinal)
        {

            string strConsulta = "";
            DateTime fechaHoraActual = System.DateTime.Now;
            string fecharegistro = fechaHoraActual.ToShortDateString();

            //string fecharegistro = System.DateTime.Now.ToString("yyyy-MM-ss HH:mm:ss");
            try
            {

                strConsulta = "INSERT INTO [Notificaciones].[CorreosRecordatorio] ([IdCorreosEnviados], [NroDiasRecordatorio], [FechaFinal], [Estado], [FechaRegistro], [IdUsuario])) " +
              "VALUES ('(select IDENT_CURRENT('Notificaciones.CorreosEnviados'))', '" + NroDiasRecordatorio + "', '" + FechaFinal + "', 'POR ENVIAR', getdate(), '1')";

                //strConsulta = "INSERT INTO [Notificaciones].[CorreosRecordatorio] ([IdCorreosEnviados], [NroDiasRecordatorio], [FechaFinal], [Estado], [FechaRegistro], [IdUsuario])) " +
                //    "VALUES ('(select IDENT_CURRENT('Notificaciones.CorreosEnviados'))', '" + NroDiasRecordatorio + "', '" + FechaFinal + "', 'POR ENVIAR', " + fecharegistro + ", '1')";

                cDataBase.conectar();
                cDataBase.ejecutarQuery(strConsulta);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("genero error: " + ex.Message + "en consulta: " + strConsulta, EventLogEntryType.Error);
            }
            finally
            {
                cDataBase.desconectar();
            }


        }

        private void ActualizaEstadoCoreo()
        {

            string strConsulta = "";

            DateTime fechaHoraActual = System.DateTime.Now;
            string FechaEnvio = fechaHoraActual.ToShortDateString();
            try
            {
                strConsulta = "UPDATE [Notificaciones].[CorreosEnviados] SET [FechaEnvio] = getdate()," +
                   "[Estado] ='ENVIADO' " +
                   "WHERE [IdCorreosEnviados] =(select IDENT_CURRENT('Notificaciones.CorreosEnviados'))"
                   ;
                //strConsulta = "UPDATE [Notificaciones].[CorreosEnviados] SET [FechaEnvio] =" + FechaEnvio + "," +
                //    "[Estado] ='ENVIADO' " +
                //    "WHERE [IdCorreosEnviados] =(select IDENT_CURRENT('Notificaciones.CorreosEnviados'))"
                //    ;

                cDataBase.conectar();
                cDataBase.ejecutarQuery(strConsulta);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("genero error: " + ex.Message + "en consulta: " + strConsulta, EventLogEntryType.Error);
            }
            finally
            {
                cDataBase.desconectar();
            }

        }
    }
}
