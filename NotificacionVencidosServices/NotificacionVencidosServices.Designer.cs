
namespace NotificacionVencidosServices
{
    partial class NotificacionVencidosServices
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
            this.lapso = new System.Timers.Timer();
            ((System.ComponentModel.ISupportInitialize)(this.lapso)).BeginInit();
            // 
            // lapso
            // 
            this.lapso.Enabled = true;
            this.lapso.Interval = 120000D;
            this.lapso.Elapsed += new System.Timers.ElapsedEventHandler(this.lapso_Elapsed);
            // 
            // NotificacionVencidosServices
            // 
            this.ServiceName = "NotificacionVencidosServices";
            ((System.ComponentModel.ISupportInitialize)(this.lapso)).EndInit();

        }

        #endregion

        private System.Timers.Timer lapso;
    }
}
