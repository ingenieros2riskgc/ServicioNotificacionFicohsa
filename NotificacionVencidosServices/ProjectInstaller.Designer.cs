
namespace NotificacionVencidosServices
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.servicioNotificacionProcessIntaler = new System.ServiceProcess.ServiceProcessInstaller();
            this.NotificacionServicioInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // servicioNotificacionProcessIntaler
            // 
            this.servicioNotificacionProcessIntaler.Account = System.ServiceProcess.ServiceAccount.LocalService;
            this.servicioNotificacionProcessIntaler.Password = null;
            this.servicioNotificacionProcessIntaler.Username = null;
            // 
            // NotificacionServicioInstaller
            // 
            this.NotificacionServicioInstaller.Description = "este servicio valida loe eventos vencidos y proximos a vencer";
            this.NotificacionServicioInstaller.ServiceName = "NotificacionVencidosServices";
            this.NotificacionServicioInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.servicioNotificacionProcessIntaler,
            this.NotificacionServicioInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller servicioNotificacionProcessIntaler;
        private System.ServiceProcess.ServiceInstaller NotificacionServicioInstaller;
    }
}