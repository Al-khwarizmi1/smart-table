namespace ArduinoService
{
    partial class ArduinoService
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SmartTableArduinoService = new System.ServiceProcess.ServiceController();
            this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
            // 
            // serialPort1
            // 
            this.serialPort1.PortName = "COM3";
            // 
            // ArduinoService
            // 
            this.ServiceName = "Service1";

        }

        #endregion

        private System.ServiceProcess.ServiceController SmartTableArduinoService;
        private System.IO.Ports.SerialPort serialPort1;
    }
}
